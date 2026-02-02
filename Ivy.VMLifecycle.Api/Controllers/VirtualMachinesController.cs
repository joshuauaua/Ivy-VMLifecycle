using Ivy.VMLifecycle.Api.Data;
using Ivy.VMLifecycle.Api.Enums;
using Ivy.VMLifecycle.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ivy.VMLifecycle.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VirtualMachinesController : ControllerBase
{
  private readonly AppDbContext _context;

  public VirtualMachinesController(AppDbContext context)
  {
    _context = context;
  }

  [HttpGet]
  public async Task<ActionResult<IEnumerable<VirtualMachine>>> GetVMs()
  {
    return await _context.VirtualMachines.Include(v => v.Snapshots).ToListAsync();
  }

  [HttpGet("{id}")]
  public async Task<ActionResult<VirtualMachine>> GetVM(Guid id)
  {
    var vm = await _context.VirtualMachines.Include(v => v.Snapshots).FirstOrDefaultAsync(v => v.Id == id);
    if (vm == null) return NotFound();
    return vm;
  }

  [HttpPost]
  public async Task<ActionResult<VirtualMachine>> CreateVM(VirtualMachine vm)
  {
    vm.Id = Guid.NewGuid();
    vm.LastAction = DateTime.UtcNow;
    _context.VirtualMachines.Add(vm);

    await LogAction("System", AuditAction.View, "Created VM " + vm.Name); // Just a placeholder user
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetVM), new { id = vm.Id }, vm);
  }

  [HttpPost("{id}/start")]
  public async Task<IActionResult> StartVM(Guid id)
  {
    var vm = await _context.VirtualMachines.FindAsync(id);
    if (vm == null) return NotFound();

    vm.Status = VMStatus.Running;
    vm.LastAction = DateTime.UtcNow;

    await LogAction("User", AuditAction.Start, "Started VM " + vm.Name);
    await _context.SaveChangesAsync();

    return Ok(vm);
  }

  [HttpPost("{id}/stop")]
  public async Task<IActionResult> StopVM(Guid id)
  {
    var vm = await _context.VirtualMachines.FindAsync(id);
    if (vm == null) return NotFound();

    vm.Status = VMStatus.Stopped;
    vm.LastAction = DateTime.UtcNow;

    await LogAction("User", AuditAction.Stop, "Stopped VM " + vm.Name);
    await _context.SaveChangesAsync();

    return Ok(vm);
  }

  [HttpPost("{id}/reboot")]
  public async Task<IActionResult> RebootVM(Guid id)
  {
    var vm = await _context.VirtualMachines.FindAsync(id);
    if (vm == null) return NotFound();

    vm.Status = VMStatus.Running;
    vm.LastAction = DateTime.UtcNow;

    await LogAction("User", AuditAction.Reboot, "Rebooted VM " + vm.Name);
    await _context.SaveChangesAsync();

    return Ok(vm);
  }

  [HttpPost("{id}/snapshot")]
  public async Task<ActionResult<Snapshot>> CreateSnapshot(Guid id, [FromBody] string snapshotName)
  {
    var vm = await _context.VirtualMachines.FindAsync(id);
    if (vm == null) return NotFound();

    var snapshot = new Snapshot
    {
      Id = Guid.NewGuid(),
      SnapshotName = snapshotName,
      CreatedAt = DateTime.UtcNow,
      VirtualMachineId = id
    };

    _context.Snapshots.Add(snapshot);
    await LogAction("User", AuditAction.SnapshotCreate, "Created snapshot " + snapshotName + " for VM " + vm.Name);
    await _context.SaveChangesAsync();

    return Ok(snapshot);
  }

  private async Task LogAction(string user, AuditAction action, string details)
  {
    var log = new AuditLog
    {
      Id = Guid.NewGuid(),
      User = user,
      Action = action,
      StartDate = DateTime.UtcNow,
      EndDate = DateTime.UtcNow
    };
    _context.AuditLogs.Add(log);
  }
}
