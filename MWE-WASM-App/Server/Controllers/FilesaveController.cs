using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace MWE_WASM_App.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class FilesaveController : ControllerBase
{
    private readonly IWebHostEnvironment env;
    private readonly ILogger<FilesaveController> logger;

    public FilesaveController(IWebHostEnvironment env, ILogger<FilesaveController> logger)
    {
        this.env = env;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<String>> PostFile([FromForm] IEnumerable<IFormFile> files)
    {
        long maxFileSize = 1024 * 1024 * 15;
        var filesProcessed = 0;
        var resourcePath = new Uri($"{Request.Scheme}://{Request.Host}/");
        var fileNameBuilder = new StringBuilder();

        foreach (var file in files)
        {
            string trustedFileNameForFileStorage;
            var untrustedFileName = file.FileName;
            var trustedFileNameForDisplay = WebUtility.HtmlEncode(untrustedFileName);


            if (file.Length == 0)
            {
                logger.LogInformation("{FileName} length is 0 (Err: 1)", trustedFileNameForDisplay);
            }
            else if (file.Length > maxFileSize)
            {
                logger.LogInformation("{FileName} of {Length} bytes is larger than the limit of {Limit} bytes (Err: 2)", trustedFileNameForDisplay, file.Length, maxFileSize);
            }
            else
            {
                try
                {
                    trustedFileNameForFileStorage = Path.GetRandomFileName();
                    var path = Path.Combine(env.ContentRootPath, env.EnvironmentName, "unsafe_uploads", trustedFileNameForFileStorage);

                    await using FileStream fs = new(path, FileMode.Create);
                    await file.CopyToAsync(fs);

                    logger.LogInformation("{FileName} saved at {Path}", trustedFileNameForDisplay, path);
                    fileNameBuilder.AppendLine(untrustedFileName);
                }
                catch (IOException ex)
                {
                    logger.LogError("{FileName} error on upload (Err: 3): {Message}", trustedFileNameForDisplay, ex.Message);
                }
            }

            filesProcessed++;

        }

        return new CreatedResult(resourcePath, fileNameBuilder.ToString());
    }
}