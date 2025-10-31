using BACKEND_CQRS.Domain.Dto.AI;
using BACKEND_CQRS.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BACKEND_CQRS.Infrastructure.Services
{
    public class GeminiAIService : IGeminiAIService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GeminiAIService> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const int MaxRetries = 3;

        public GeminiAIService(IConfiguration configuration, ILogger<GeminiAIService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
            _apiKey = _configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini API Key is not configured");
        }

        public async Task<GeminiSprintPlanResponseDto> GenerateSprintPlanAsync(SprintPlanningContextDto context)
        {
            var prompt = BuildGeminiPrompt(context);

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    _logger.LogInformation($"Calling Gemini AI API (Attempt {attempt}/{MaxRetries})");

                    var response = await CallGeminiAPIAsync(prompt);
                    var geminiResponse = ParseGeminiResponse(response);

                    _logger.LogInformation("Successfully received and parsed Gemini AI response");
                    return geminiResponse;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error calling Gemini AI (Attempt {attempt}/{MaxRetries})");

                    if (attempt == MaxRetries)
                    {
                        throw new InvalidOperationException($"Failed to generate sprint plan after {MaxRetries} attempts", ex);
                    }

                    // Wait before retry (exponential backoff)
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                }
            }

            throw new InvalidOperationException("Failed to generate sprint plan");
        }

        private async Task<string> CallGeminiAPIAsync(string prompt)
        {
            try
            {
                // Using direct REST API call to Gemini
                var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}";

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
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending request to Gemini API");
                _logger.LogDebug($"Request URL: {apiUrl.Replace(_apiKey, "***")}");
                _logger.LogDebug($"Request Body: {jsonContent.Substring(0, Math.Min(500, jsonContent.Length))}...");

                var response = await _httpClient.PostAsync(apiUrl, httpContent);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Gemini API returned error status: {response.StatusCode}");
                    _logger.LogError($"Response: {responseContent}");
                    throw new HttpRequestException($"Gemini API request failed with status {response.StatusCode}: {responseContent}");
                }

                _logger.LogInformation("Gemini API Response received successfully");
                _logger.LogDebug($"Response: {responseContent}");

                // Parse the response to extract the text
                var geminiResponse = JsonSerializer.Deserialize<JsonElement>(responseContent);

                if (geminiResponse.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var content))
                    {
                        if (content.TryGetProperty("parts", out var parts) && parts.GetArrayLength() > 0)
                        {
                            var firstPart = parts[0];
                            if (firstPart.TryGetProperty("text", out var textElement))
                            {
                                var responseText = textElement.GetString();
                                if (string.IsNullOrEmpty(responseText))
                                {
                                    throw new InvalidOperationException("Gemini API returned empty text response");
                                }
                                return responseText;
                            }
                        }
                    }
                }

                throw new InvalidOperationException("Gemini API response format is invalid or missing expected fields");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                throw;
            }
        }

        private GeminiSprintPlanResponseDto ParseGeminiResponse(string responseText)
        {
            try
            {
                // Try to extract JSON from markdown code blocks
                var jsonMatch = Regex.Match(responseText, @"```json\s*([\s\S]*?)\s*```");
                var jsonText = jsonMatch.Success ? jsonMatch.Groups[1].Value : responseText;

                // Remove any leading/trailing whitespace
                jsonText = jsonText.Trim();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var response = JsonSerializer.Deserialize<GeminiSprintPlanResponseDto>(jsonText, options);

                if (response == null || response.SprintPlan == null)
                {
                    throw new InvalidOperationException("Failed to parse Gemini response: response is null");
                }

                return response;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Failed to parse Gemini response as JSON. Response: {responseText}");
                throw new InvalidOperationException("Failed to parse Gemini response as valid JSON", ex);
            }
        }

        private string BuildGeminiPrompt(SprintPlanningContextDto context)
        {
            var contextJson = JsonSerializer.Serialize(context, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            var prompt = $@"# Sprint Planning AI Prompt (Send to Gemini)

You are an expert Agile sprint planner. Your task is to analyze the provided project context and create an optimal sprint plan by selecting appropriate issues from the backlog.

## Context Provided
You have been given:
1. **Project Information**: Basic project details
2. **New Sprint Parameters**: Name, goal, team, dates, and target story points
3. **Backlog Issues**: All available issues that can be assigned to this sprint
4. **Team Velocity Data**: Historical performance metrics, individual team member velocities, and trends
5. **Current Sprint Load**: In-progress and planned sprints to avoid overloading the team

## Your Objectives

### Primary Goals
1. **Capacity Planning**: Select issues that match the team's proven velocity without overloading
2. **Priority Alignment**: Prioritize CRITICAL and HIGH priority issues
3. **Sprint Goal Alignment**: Choose issues that support the sprint goal
4. **Team Balance**: Distribute work appropriately across team members based on their historical performance
5. **Dependency Management**: Consider parent-child issue relationships

### Selection Criteria

**MUST Consider:**
- Team's average velocity from historical sprints
- Individual team member completion rates and preferences
- Issue priority levels (CRITICAL > HIGH > MEDIUM > LOW)
- Story point estimates already assigned to issues
- Current workload (in-progress and planned sprints)
- Dependencies (parent_issue_id relationships)
- Sprint goal alignment

**MUST Avoid:**
- Overloading team beyond historical velocity (target 85-95% of average velocity)
- Selecting dependent issues without their parent issues
- Ignoring critical/high priority issues in favor of low priority ones
- Assigning more work to team members already heavily loaded in active sprints

### Response Format

Return ONLY a valid JSON object (no markdown, no explanations outside JSON) with this exact structure:

```json
{{
  ""sprint_plan"": {{
    ""selected_issues"": [
      {{
        ""issue_id"": ""uuid"",
        ""issue_key"": ""string"",
        ""story_points"": integer,
        ""suggested_assignee_id"": integer or null,
        ""rationale"": ""Brief explanation why this issue was selected""
      }}
    ],
    ""total_story_points"": number,
    ""summary"": ""2-3 paragraph summary explaining the overall sprint composition, how it aligns with the sprint goal, and why these issues were chosen together"",
    ""recommendations"": [
      {{
        ""type"": ""capacity|priority|risk|dependency|team_balance"",
        ""severity"": ""info|warning|critical"",
        ""message"": ""Specific recommendation or concern""
      }}
    ],
    ""capacity_analysis"": {{
      ""team_capacity_utilization"": number (as percentage, e.g., 87.5),
      ""estimated_completion_probability"": number (as percentage, e.g., 92),
      ""risk_factors"": [""Array of specific risks identified in this sprint plan""]
    }}
  }}
}}
```

## Decision-Making Guidelines

### When team velocity is high (consistent 90%+ completion):
- Target 90-95% of average velocity
- Include some stretch goals
- Balance between different issue types

### When team velocity is inconsistent or trending down:
- Target 80-85% of average velocity
- Focus on higher priority items only
- Flag capacity concerns in recommendations

### When backlog has many CRITICAL items:
- Prioritize these even if it means fewer total issues
- Flag if critical items exceed team capacity

### When dependencies exist:
- Always include parent issues before subtasks
- Group related issues in the same sprint when possible
- Flag dependency chains in recommendations

### For team member assignments:
- Match issue types to member preferences (if data available)
- Consider current workload in active sprints
- Distribute story points proportionally to individual velocities
- Leave assignee as null if no clear match

## Important Rules

1. **ONLY select issues from the provided backlog** - Never create new issues or reference issues not in the backlog
2. **NEVER modify story points** - Use the story_points value exactly as provided in each issue
3. **Stay within capacity** - Total selected story points should not exceed team's proven velocity significantly
4. **Provide specific rationales** - Each issue should have a clear reason for selection
5. **Be honest about risks** - If the sprint seems overloaded or high-risk, say so in recommendations

## Context Data

{contextJson}

## Your Task

Analyze the above context and create an optimal sprint plan. Return your response as a valid JSON object following the exact structure specified above.";

            _logger.LogDebug($"Generated prompt for Gemini: {prompt}");

            return prompt;
        }
    }
}
