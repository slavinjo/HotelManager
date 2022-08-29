using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Hotel.Server.Users;
using Hotel.Server.Hotels;

namespace Hotel.Server.Data;

public class HotelContext : DbContext
{
    public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
    private readonly DbUserTrackingService _dbUserTrackingService;

    public HotelContext(DbContextOptions<HotelContext> options, DbUserTrackingService dbUserTrackingService) :
        base(options)
    {
        _dbUserTrackingService = dbUserTrackingService;
    }

    public DbSet<User> Users { get; set; }

    public DbSet<Hotel.Server.Hotels.Hotel> Hotels { get; set; }

    public void seedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasData(User.SYSTEM_USER);
    }

    private void disableCascadeDeletes(ModelBuilder modelBuilder)
    {
        var cascadeFKs = modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

        foreach (var fk in cascadeFKs)
            fk.DeleteBehavior = DeleteBehavior.Restrict;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        DbFormatter.SetDefaultValues(modelBuilder);
        DbFormatter.FormatTableNames(modelBuilder);
        DbFormatter.FormatColumnsSnakeCase(modelBuilder);
        disableCascadeDeletes(modelBuilder);
        seedData(modelBuilder);
    }

    public override int SaveChanges()
    {
        SoftDelete.ProcessSoftDeletedItems(ChangeTracker);

        // this will be null while seeding data and creating initial tenant users
        if (_dbUserTrackingService != null)
        {
            UserChangeTracker.ProcessUserChangeTrackedItems(ChangeTracker,
                _dbUserTrackingService.GetCurrentUserId(User.SYSTEM_USER.Id));
        }

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
    {
        SoftDelete.ProcessSoftDeletedItems(ChangeTracker);

        // this will be null while seeding data and creating initial tenant users
        if (_dbUserTrackingService != null)
        {
            UserChangeTracker.ProcessUserChangeTrackedItems(ChangeTracker,
                _dbUserTrackingService.GetCurrentUserId(User.SYSTEM_USER.Id));
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
