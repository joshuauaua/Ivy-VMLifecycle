using Ivy.VMLifecycle.Frontend.Models;
using Ivy.VMLifecycle.Frontend.Services;

namespace Ivy.VMLifecycle.Frontend.Apps;

[App(icon: Icons.Monitor, title: "Home")]
public class HomeApp : ViewBase
{
  public override object? Build()
  {
    var client = UseService<IClientProvider>();
    var isLoggedIn = UseState(false);
    var selectedIndex = UseState(0);

    // Audit Log Filters
    var filterUser = UseState("All Users");
    var filterAction = UseState("All Actions");
    var filterStart = UseState<DateTime?>((DateTime?)null);
    var filterEnd = UseState<DateTime?>((DateTime?)null);

    // VM Filters
    var vmSearch = UseState("");
    var selectedVMs = UseState(new List<string>());
    var snapshotVM = UseState<VirtualMachine?>(() => null);

    object DashboardSection() => Layout.Vertical().Gap(4)
        | Layout.Horizontal().Gap(4)
            | new Card(Layout.Vertical().Center() | Text.H3("5") | Text.P("Active VMs"))
            | new Card(Layout.Vertical().Center() | Text.H3("12") | Text.P("Snapshots"))
            | new Card(Layout.Vertical().Center() | Text.H3("128") | Text.P("Audit Actions"));

    object VMSection() => new HeaderLayout(
        new Card(
            Layout.Horizontal().Gap(4).Center()
                | Text.H3("Virtual Machines")
                | new Spacer()
                | new TextInput(vmSearch, "Search VMs...").Variant(TextInputs.Search).Width(300)
        ),
        Layout.Vertical().Gap(4)
            | UseService<IVMService>().GetVMsAsync().Result.ToTable()
                .Width(Size.Full())
                .Header(v => v.Id, "")
                .Builder(v => v.Id, f => f.Func<VirtualMachine, Guid>(id =>
                    selectedVMs.ToSelectInput(new[] { id.ToString() }.ToOptions())
                        .Variant(SelectInputs.List)))
                .Header(v => v.Name, "VM Name")
                .Header(v => v.Provider, "Provider")
                .Header(v => v.Status, "Status")
                .Builder(v => v.Status, f => f.Func<VirtualMachine, VMStatus>(status => status == VMStatus.Running
                    ? new Badge("Running").Primary()
                    : new Badge("Stopped").Destructive()))
                .Header(v => v.DisplayTags, "Tags")
                .Header(v => v.LastAction, "Last Action")
                .Header(v => v.Self, "Actions")
                .Builder(v => v.Self, f => f.Func<VirtualMachine, VirtualMachine>(vm =>
                    Layout.Horizontal().Gap(2)
                        | new Button("").Icon(Icons.Camera).Variant(ButtonVariant.Ghost).HandleClick(_ => snapshotVM.Value = vm)
                        | (vm.Status == VMStatus.Running
                            ? (Layout.Horizontal().Gap(2)
                                | new Button("").Icon(Icons.CircleStop).Variant(ButtonVariant.Ghost)
                                | new Button("").Icon(Icons.RefreshCw).Variant(ButtonVariant.Ghost))
                            : new Button("").Icon(Icons.Play).Variant(ButtonVariant.Ghost))
                ))
                .Align(v => v.Name, Align.Left)
                .Align(v => v.Status, Align.Center)
    );

    object AuditLogSection() => new HeaderLayout(
        new Card(
            Layout.Horizontal().Gap(4).Center()
                | new Field(new SelectInput<string>(filterUser, new[] { "All Users", "joshuang", "system", "admin" }.ToOptions()), "User")
                | new Field(new SelectInput<string>(filterAction, new[] { "All Actions" }.Concat(Enum.GetNames<AuditAction>()).ToOptions()), "Action")
                | new Field(new DateTimeInput<DateTime?>(filterStart), "Start Date")
                | new Field(new DateTimeInput<DateTime?>(filterEnd), "End Date")
        ),
        Layout.Vertical().Gap(4)
            | UseService<IVMService>().GetAuditLogsAsync().Result.ToTable()
                .Width(Size.Full())
                .Header(l => l.User, "User")
                .Header(l => l.Action, "Action")
                .Header(l => l.StartDate, "Time")
                .Align(l => l.User, Align.Left)
                .Align(l => l.Action, Align.Center)
    );

    object SnapshotSection(VirtualMachine vm) => new HeaderLayout(
        Layout.Vertical().Gap(2)
            | new Button("Back").Icon(Icons.ArrowLeft).Variant(ButtonVariant.Ghost).HandleClick(_ => snapshotVM.Value = null)
            | Layout.Horizontal().Gap(4).Center()
                | Text.H3($"Snapshots for {vm.Name}")
                | new Spacer()
                | new Button("Create Snapshot").Icon(Icons.Camera).Variant(ButtonVariant.Primary)
        ,
        Layout.Vertical().Gap(4)
            | UseService<IVMService>().GetSnapshotsAsync(vm.Id).Result.ToTable()
                .Width(Size.Full())
                .Header(s => s.Name, "Snapshot Name")
                .Header(s => s.CreatedAt, "Created At")
                .Header(s => s.Id, "Actions")
                .Builder(s => s.Id, f => f.Func<Snapshot, Guid>(id =>
                    Layout.Horizontal().Gap(2)
                        | new Button("").Icon(Icons.RefreshCw).Variant(ButtonVariant.Ghost)
                        | new Button("").Icon(Icons.Trash2).Variant(ButtonVariant.Ghost)
                ))
    );

    var navHeader = new Card(
        Layout.Horizontal().Gap(3).Center()
            | Text.H5("Ivy VM Lifecycle")
            | new TabsLayout(e => selectedIndex.Value = e.Value, null, null, null, selectedIndex.Value,
                  new Tab("Dashboard").Icon(Icons.LayoutDashboard),
                  new Tab("Virtual Machines").Icon(Icons.Monitor),
                  new Tab("Audit Logs").Icon(Icons.List),
                  new Tab("Settings").Icon(Icons.Settings)
              )
              .Variant(TabsVariant.Content)
              .Padding(0)
              .Width(Size.Shrink())
            | new Spacer()
            | new Button(isLoggedIn.Value ? "Logout" : "Login")
                .Variant(isLoggedIn.Value ? ButtonVariant.Ghost : ButtonVariant.Primary)
                .HandleClick(_ =>
                {
                  isLoggedIn.Value = !isLoggedIn.Value;
                  client.Toast(isLoggedIn.Value ? "Logged in successfully" : "Logged out successfully");
                })
    );

    object GetSectionContent()
    {
      if (snapshotVM.Value != null) return SnapshotSection(snapshotVM.Value);

      return selectedIndex.Value switch
      {
        0 => DashboardSection(),
        1 => VMSection(),
        2 => AuditLogSection(),
        3 => Text.Label("Settings Section"),
        _ => Text.P("Section not found")
      };
    }

    return Layout.Vertical().Gap(2)
        | navHeader
        | GetSectionContent();
  }
}

// Removing public class HomeView : ViewBase and its content as it's merged into HomeApp

