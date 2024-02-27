using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace GSOD_DataProcessor.Models;

public partial class WeatheredContext : DbContext
{
    public WeatheredContext(DbContextOptions<WeatheredContext> options)
        : base(options) { }

    public virtual DbSet<PastWeekStationData> PastWeekStationData { get; set; }

    public virtual DbSet<NoaaArchiveUpdate> NoaaArchiveUpdates { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<PastWeekStationData>().ToCollection("past_week_station_data");
        modelBuilder.Entity<NoaaArchiveUpdate>().ToCollection("noaa_archive_update");
    }
}