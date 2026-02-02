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

    object DashboardSection() => Layout.Vertical().Gap(4)
        | Text.Label("Dashboard Overview")
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
            | Text.P("Manage your virtual instances and perform lifecycle actions.")
            | UseService<IVMService>().GetVMsAsync().Result.ToTable()
                .Width(Size.Full())
                .Header(v => v.Name, "VM Name")
                .Header(v => v.Provider, "Provider")
                .Header(v => v.Status, "Status")
                .Builder(v => v.Status, f => f.Func<VirtualMachine, VMStatus>(status => status == VMStatus.Running
                    ? new Badge("Running").Primary()
                    : new Badge("Stopped").Destructive()))
                .Header(v => v.DisplayTags, "Tags")
                .Header(v => v.LastAction, "Last Action")
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
            | Text.Label("Audit Logs")
            | Text.P("Track system actions and user activities.")
            | UseService<IVMService>().GetAuditLogsAsync().Result.ToTable()
                .Width(Size.Full())
                .Header(l => l.User, "User")
                .Header(l => l.Action, "Action")
                .Header(l => l.StartDate, "Time")
                .Align(l => l.User, Align.Left)
                .Align(l => l.Action, Align.Center)
    );

    var navHeader = new Card(
        Layout.Horizontal().Center().Padding(0, 4)
            | new TabsLayout(e => selectedIndex.Value = e.Value, null, null, null, selectedIndex.Value,
                  new Tab("Dashboard").Icon(Icons.LayoutDashboard),
                  new Tab("Virtual Machines").Icon(Icons.Monitor),
                  new Tab("Audit Logs").Icon(Icons.List),
                  new Tab("Settings").Icon(Icons.Settings)
              )
              .Variant(TabsVariant.Tabs)
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
      return selectedIndex.Value switch
      {
        0 => DashboardSection(),
        1 => VMSection(),
        2 => AuditLogSection(),
        3 => Text.Label("Settings Section"),
        _ => Text.P("Section not found")
      };
    }

    return new HeaderLayout(navHeader, GetSectionContent());
  }
}

// Removing public class HomeView : ViewBase and its content as it's merged into HomeApp

