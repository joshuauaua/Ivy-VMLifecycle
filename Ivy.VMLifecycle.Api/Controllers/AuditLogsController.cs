using Ivy.VMLifecycle.Api.Data;
using Ivy.VMLifecycle.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ivy.VMLifecycle.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
  private readonly AppDbContext _context;

  public AuditLogsController(AppDbContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<AuditLog>>> GetLogs()
  {
    return await _context.AuditLogs.OrderByDescending(l => l.StartDate).ToListAsync();
  }
}
