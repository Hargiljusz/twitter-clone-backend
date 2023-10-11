using DataService.Services.Interfaces.Documents;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services
{
    public class FileService : IFileService
    {
        private readonly IConfiguration _configuration;

        public FileService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Stream GetFile(string userId, string fileName)
        {
            string path = Path.Combine(_configuration["FileDir"], userId);
            if (!Directory.Exists(path))
            {
                throw new FileNotFoundException("File Not Found");
            }
            path = Path.Combine(path, fileName);
            var stream =  File.OpenRead(path);
            return stream;
        }

        public async Task<string> SaveFile(Stream fileStream, string userId, string fileName)
        {
            string path = Path.Combine(_configuration["FileDir"],  userId);

            bool dirExist = Directory.Exists(path);
            if (!dirExist)
            {
                Directory.CreateDirectory(path);
            }

            var guid = Guid.NewGuid();
            path = Path.Combine(path, $"{guid}_{fileName}");

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }

            return Path.Combine(userId, $"{guid}_{fileName}");
        }
    }
}
