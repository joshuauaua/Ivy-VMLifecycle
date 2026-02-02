using Ivy.VMLifecycle.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Ivy.VMLifecycle.Api.Data;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

  public DbSet<VirtualMachine> VirtualMachines { get; set; }
  public DbSet<Snapshot> Snapshots { get; set; }
  public DbSet<AuditLog> AuditLogs { get; set; }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    // Configure Tags as a JSON column for SQLite (since SQLite doesn't have a native list type)
    modelBuilder.Entity<VirtualMachine>()
        .Property(v => v.Tags)
        .HasConversion(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
        );

    // Configure Relationships
    modelBuilder.Entity<Snapshot>()
        .HasOne(s => s.VirtualMachine)
        .WithMany(v => v.Snapshots)
        .HasForeignKey(s => s.VirtualMachineId)
        .OnDelete(DeleteBehavior.Cascade);
  }
}
