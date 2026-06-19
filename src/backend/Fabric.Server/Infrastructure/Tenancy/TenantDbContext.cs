using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Fabric.Server.Tenants.Domain;

namespace Fabric.Server.Infrastructure.Tenancy;

public abstract class TenantDbContext : DbContext
{
    public const string TenantIdPropertyName = "TenantId";
    public const string TenantIdColumnName = "tenant_id";

    private readonly ITenantContext _tenantContext;

    protected TenantDbContext(DbContextOptions options, ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    protected TenantDbContext()
    {
        _tenantContext = new DesignTimeTenantContext();
    }

    public string TenantId => _tenantContext.TenantId;

    public static PropertyBuilder<string> ConfigureTenantProperty(EntityTypeBuilder builder) =>
        builder.Property<string>(TenantIdPropertyName)
            .HasColumnName(TenantIdColumnName)
            .HasMaxLength(100)
            .IsRequired();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        SetTenantIds();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        SetTenantIds();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    protected void ApplyTenantFilters(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType is null
                || entityType.BaseType is not null
                || entityType.HasSharedClrType
                || entityType.IsOwned()
                || entityType.GetTableName() is null)
                continue;

            ConfigureTenantProperty(modelBuilder.Entity(entityType.ClrType));

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(CreateTenantFilter(entityType.ClrType));

            modelBuilder.Entity(entityType.ClrType)
                .HasIndex(TenantIdPropertyName)
                .HasDatabaseName($"ix_{entityType.GetTableName()}_tenant_id");
        }
    }

    private LambdaExpression CreateTenantFilter(Type entityType)
    {
        ParameterExpression parameter = Expression.Parameter(entityType, "entity");
        MethodCallExpression tenantProperty = Expression.Call(
            typeof(EF),
            nameof(EF.Property),
            [typeof(string)],
            parameter,
            Expression.Constant(TenantIdPropertyName));
        MemberExpression tenantId = Expression.Property(Expression.Constant(this), nameof(TenantId));
        BinaryExpression body = Expression.Equal(tenantProperty, tenantId);

        return Expression.Lambda(body, parameter);
    }

    private void SetTenantIds()
    {
        foreach (EntityEntry entry in ChangeTracker.Entries())
        {
            if (entry.State != EntityState.Added || entry.Metadata.FindProperty(TenantIdPropertyName) is null)
                continue;

            entry.Property(TenantIdPropertyName).CurrentValue = TenantId;
        }
    }

    private sealed class DesignTimeTenantContext : ITenantContext
    {
        public string TenantId => TenantContext.DefaultTenantId;
        public TenantConfiguration Configuration { get; } = new()
        {
            Oidc = new OidcSettings
            {
                MetadataUrl = "http://localhost/.well-known/openid-configuration",
                ClientId = "fabric",
                RequireHttpsMetadata = false
            }
        };
    }
}
