using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Services.Interfaces.Documents
{
    public interface IFileService
    {
        Task<string> SaveFile(Stream fileStream, string userId,string fileName);

        Stream GetFile(string userId, string fileName);
    }
}
