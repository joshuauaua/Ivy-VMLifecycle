using Ivy.VMLifecycle.Api.Data;
using Ivy.VMLifecycle.Api.Enums;
using Ivy.VMLifecycle.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ivy.VMLifecycle.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SnapshotsController : ControllerBase
{
  private readonly AppDbContext _context;

  public SnapshotsController(AppDbContext context)
  {
    _context = context;
  }

  [HttpDelete("{id}")]
  public async Task<IActionResult> DeleteSnapshot(Guid id)
  {
    var snapshot = await _context.Snapshots.FindAsync(id);
    if (snapshot == null) return NotFound();

    _context.Snapshots.Remove(snapshot);

    var log = new AuditLog
    {
      Id = Guid.NewGuid(),
      User = "User",
      Action = AuditAction.SnapshotDelete,
      StartDate = DateTime.UtcNow,
      EndDate = DateTime.UtcNow
    };
    _context.AuditLogs.Add(log);

    await _context.SaveChangesAsync();
    return NoContent();
  }

  [HttpPost("{id}/restore")]
  public async Task<IActionResult> RestoreSnapshot(Guid id)
  {
    var snapshot = await _context.Snapshots.Include(s => s.VirtualMachine).FirstOrDefaultAsync(s => s.Id == id);
    if (snapshot == null) return NotFound();

    if (snapshot.VirtualMachine != null)
    {
      snapshot.VirtualMachine.LastAction = DateTime.UtcNow;
      // In a real system, you'd trigger a hypervisor restore here.
    }

    var log = new AuditLog
    {
      Id = Guid.NewGuid(),
      User = "User",
      Action = AuditAction.SnapshotRestore,
      StartDate = DateTime.UtcNow,
      EndDate = DateTime.UtcNow
    };
    _context.AuditLogs.Add(log);

    await _context.SaveChangesAsync();
    return Ok(snapshot);
  }
}
