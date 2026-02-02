using Ivy.VMLifecycle.Frontend.Models;
using System.Net.Http.Json;

namespace Ivy.VMLifecycle.Frontend.Services;

public interface IVMService
{
  Task<List<VirtualMachine>> GetVMsAsync();
  Task<List<AuditLog>> GetAuditLogsAsync();
  Task<List<Snapshot>> GetSnapshotsAsync(Guid vmId);
}

public class VMService : IVMService
{
  private readonly HttpClient _httpClient;
  private const string BaseUrl = "http://localhost:5000/api";

  public VMService()
  {
    _httpClient = new HttpClient();
  }

  public async Task<List<VirtualMachine>> GetVMsAsync()
  {
    try
    {
      var vms = await _httpClient.GetFromJsonAsync<List<VirtualMachine>>($"{BaseUrl}/virtualmachines");
      return vms ?? new List<VirtualMachine>();
    }
    catch
    {
      // Fallback for demo/if backend is down
      return new List<VirtualMachine>
            {
                new VirtualMachine {Id = Guid.NewGuid(), Name = "Web-Prod-01", Status = VMStatus.Running, Provider = VMProvider.AWS, Tags = new List<string>{"web", "prod"}, LastAction = DateTime.Now},
                new VirtualMachine {Id = Guid.NewGuid(), Name = "DB-Staging-01", Status = VMStatus.Stopped, Provider = VMProvider.Azure, Tags = new List<string>{"db", "staging"}, LastAction = DateTime.Now.AddDays(-1)}
            };
    }
  }

  public async Task<List<AuditLog>> GetAuditLogsAsync()
  {
    try
    {
      var logs = await _httpClient.GetFromJsonAsync<List<AuditLog>>($"{BaseUrl}/auditlogs");
      return logs ?? new List<AuditLog>();
    }
    catch
    {
      // Fallback for demo/if backend is down
      return new List<AuditLog>
            {
                new AuditLog {Id = Guid.NewGuid(), User = "joshuang", Action = AuditAction.Login, StartDate = DateTime.Now.AddHours(-2), EndDate = DateTime.Now.AddHours(-2)},
                new AuditLog {Id = Guid.NewGuid(), User = "joshuang", Action = AuditAction.Start, StartDate = DateTime.Now.AddMinutes(-45), EndDate = DateTime.Now.AddMinutes(-45)},
                new AuditLog {Id = Guid.NewGuid(), User = "system", Action = AuditAction.SnapshotCreate, StartDate = DateTime.Now.AddMinutes(-10), EndDate = DateTime.Now.AddMinutes(-10)}
            };
    }
  }

  public async Task<List<Snapshot>> GetSnapshotsAsync(Guid vmId)
  {
    // Return mock snapshots
    return new List<Snapshot>
    {
      new Snapshot { Id = Guid.NewGuid(), Name = "Initial Backup", CreatedAt = DateTime.Now.AddDays(-2) },
      new Snapshot { Id = Guid.NewGuid(), Name = "Post-Config", CreatedAt = DateTime.Now.AddDays(-1) },
      new Snapshot { Id = Guid.NewGuid(), Name = "Daily-Snap-01", CreatedAt = DateTime.Now.AddHours(-5) }
    };
  }
}
