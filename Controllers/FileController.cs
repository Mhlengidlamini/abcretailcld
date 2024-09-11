using Microsoft.AspNetCore.Mvc;
using ABCRetail.AzureFileService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using ABCRetail.ViewModel;

namespace ABCRetail.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FileController : Controller
    {
        private readonly IContractsFileService _contractsFileService;
        private readonly ILogsFileService _logsFileService;

        public FileController(IContractsFileService contractsFileService, ILogsFileService logsFileService)
        {
            _contractsFileService = contractsFileService;
            _logsFileService = logsFileService;
        }

        public async Task<IActionResult> Contracts()
        {
            var files = await _contractsFileService.ListFilesAsync();
            var model = new ContractsViewModel
            {
                Contracts = files
            };
            return View(model);
        }
        public async Task<IActionResult> Logs()
        {
            var files = await _logsFileService.ListFilesAsync();
            var model = new LogsViewModel
            {
                Logs = files
            };
            return View(model);
        }

        // Upload contract files
        [HttpPost]
        public async Task<IActionResult> UploadContract(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                await _contractsFileService.UploadFileAsync(file);
            }
            return RedirectToAction(nameof(Contracts));
        }

        // Upload log files
        [HttpPost]
        public async Task<IActionResult> UploadLog(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                await _logsFileService.UploadFileAsync(file);
            }
            return RedirectToAction(nameof(Logs));
        }

        // Download contract files
        public async Task<IActionResult> DownloadContract(string fileName)
        {
            var fileStream = await _contractsFileService.DownloadFileAsync(fileName);
            return File(fileStream, "application/octet-stream", fileName);
        }

        // Download log files
        public async Task<IActionResult> DownloadLog(string fileName)
        {
            var fileStream = await _logsFileService.DownloadFileAsync(fileName);
            return File(fileStream, "application/octet-stream", fileName);
        }
    }
}
