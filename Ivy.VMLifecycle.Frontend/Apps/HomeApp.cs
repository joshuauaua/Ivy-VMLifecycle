using Ivy.VMLifecycle.Frontend.Models;
using Ivy.VMLifecycle.Frontend.Services;

namespace Ivy.VMLifecycle.Frontend.Apps;

[App(icon: Icons.Monitor, title: "Home")]
public class HomeApp : ViewBase
{
  public override object? Build()
  {
    var client = UseService<IClientProvider>();
    var currentSection = UseState("dashboard");
    var isLoggedIn = UseState(false);

    // Audit Log Filters
    var filterUser = UseState("All Users");
    var filterAction = UseState("All Actions");
    var filterStart = UseState<DateTime?>((DateTime?)null);
    var filterEnd = UseState<DateTime?>((DateTime?)null);

    // VM Filters
    var vmSearch = UseState("");

    var navHeader = new Card(
        Layout.Horizontal().Gap(3)
            | new Button("Dashboard")
                .Variant(currentSection.Value == "dashboard" ? ButtonVariant.Primary : ButtonVariant.Ghost)
                .HandleClick(_ =>
                {
                  currentSection.Value = "dashboard";
                })
            | new Button("Virtual Machines")
                .Variant(currentSection.Value == "vms" ? ButtonVariant.Primary : ButtonVariant.Ghost)
                .HandleClick(_ =>
                {
                  currentSection.Value = "vms";
                })
            | new Button("Audit Logs")
                .Variant(currentSection.Value == "audit-logs" ? ButtonVariant.Primary : ButtonVariant.Ghost)
                .HandleClick(_ =>
                {
                  currentSection.Value = "audit-logs";
                })
            | new Spacer()
            | new Button("Settings").Icon(Icons.Settings).Variant(ButtonVariant.Outline)
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
      return currentSection.Value switch
      {
        "dashboard" => Layout.Vertical().Gap(4)
            | Text.Label("Dashboard Overview")
            | Layout.Horizontal().Gap(4)
                | new Card(Layout.Vertical().Center() | Text.H3("5") | Text.P("Active VMs"))
                | new Card(Layout.Vertical().Center() | Text.H3("12") | Text.P("Snapshots"))
                | new Card(Layout.Vertical().Center() | Text.H3("128") | Text.P("Audit Actions")),

        "vms" => new HeaderLayout(
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
        ),

        "audit-logs" => new HeaderLayout(
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
        ),

        _ => Text.P("Section not found")
      };
    }

    return new HeaderLayout(navHeader, GetSectionContent());
  }
}

// Removing public class HomeView : ViewBase and its content as it's merged into HomeApp

