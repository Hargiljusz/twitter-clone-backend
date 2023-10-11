using DataService.Services.Interfaces.Documents;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace GraphQL.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet("{userId}/{appFileName}")]
        public async Task<IActionResult>GetFile(string userId,string appFileName)
        {
            var fileStream = _fileService.GetFile(userId, appFileName);
            await Task.Delay(1);
            
            return File(fileStream, "application/octet-stream", getFileName(appFileName));
        }

        private string getFileName(string appFileName)
        {
            var idxOfUnderLine = appFileName.IndexOf('_');
            var fileName = appFileName[idxOfUnderLine..];
            return fileName;
        }
    }
}
