namespace Ivy.VMLifecycle.Frontend.Models;

public enum VMStatus
{
  Running,
  Stopped
}

public enum VMProvider
{
  AWS,
  Azure,
  GCP
}

public class VirtualMachine
{
  public Guid Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public VMStatus Status { get; set; }
  public VMProvider Provider { get; set; }
  public List<string> Tags { get; set; } = new();
  public DateTime LastAction { get; set; }

  public string DisplayTags => string.Join(" ", Tags);
}

public enum AuditAction
{
  Login,
  Logout,
  Reboot,
  SnapshotCreate,
  SnapshotDelete,
  SnapshotRestore,
  Start,
  Stop,
  View
}

public class AuditLog
{
  public Guid Id { get; set; }
  public string User { get; set; } = string.Empty;
  public AuditAction Action { get; set; }
  public DateTime StartDate { get; set; }
  public DateTime? EndDate { get; set; }
}
