using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Infrastructure.Storage
{

    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public LocalFileStorageService(
            IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<(string StoredFileName, string FilePath)>
            SaveFileAsync(IFormFile file)
        {
            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "tickets");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var storedFileName =
                $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var fullPath = Path.Combine(
                uploadsFolder,
                storedFileName);

            using var stream = new FileStream(
                fullPath,
                FileMode.Create);

            await file.CopyToAsync(stream);

            var relativePath =
                $"uploads/tickets/{storedFileName}";

            return (storedFileName, relativePath);
        }
    }
}
