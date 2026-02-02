using Ivy.VMLifecycle.Api.Enums;
using System.Text.Json.Serialization;

namespace Ivy.VMLifecycle.Api.Models;

public class VirtualMachine
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public VMStatus Status { get; set; }
  public VMProvider Provider { get; set; }
  public List<string> Tags { get; set; } = new();
  public DateTime LastAction { get; set; }

  // Navigation property
  public List<Snapshot> Snapshots { get; set; } = new();
}

public class Snapshot
{
  public Guid Id { get; set; }
  public string SnapshotName { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; }

  // Foreign Key
  public Guid VirtualMachineId { get; set; }
  [JsonIgnore]
  public VirtualMachine? VirtualMachine { get; set; }
}

public class AuditLog
{
  public Guid Id { get; set; }
  public string User { get; set; } = string.Empty;
  public AuditAction Action { get; set; }
  public DateTime StartDate { get; set; }
  public DateTime? EndDate { get; set; }
}
