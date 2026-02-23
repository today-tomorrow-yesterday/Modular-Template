using Microsoft.EntityFrameworkCore;
using Modules.Organization.Domain;
using Modules.Organization.Domain.HomeCenters;
using Modules.Organization.Domain.ManualAssignments;
using Modules.Organization.Domain.Regions;
using Modules.Organization.Domain.Users;
using Modules.Organization.Domain.Zones;
using Modules.Organization.Infrastructure.Persistence.Configurations;
using Rtl.Core.Application.Persistence;
using Rtl.Core.Infrastructure.Persistence;

namespace Modules.Organization.Infrastructure.Persistence;

public sealed class OrganizationDbContext(DbContextOptions<OrganizationDbContext> options)
    : ModuleDbContext<OrganizationDbContext>(options), IUnitOfWork<IOrganizationModule>
{
    protected override string Schema => Schemas.Organizations;

    internal DbSet<HomeCenter> HomeCenters => Set<HomeCenter>();
    internal DbSet<User> Users => Set<User>();
    internal DbSet<UserHomeCenter> UserHomeCenters => Set<UserHomeCenter>();
    internal DbSet<Region> Regions => Set<Region>();
    internal DbSet<Zone> Zones => Set<Zone>();
    internal DbSet<ManualHomeCenterAssignment> ManualHomeCenterAssignments => Set<ManualHomeCenterAssignment>();
    internal DbSet<ManualZoneAssignment> ManualZoneAssignments => Set<ManualZoneAssignment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new HomeCenterConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserHomeCenterConfiguration());
        modelBuilder.ApplyConfiguration(new RegionConfiguration());
        modelBuilder.ApplyConfiguration(new ZoneConfiguration());
        modelBuilder.ApplyConfiguration(new ManualHomeCenterAssignmentConfiguration());
        modelBuilder.ApplyConfiguration(new ManualZoneAssignmentConfiguration());
    }
}
