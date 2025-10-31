using BACKEND_CQRS.Domain.Entities;
using BACKEND_CQRS.Domain.Persistance;
using BACKEND_CQRS.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BACKEND_CQRS.Infrastructure.Repository
{
    public class StatusRepository : GenericRepository<Status>, IStatusRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StatusRepository>? _logger;

        public StatusRepository(AppDbContext context, ILogger<StatusRepository>? logger = null) : base(context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger;
        }

        public async Task<Status?> GetStatusByNameAsync(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
            {
                throw new ArgumentException("Status name cannot be null or empty", nameof(statusName));
            }

            try
            {
                _logger?.LogInformation("Searching for status with name: {StatusName}", statusName);

                // Case-insensitive search
                var status = await _context.Statuses
                    .FirstOrDefaultAsync(s => s.StatusName.ToLower() == statusName.ToLower());

                if (status != null)
                {
                    _logger?.LogInformation("Found existing status: {StatusName} with ID: {StatusId}", 
                        status.StatusName, status.Id);
                }
                else
                {
                    _logger?.LogInformation("No existing status found with name: {StatusName}", statusName);
                }

                return status;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error searching for status with name: {StatusName}", statusName);
                throw new InvalidOperationException(
                    $"An error occurred while searching for status '{statusName}'", ex);
            }
        }

        public async Task<Status?> GetStatusByIdAsync(int statusId)
        {
            try
            {
                _logger?.LogInformation("Fetching status with ID: {StatusId}", statusId);

                var status = await _context.Statuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == statusId);

                if (status != null)
                {
                    _logger?.LogInformation("Found status: {StatusName} with ID: {StatusId}", 
                        status.StatusName, status.Id);
                }
                else
                {
                    _logger?.LogWarning("Status with ID {StatusId} not found", statusId);
                }

                return status;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error fetching status with ID: {StatusId}", statusId);
                throw new InvalidOperationException(
                    $"An error occurred while fetching status with ID {statusId}", ex);
            }
        }

        public async Task<Status> CreateStatusAsync(string statusName)
        {
            if (string.IsNullOrWhiteSpace(statusName))
            {
                throw new ArgumentException("Status name cannot be null or empty", nameof(statusName));
            }

            try
            {
                _logger?.LogInformation("Creating new status: {StatusName}", statusName);

                var status = new Status
                {
                    StatusName = statusName.Trim()
                };

                await _context.Statuses.AddAsync(status);
                await _context.SaveChangesAsync();

                _logger?.LogInformation("Successfully created status: {StatusName} with ID: {StatusId}", 
                    status.StatusName, status.Id);

                return status;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating status with name: {StatusName}", statusName);
                throw new InvalidOperationException(
                    $"An error occurred while creating status '{statusName}'", ex);
            }
        }
    }
}
