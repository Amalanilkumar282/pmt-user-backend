using BACKEND_CQRS.Domain.Dto.AI;
using BACKEND_CQRS.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Infrastructure.Services
{
    public class GeminiAIService : IGeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiAIService> _logger;
        private const int MAX_RETRIES = 3;

        public GeminiAIService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GeminiAIService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<GeminiSprintPlanResponseDto> GenerateSprintPlanAsync(SprintPlanningContextDto context)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Gemini API key is not configured");
            }

            // Build the comprehensive prompt
            var prompt = BuildSprintPlanningPrompt(context);

            _logger.LogInformation("Calling Gemini API to generate sprint plan");
            _logger.LogDebug($"Prompt length: {prompt.Length} characters");

            // Try up to MAX_RETRIES times
            for (int attempt = 1; attempt <= MAX_RETRIES; attempt++)
            {
                try
                {
                    var response = await CallGeminiAPIAsync(apiKey, prompt);

                    if (response?.SprintPlan != null &&
                        response.SprintPlan.SelectedIssues != null &&
                        response.SprintPlan.SelectedIssues.Any())
                    {
                        // Validate that all returned issue IDs exist in the backlog
                        var backlogIssueIds = context.BacklogIssues?.Select(i => i.Id).ToHashSet() ?? new HashSet<Guid>();
                        var invalidIssues = response.SprintPlan.SelectedIssues
                            .Where(si => !backlogIssueIds.Contains(si.IssueId))
                            .ToList();

                        if (invalidIssues.Any())
                        {
                            _logger.LogWarning($"Gemini returned {invalidIssues.Count} issues with IDs not in backlog: {string.Join(", ", invalidIssues.Select(i => i.IssueId))}");
                            // Remove invalid issues from the response
                            response.SprintPlan.SelectedIssues = response.SprintPlan.SelectedIssues
                                .Where(si => backlogIssueIds.Contains(si.IssueId))
                                .ToList();

                            // Recalculate total story points
                            response.SprintPlan.TotalStoryPoints = response.SprintPlan.SelectedIssues.Sum(i => i.StoryPoints);
                        }

                        if (response.SprintPlan.SelectedIssues.Any())
                        {
                            _logger.LogInformation($"Successfully generated sprint plan with {response.SprintPlan.SelectedIssues.Count} valid issues");
                            _logger.LogDebug($"Selected issue IDs: {string.Join(", ", response.SprintPlan.SelectedIssues.Select(i => i.IssueId))}");
                            return response;
                        }
                        else
                        {
                            _logger.LogWarning($"All selected issues had invalid IDs (attempt {attempt}/{MAX_RETRIES})");
                        }
                    }
                    else
                    {
                        _logger.LogWarning($"Gemini returned empty sprint plan (attempt {attempt}/{MAX_RETRIES})");
                    }

                    if (attempt < MAX_RETRIES)
                    {
                        // Wait before retrying
                        await Task.Delay(1000 * attempt);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error calling Gemini API (attempt {attempt}/{MAX_RETRIES})");

                    if (attempt == MAX_RETRIES)
                    {
                        throw;
                    }

                    await Task.Delay(1000 * attempt);
                }
            }

            // If all retries failed, use fallback logic
            _logger.LogWarning("All Gemini API attempts failed, using fallback rule-based planning");
            return GenerateFallbackSprintPlan(context);
        }

        private async Task<GeminiSprintPlanResponseDto> CallGeminiAPIAsync(string apiKey, string prompt)
        {
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 4096,
                    responseMimeType = "application/json"
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);

            // Log the COMPLETE JSON request being sent to Gemini
            _logger.LogInformation($"===== COMPLETE GEMINI API REQUEST =====");
            _logger.LogInformation($"{jsonRequest}");
            _logger.LogInformation($"===== END GEMINI REQUEST =====");

            // Also log just the prompt text for easy review
            _logger.LogInformation($"===== PROMPT TEXT ONLY =====");
            _logger.LogInformation($"{prompt}");
            _logger.LogInformation($"===== END PROMPT =====");

            // Keep debug log for backward compatibility (truncated)
            _logger.LogDebug($"Gemini Request: {jsonRequest.Substring(0, Math.Min(500, jsonRequest.Length))}...");

            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(endpoint, content);
            var responseText = await response.Content.ReadAsStringAsync();

            _logger.LogInformation($"===== COMPLETE GEMINI API RESPONSE =====");
            _logger.LogInformation($"Status Code: {response.StatusCode}");
            _logger.LogInformation($"Response Body: {responseText}");
            _logger.LogInformation($"===== END GEMINI RESPONSE =====");

            _logger.LogDebug($"Gemini Response Status: {response.StatusCode}");
            _logger.LogDebug($"Gemini Response (first 1000 chars): {responseText.Substring(0, Math.Min(1000, responseText.Length))}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Gemini API error: {responseText}");
                throw new HttpRequestException($"Gemini API returned {response.StatusCode}: {responseText}");
            }

            // Parse Gemini response
            var geminiResponse = JsonSerializer.Deserialize<GeminiApiResponse>(responseText);

            if (geminiResponse?.Candidates == null || !geminiResponse.Candidates.Any())
            {
                _logger.LogWarning("Gemini response has no candidates");
                return new GeminiSprintPlanResponseDto();
            }

            var candidate = geminiResponse.Candidates.First();
            if (candidate?.Content?.Parts == null || !candidate.Content.Parts.Any())
            {
                _logger.LogWarning("Gemini response has no content parts");
                return new GeminiSprintPlanResponseDto();
            }

            var textResponse = candidate.Content.Parts.First().Text;

            if (string.IsNullOrWhiteSpace(textResponse))
            {
                _logger.LogWarning("Gemini response text is empty");
                return new GeminiSprintPlanResponseDto();
            }

            _logger.LogDebug($"Gemini text response: {textResponse}");

            // Clean up the response (remove markdown code fences if present)
            textResponse = CleanJsonResponse(textResponse);

            try
            {
                var sprintPlan = JsonSerializer.Deserialize<GeminiSprintPlanResponseDto>(textResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                });

                return sprintPlan ?? new GeminiSprintPlanResponseDto();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Failed to parse Gemini JSON response: {textResponse}");
                throw new InvalidOperationException($"Invalid JSON from Gemini: {ex.Message}");
            }
        }

        private string CleanJsonResponse(string text)
        {
            // Remove markdown code fences
            text = Regex.Replace(text, @"```json\s*", "", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"```\s*$", "", RegexOptions.IgnoreCase);
            text = text.Trim();
            return text;
        }

        private string BuildSprintPlanningPrompt(SprintPlanningContextDto context)
        {
            var prompt = new StringBuilder();

            prompt.AppendLine("You are an expert Agile Sprint Planner AI. Your task is to create an optimal sprint plan based on the provided project data, backlog issues, team velocity, and constraints.");
            prompt.AppendLine();
            prompt.AppendLine("## CRITICAL OUTPUT REQUIREMENTS:");
            prompt.AppendLine("1. You MUST return ONLY valid JSON - no explanatory text, no markdown formatting");
            prompt.AppendLine("2. You MUST select at least 3-10 backlog issues for the sprint");
            prompt.AppendLine("3. You MUST provide a non-empty summary and recommendations");
            prompt.AppendLine("4. For each selected issue, the 'issueId' field MUST be the exact UUID from the backlog issue list");
            prompt.AppendLine("5. Your response must match this exact JSON structure:");
            prompt.AppendLine(@"{
  ""sprintPlan"": {
    ""selectedIssues"": [
      {
        ""issueId"": ""exact-uuid-from-backlog-list"",
        ""issueKey"": ""string like PHX-201"",
        ""storyPoints"": number,
        ""suggestedAssigneeId"": number or null,
        ""rationale"": ""brief explanation why this issue was selected""
      }
    ],
    ""totalStoryPoints"": number,
    ""summary"": ""comprehensive summary of the sprint plan"",
    ""recommendations"": [
      {
        ""type"": ""capacity|priority|risk|dependency|team_balance"",
        ""severity"": ""info|warning|critical"",
        ""message"": ""recommendation text""
      }
    ],
    ""capacityAnalysis"": {
      ""teamCapacityUtilization"": number (percentage),
      ""estimatedCompletionProbability"": number (percentage),
      ""riskFactors"": [""string""]
    }
  }
}");
            prompt.AppendLine();
            prompt.AppendLine("## PROJECT CONTEXT:");
            prompt.AppendLine($"- Project: {context.Project?.Name} ({context.Project?.Key})");
            prompt.AppendLine($"- Sprint: {context.NewSprint?.Name}");
            prompt.AppendLine($"- Sprint Goal: {context.NewSprint?.Goal ?? "Not specified"}");
            prompt.AppendLine($"- Sprint Duration: {context.NewSprint?.StartDate:yyyy-MM-dd} to {context.NewSprint?.DueDate:yyyy-MM-dd}");
            prompt.AppendLine($"- Target Story Points: {context.NewSprint?.TargetStoryPoints ?? 0}");
            prompt.AppendLine();

            // Team velocity information
            if (context.TeamVelocity != null)
            {
                prompt.AppendLine("## TEAM VELOCITY:");
                prompt.AppendLine($"- Team: {context.TeamVelocity.TeamName}");
                prompt.AppendLine($"- Team Size: {context.TeamVelocity.MemberCount} members");
                prompt.AppendLine($"- Average Velocity: {context.TeamVelocity.AverageVelocity:F2} story points per sprint");
                prompt.AppendLine($"- Velocity Trend: {context.TeamVelocity.RecentVelocityTrend}");

                if (context.TeamVelocity.HistoricalSprints?.Any() == true)
                {
                    prompt.AppendLine($"- Recent Sprint Performance:");
                    foreach (var sprint in context.TeamVelocity.HistoricalSprints.Take(5))
                    {
                        prompt.AppendLine($"  * {sprint.SprintName}: {sprint.CompletedPoints}/{sprint.PlannedPoints} points ({sprint.CompletionRate:F0}% completion)");
                    }
                }
            }
            prompt.AppendLine();

            // Backlog issues
            prompt.AppendLine("## AVAILABLE BACKLOG ISSUES:");
            prompt.AppendLine($"Total backlog issues: {context.BacklogIssues?.Count ?? 0}");
            prompt.AppendLine();
            prompt.AppendLine("IMPORTANT: Each issue has a unique 'id' field. When you return the list of issues to be included in the sprint,");
            prompt.AppendLine("you MUST include this exact same 'id' for each selected issue. Do NOT alter, regenerate, or modify these IDs.");
            prompt.AppendLine("The 'issueId' in your response must exactly match the 'id' from this list.");
            prompt.AppendLine();

            if (context.BacklogIssues?.Any() == true)
            {
                foreach (var issue in context.BacklogIssues.Take(50)) // Limit to avoid token limits
                {
                    prompt.AppendLine($"- Issue ID: {issue.Id}");
                    prompt.AppendLine($"  * Key: {issue.Key}");
                    prompt.AppendLine($"  * Title: \"{issue.Title}\"");
                    prompt.AppendLine($"  * Type: {issue.Type}, Priority: {issue.Priority}");
                    prompt.AppendLine($"  * Story Points: {issue.StoryPoints ?? 0}");
                    if (issue.AssigneeId.HasValue)
                    {
                        prompt.AppendLine($"  * Assigned to: User ID {issue.AssigneeId}");
                    }
                    if (issue.Labels?.Any() == true)
                    {
                        prompt.AppendLine($"  * Labels: {string.Join(", ", issue.Labels)}");
                    }
                }
            }
            else
            {
                prompt.AppendLine("ERROR: No backlog issues available!");
            }
            prompt.AppendLine();

            // In-progress and planned sprints context
            if (context.InProgressSprints?.Any() == true)
            {
                prompt.AppendLine("## IN-PROGRESS SPRINTS (Same Team):");
                foreach (var sprint in context.InProgressSprints)
                {
                    prompt.AppendLine($"- {sprint.SprintName}: {sprint.AllocatedPoints} points allocated, {sprint.RemainingPoints} points remaining, Due: {sprint.DueDate:yyyy-MM-dd}");
                }
                prompt.AppendLine();
            }

            if (context.PlannedSprints?.Any() == true)
            {
                prompt.AppendLine("## PLANNED SPRINTS (Same Team):");
                foreach (var sprint in context.PlannedSprints)
                {
                    prompt.AppendLine($"- {sprint.SprintName}: {sprint.AllocatedPoints} points allocated, Start: {sprint.StartDate:yyyy-MM-dd}");
                }
                prompt.AppendLine();
            }

            // Planning constraints
            prompt.AppendLine("## PLANNING CONSTRAINTS & RULES:");
            prompt.AppendLine($"1. Total story points should be close to target ({context.NewSprint?.TargetStoryPoints ?? 0}) but NOT EXCEED average velocity + 20%");
            prompt.AppendLine("2. Prioritize issues in this order: CRITICAL > HIGH > MEDIUM > LOW");
            prompt.AppendLine("3. Balance story point distribution across team members");
            prompt.AppendLine("4. Include a healthy mix of issue types: STORY, TASK, BUG");
            prompt.AppendLine("5. Consider velocity trend:");

            if (context.TeamVelocity?.RecentVelocityTrend == "decreasing")
            {
                prompt.AppendLine("   - Velocity is DECREASING → Reduce sprint load by 10-15%");
            }
            else if (context.TeamVelocity?.RecentVelocityTrend == "increasing")
            {
                prompt.AppendLine("   - Velocity is INCREASING → Can slightly increase sprint load");
            }
            else
            {
                prompt.AppendLine("   - Velocity is STABLE → Plan at average velocity level");
            }

            prompt.AppendLine("6. Assign issues to team members based on their historical performance");
            prompt.AppendLine("7. Identify and flag any capacity or risk concerns");
            prompt.AppendLine();

            prompt.AppendLine("## YOUR TASK:");
            prompt.AppendLine("Analyze the data above and generate a sprint plan. Select the optimal set of backlog issues that:");
            prompt.AppendLine("- Aligns with the team's velocity and capacity");
            prompt.AppendLine("- Prioritizes high-value work");
            prompt.AppendLine("- Balances workload across the team");
            prompt.AppendLine("- Achieves the sprint goal");
            prompt.AppendLine();
            prompt.AppendLine("Return ONLY the JSON sprint plan - no additional text or explanations!");

            return prompt.ToString();
        }

        private GeminiSprintPlanResponseDto GenerateFallbackSprintPlan(SprintPlanningContextDto context)
        {
            _logger.LogInformation("Generating rule-based fallback sprint plan");

            var targetPoints = context.NewSprint?.TargetStoryPoints ??
                               (int)(context.TeamVelocity?.AverageVelocity ?? 0);

            // Adjust target based on velocity trend
            if (context.TeamVelocity?.RecentVelocityTrend == "decreasing")
            {
                targetPoints = (int)(targetPoints * 0.85m); // Reduce by 15%
            }

            // Priority order for sorting
            var priorityOrder = new Dictionary<string, int>
            {
                { "CRITICAL", 1 },
                { "HIGH", 2 },
                { "MEDIUM", 3 },
                { "LOW", 4 }
            };

            // Sort issues by priority and story points
            var sortedIssues = context.BacklogIssues?
                .OrderBy(i => priorityOrder.GetValueOrDefault(i.Priority?.ToUpper() ?? "MEDIUM", 3))
                .ThenByDescending(i => i.StoryPoints ?? 0)
                .ToList() ?? new List<BacklogIssueDto>();

            var selectedIssues = new List<SelectedIssueDto>();
            int totalPoints = 0;

            // Select issues until we reach target
            foreach (var issue in sortedIssues)
            {
                var issuePoints = issue.StoryPoints ?? 0;

                if (totalPoints + issuePoints <= targetPoints || selectedIssues.Count < 3)
                {
                    selectedIssues.Add(new SelectedIssueDto
                    {
                        IssueId = issue.Id,
                        IssueKey = issue.Key,
                        StoryPoints = issuePoints,
                        SuggestedAssigneeId = issue.AssigneeId,
                        Rationale = $"{issue.Priority ?? "MEDIUM"} priority {issue.Type} issue selected for sprint"
                    });
                    totalPoints += issuePoints;
                }

                if (totalPoints >= targetPoints && selectedIssues.Count >= 5)
                {
                    break;
                }
            }

            var utilizationPercent = targetPoints > 0 ? (decimal)totalPoints / targetPoints * 100 : 0;

            // Build recommendations
            var recommendations = new List<RecommendationDto>
            {
                new RecommendationDto
                {
                    Type = "priority",
                    Severity = "info",
                    Message = "This is a fallback plan generated using rule-based logic."
                },
                new RecommendationDto
                {
                    Type = "capacity",
                    Severity = "info",
                    Message = $"Team velocity trend is {context.TeamVelocity?.RecentVelocityTrend} - adjust future sprint planning accordingly."
                }
            };

            // Count critical issues
            var criticalCount = context.BacklogIssues?.Count(i => i.Priority == "CRITICAL" && selectedIssues.Any(s => s.IssueId == i.Id)) ?? 0;

            if (criticalCount > 0)
            {
                recommendations.Add(new RecommendationDto
                {
                    Type = "priority",
                    Severity = "warning",
                    Message = "Sprint includes critical priority items - ensure adequate focus."
                });
            }
            else
            {
                recommendations.Add(new RecommendationDto
                {
                    Type = "priority",
                    Severity = "info",
                    Message = "Consider including high-priority items for maximum value delivery."
                });
            }

            if (utilizationPercent > 110)
            {
                recommendations.Add(new RecommendationDto
                {
                    Type = "capacity",
                    Severity = "warning",
                    Message = "⚠️ Sprint may be overloaded - consider removing low-priority items."
                });
            }
            else if (utilizationPercent < 70)
            {
                recommendations.Add(new RecommendationDto
                {
                    Type = "capacity",
                    Severity = "info",
                    Message = "Sprint has capacity for additional work - consider adding more issues."
                });
            }
            else
            {
                recommendations.Add(new RecommendationDto
                {
                    Type = "capacity",
                    Severity = "info",
                    Message = "Sprint capacity utilization is healthy."
                });
            }

            // Build risk factors
            var riskFactors = new List<string>();
            if (context.TeamVelocity?.RecentVelocityTrend == "decreasing")
            {
                riskFactors.Add("Team velocity is decreasing");
            }
            if (utilizationPercent > 110)
            {
                riskFactors.Add("Sprint overloaded beyond team capacity");
            }
            if (criticalCount > 3)
            {
                riskFactors.Add("High number of critical priority items");
            }

            return new GeminiSprintPlanResponseDto
            {
                SprintPlan = new SprintPlanDto
                {
                    SelectedIssues = selectedIssues,
                    TotalStoryPoints = totalPoints,
                    Summary = $"Rule-based sprint plan generated with {selectedIssues.Count} issues totaling {totalPoints} story points. " +
                             $"Target was {targetPoints} points (Team velocity: {context.TeamVelocity?.AverageVelocity:F1}, Trend: {context.TeamVelocity?.RecentVelocityTrend}).",
                    Recommendations = recommendations,
                    CapacityAnalysis = new CapacityAnalysisDto
                    {
                        TeamCapacityUtilization = (int)utilizationPercent,
                        EstimatedCompletionProbability = Math.Min(95, 100 - Math.Abs((int)(utilizationPercent - 90))),
                        RiskFactors = riskFactors
                    }
                }
            };
        }

        // Internal classes for Gemini API response parsing
        private class GeminiApiResponse
        {
            [JsonPropertyName("candidates")]
            public List<Candidate>? Candidates { get; set; }
        }

        private class Candidate
        {
            [JsonPropertyName("content")]
            public Content? Content { get; set; }
        }

        private class Content
        {
            [JsonPropertyName("parts")]
            public List<Part>? Parts { get; set; }
        }

        private class Part
        {
            [JsonPropertyName("text")]
            public string? Text { get; set; }
        }
    }
}
