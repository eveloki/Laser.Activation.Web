using Laser.Activation.Web.Data;
using Laser.Activation.Web.Models;
using Laser.Activation.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Laser.Activation.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActivationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IActivationService _activationService;
    private readonly ILogger<ActivationController> _logger;

    public ActivationController(
        AppDbContext context,
        IActivationService activationService,
        ILogger<ActivationController> logger)
    {
        _context = context;
        _activationService = activationService;
        _logger = logger;
    }

    [HttpPost("activate")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Activate(
        [FromForm] IFormFile hwidFile,
        [FromForm] string projectName,
        [FromForm] string departmentName,
        [FromForm] string personName,
        [FromForm] string versionInf)
    {
        if (hwidFile == null || hwidFile.Length == 0)
        {
            return BadRequest(new ActivateResponse
            {
                Success = false,
                Message = "请上传HWID文件"
            });
        }

        if (!hwidFile.FileName.EndsWith(".req", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ActivateResponse
            {
                Success = false,
                Message = "请上传.req后缀的HWID文件"
            });
        }

        string identificationCode;
        using (var reader = new StreamReader(hwidFile.OpenReadStream()))
        {
            identificationCode = await reader.ReadToEndAsync();
        }

        if (string.IsNullOrWhiteSpace(identificationCode))
        {
            return BadRequest(new ActivateResponse
            {
                Success = false,
                Message = "HWID文件内容为空"
            });
        }

        var existingRecord = await _context.ActivationRecords
            .FirstOrDefaultAsync(r => r.IdentificationCode == identificationCode);

        if (existingRecord != null)
        {
            _logger.LogInformation("Found existing activation for HWID, returning record {Id}", existingRecord.Id);
            return Ok(new ActivateResponse
            {
                Success = true,
                Message = "该设备已激活，返回已有激活码",
                ActivationCode = existingRecord.ActivationCode,
                RecordId = existingRecord.Id
            });
        }

        var activationCode = _activationService.GenerateActivationCode(identificationCode);

        var record = new ActivationRecord
        {
            DataGuid = Guid.NewGuid().ToString(),
            ProjectName = projectName,
            DepartmentName = departmentName,
            PersonName = personName,
            VersionInf = versionInf,
            IdentificationCode = identificationCode,
            ActivationCode = activationCode,
            CreatedTime = DateTime.Now
        };

        _context.ActivationRecords.Add(record);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new activation record {Id} for project {Project}", record.Id, projectName);

        return Ok(new ActivateResponse
        {
            Success = true,
            Message = "激活成功",
            ActivationCode = activationCode,
            RecordId = record.Id
        });
    }

    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadActivationFile(int id, [FromQuery] string version = "v1")
    {
        var record = await _context.ActivationRecords.FindAsync(id);
        if (record == null || string.IsNullOrEmpty(record.ActivationCode))
        {
            return NotFound(new { success = false, message = "激活记录不存在" });
        }

        var content = record.ActivationCode;
        if (string.Equals(version, "v2", StringComparison.OrdinalIgnoreCase))
        {
            content = _activationService.FormatV2Output(content);
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(content);

        return File(bytes, "application/octet-stream", $"activation_{id}.reqrep");
    }
}
