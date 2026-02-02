namespace Ivy.VMLifecycle.Api.Enums;

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
