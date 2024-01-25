using Microsoft.EntityFrameworkCore;

namespace GSOD_DataProcessor.Models;

public partial class weatheredContext : DbContext
{
    public weatheredContext()
    {
    }

    public weatheredContext(DbContextOptions<weatheredContext> options)
        : base(options)
    {
    }

    public virtual DbSet<PastWeekDatum> PastWeekData { get; set; }

    public virtual DbSet<StationLoc> StationLocs { get; set; }

    public virtual DbSet<NoaaArchiveUpdate> NoaaArchiveUpdates { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(AppSettings.WeatheredDbString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NoaaArchiveUpdate>(entity =>
        {
            entity.HasKey(e => e.NoaaArchiveUpdatesId).HasName("noaa_archive_updates_pkey");

            entity.ToTable("noaa_archive_updates");

            entity.Property(e => e.NoaaArchiveUpdatesId).HasColumnName("noaa_archive_updates_id");
            entity.Property(e => e.Filename)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("filename");
            entity.Property(e => e.Lastsiteupdate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastsiteupdate");
        });

        modelBuilder.Entity<PastWeekDatum>(entity =>
        {
            entity.HasKey(e => e.PastWeekDataId).HasName("past_week_data_pkey");

            entity.ToTable("past_week_data");

            entity.Property(e => e.PastWeekDataId).HasColumnName("past_week_data_id");
            entity.Property(e => e.LastUpdated).HasColumnName("last_updated");
            entity.Property(e => e.PrecipData).HasColumnName("precip_data");
            entity.Property(e => e.StationLocId).HasColumnName("station_loc_id");
            entity.Property(e => e.TempData).HasColumnName("temp_data");

            entity.HasOne(d => d.StationLoc).WithOne(p => p.PastWeekData)
                .HasForeignKey<PastWeekDatum>(d => d.StationLocId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_station_locs");
        });

        modelBuilder.Entity<StationLoc>(entity =>
        {
            entity.HasKey(e => e.StationLocId).HasName("station_locs_pkey");

            entity.ToTable("station_locs");

            entity.Property(e => e.StationLocId).HasColumnName("station_loc_id");
            entity.Property(e => e.Country)
                .HasMaxLength(2)
                .HasColumnName("country");
            entity.Property(e => e.Lat).HasColumnName("lat");
            entity.Property(e => e.Lon).HasColumnName("lon");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Region)
                .HasMaxLength(5)
                .HasColumnName("region");
            entity.Property(e => e.StationNumber)
                .HasMaxLength(100)
                .HasColumnName("station_number");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}