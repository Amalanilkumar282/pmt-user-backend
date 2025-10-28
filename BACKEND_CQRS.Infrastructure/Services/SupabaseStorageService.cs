using Supabase;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;
using System;

namespace BACKEND_CQRS.Infrastructure.Services
{
    public interface ISupabaseStorageService
    {
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string bucketName);
    }

    public class SupabaseStorageService : ISupabaseStorageService
    {
        private readonly Supabase.Client _client;

        public SupabaseStorageService(IConfiguration configuration)
        {
            var url = configuration["Supabase:Url"];
            var key = configuration["Supabase:Key"];
            _client = new Supabase.Client(url, key, new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = false
            });
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string bucketName)
        {
            await _client.InitializeAsync();
            
            var storage = _client.Storage.From(bucketName);
            
            // Upload file
            var path = $"uploads/{Guid.NewGuid()}_{fileName}";
            
            // Convert stream to byte array
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();
            
            await storage.Upload(fileBytes, path);
            
            // Get public URL
            var publicUrl = storage.GetPublicUrl(path);
            return publicUrl;
        }
    }
}
