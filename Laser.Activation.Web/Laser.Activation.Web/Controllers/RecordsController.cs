using Laser.Activation.Web.Data;
using Laser.Activation.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Laser.Activation.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecordsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<RecordsController> _logger;

    public RecordsController(AppDbContext context, ILogger<RecordsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecords(
        [FromQuery] string? keyword,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.ActivationRecords.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(r =>
                (r.ProjectName != null && r.ProjectName.Contains(keyword)) ||
                (r.DepartmentName != null && r.DepartmentName.Contains(keyword)) ||
                (r.PersonName != null && r.PersonName.Contains(keyword)) ||
                (r.VersionInf != null && r.VersionInf.Contains(keyword)) ||
                (r.IdentificationCode != null && r.IdentificationCode.Contains(keyword)));
        }

        var total = await query.CountAsync();
        var records = await query
            .OrderByDescending(r => r.CreatedTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.DataGuid,
                r.ProjectName,
                r.DepartmentName,
                r.PersonName,
                r.VersionInf,
                r.IdentificationCode,
                r.ActivationCode,
                CreatedTime = r.CreatedTime != null
                    ? r.CreatedTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    : null
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, records });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRecord(int id)
    {
        var record = await _context.ActivationRecords.FindAsync(id);
        if (record == null)
        {
            return NotFound(new { success = false, message = "记录不存在" });
        }

        return Ok(new
        {
            record.Id,
            record.DataGuid,
            record.ProjectName,
            record.DepartmentName,
            record.PersonName,
            record.VersionInf,
            record.IdentificationCode,
            record.ActivationCode,
            CreatedTime = record.CreatedTime != null
                ? record.CreatedTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
                : null
        });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRecord(int id)
    {
        var record = await _context.ActivationRecords.FindAsync(id);
        if (record == null)
        {
            return NotFound(new { success = false, message = "记录不存在" });
        }

        _context.ActivationRecords.Remove(record);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted activation record {Id}", id);
        return Ok(new { success = true, message = "删除成功" });
    }
}
