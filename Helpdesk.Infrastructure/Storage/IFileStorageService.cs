using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpdesk.Infrastructure.Storage
{
    public interface IFileStorageService
    {
        Task<(string StoredFileName, string FilePath)>
            SaveFileAsync(IFormFile file);
    }
}
