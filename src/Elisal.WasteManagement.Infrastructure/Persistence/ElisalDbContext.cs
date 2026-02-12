using Elisal.WasteManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Elisal.WasteManagement.Infrastructure.Persistence;

public class ElisalDbContext : DbContext
{
    public ElisalDbContext(DbContextOptions<ElisalDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<WasteType> WasteTypes { get; set; }
    public DbSet<CollectionPoint> CollectionPoints { get; set; }
    public DbSet<CollectionRecord> CollectionRecords { get; set; }
    public DbSet<Cooperative> Cooperatives { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Route> Routes { get; set; }
    public DbSet<RoutePoint> RoutePoints { get; set; }
    public DbSet<RouteExecution> RouteExecutions { get; set; }
    public DbSet<RoutePointExecutionStatus> RoutePointExecutionStatuses { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<UserToken> UserTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<RoutePoint>()
            .HasKey(rp => new { rp.RouteId, rp.CollectionPointId });

        modelBuilder.Entity<Cooperative>().HasData(
            new Cooperative 
            { 
                Id = 999, 
                Name = "Elisal - Reaproveitamento Interno", 
                Contact = "N/A", 
                Email = "interno@elisal.co.ao", 
                Address = "Elisal Sede", 
                AcceptedWasteTypes = "Todos" 
            }
        );

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ElisalDbContext).Assembly);
    }
}
