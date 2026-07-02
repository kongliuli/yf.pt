using Microsoft.EntityFrameworkCore;
using Yf.Pt.Models;

namespace Yf.Pt.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppProject> AppProjects => Set<AppProject>();
    public DbSet<DomainBinding> DomainBindings => Set<DomainBinding>();
    public DbSet<LocalResource> LocalResources => Set<LocalResource>();
    public DbSet<CloudResourceSnapshot> CloudResourceSnapshots => Set<CloudResourceSnapshot>();
    public DbSet<KnowledgeArticle> KnowledgeArticles => Set<KnowledgeArticle>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<AlertRecord> Alerts => Set<AlertRecord>();
    public DbSet<CostRecord> CostRecords => Set<CostRecord>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<AppProject>(e =>
        {
            e.HasIndex(x => x.Name).IsUnique();
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.Owner).HasMaxLength(50);
        });

        b.Entity<DomainBinding>(e =>
        {
            e.HasIndex(x => x.Domain);
            e.Property(x => x.Domain).IsRequired().HasMaxLength(255);
            e.Property(x => x.Type).IsRequired().HasMaxLength(20);
            e.Property(x => x.Target).IsRequired().HasMaxLength(255);
            e.Property(x => x.CloudResourceId).HasMaxLength(100);
            e.HasOne(x => x.AppProject)
                .WithMany(p => p.DomainBindings)
                .HasForeignKey(x => x.AppProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        b.Entity<LocalResource>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Type).IsRequired().HasMaxLength(20);
            e.Property(x => x.Ip).HasMaxLength(50);
            e.Property(x => x.Location).HasMaxLength(100);
            e.Property(x => x.Owner).HasMaxLength(50);
            e.Property(x => x.Note).HasMaxLength(500);
        });

        b.Entity<CloudResourceSnapshot>(e =>
        {
            e.HasIndex(x => new { x.Provider, x.ResourceId }).IsUnique();
            e.Property(x => x.ResourceId).IsRequired().HasMaxLength(100);
            e.Property(x => x.Provider).IsRequired().HasMaxLength(20);
            e.Property(x => x.Type).IsRequired().HasMaxLength(20);
            e.Property(x => x.Name).IsRequired().HasMaxLength(100);
            e.Property(x => x.Status).IsRequired().HasMaxLength(20);
        });

        b.Entity<KnowledgeArticle>(e =>
        {
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Category).IsRequired().HasMaxLength(20);
            e.Property(x => x.Tags).HasMaxLength(200);
            e.Property(x => x.Author).HasMaxLength(50);
            e.HasOne(x => x.AppProject)
                .WithMany()
                .HasForeignKey(x => x.AppProjectId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        b.Entity<Certificate>(e =>
        {
            e.Property(x => x.Domain).IsRequired().HasMaxLength(255);
            e.Property(x => x.Issuer).IsRequired().HasMaxLength(100);
            e.Property(x => x.Note).HasMaxLength(500);
        });

        b.Entity<AlertRecord>(e =>
        {
            e.Property(x => x.ResourceId).IsRequired().HasMaxLength(100);
            e.Property(x => x.ResourceName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Severity).IsRequired().HasMaxLength(20);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Status).IsRequired().HasMaxLength(20);
            e.HasIndex(x => x.Status);
        });

        b.Entity<CostRecord>(e =>
        {
            e.Property(x => x.ResourceId).IsRequired().HasMaxLength(100);
            e.Property(x => x.ResourceName).IsRequired().HasMaxLength(100);
            e.Property(x => x.Currency).IsRequired().HasMaxLength(10);
            e.Property(x => x.Period).IsRequired().HasMaxLength(10);
            e.Property(x => x.Category).IsRequired().HasMaxLength(20);
            e.HasOne(x => x.AppProject)
                .WithMany()
                .HasForeignKey(x => x.AppProjectId)
                .OnDelete(DeleteBehavior.SetNull);
            e.HasIndex(x => x.Period);
        });
    }
}
