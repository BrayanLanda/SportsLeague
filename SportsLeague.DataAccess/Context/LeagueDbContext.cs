using Microsoft.EntityFrameworkCore;
using SportsLeague.Domain.Entities;

namespace SportsLeague.DataAccess.Context;

public class LeagueDbContext : DbContext
{
  public LeagueDbContext(DbContextOptions<LeagueDbContext> options)
  : base(options)
  {

  }

  public DbSet<Team> Teams => Set<Team>();
  public DbSet<Player> Players => Set<Player>();
  public DbSet<Tournament> Tournaments => Set<Tournament>();
  public DbSet<TournamentTeam> TournamentTeams => Set<TournamentTeam>();
  public DbSet<Referee> Referees => Set<Referee>();

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Team>(entity =>
    {
      entity.HasKey(t => t.Id);
      entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);
      entity.Property(t => t.City)
                .IsRequired()
                .HasMaxLength(100);
      entity.Property(t => t.Stadium)
                .HasMaxLength(150);
      entity.Property(t => t.LogoUrl)
                .HasMaxLength(500);
      entity.Property(t => t.FoundedDate)
                .HasColumnType("date")
                .IsRequired();
      entity.Property(t => t.CreatedAt)
                .IsRequired();
      entity.Property(t => t.UpdatedAt)
                .IsRequired(false);
      entity.HasIndex(t => t.Name)
                .IsUnique();
    });

    modelBuilder.Entity<Player>(entity =>
    {
      entity.HasKey(p => p.Id);
      entity.Property(p => p.FirstName)
                .IsRequired()
                .HasMaxLength(80);
      entity.Property(p => p.LastName)
                .IsRequired()
                .HasMaxLength(80);
      entity.Property(p => p.BirthDate)
                .HasColumnType("date")
                .IsRequired();
      entity.Property(p => p.Number)
                .IsRequired();
      entity.Property(p => p.Position)
                .IsRequired();
      entity.Property(p => p.CreatedAt)
                .IsRequired();
      entity.Property(p => p.UpdatedAt)
                .IsRequired(false);
      entity.HasOne(p => p.Team)
                .WithMany(t => t.Players)
                .HasForeignKey(p => p.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
      entity.HasIndex(p => new { p.TeamId, p.Number })
                .IsUnique();
    });

    modelBuilder.Entity<Referee>(entity =>
        {
          entity.HasKey(r => r.Id);
          entity.Property(r => r.FirstName)
                .IsRequired()
                .HasMaxLength(80);
          entity.Property(r => r.LastName)
                .IsRequired()
                .HasMaxLength(80);
          entity.Property(r => r.Nationality)
                .IsRequired()
                .HasMaxLength(80);
          entity.Property(r => r.CreatedAt)
                .IsRequired();
          entity.Property(r => r.UpdatedAt)
                .IsRequired(false);
        });

    modelBuilder.Entity<Tournament>(entity =>
        {
          entity.HasKey(t => t.Id);
          entity.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(150);
          entity.Property(t => t.Season)
            .IsRequired()
            .HasMaxLength(20);
          entity.Property(t => t.StartDate)
            .IsRequired();
          entity.Property(t => t.EndDate)
            .IsRequired();
          entity.Property(t => t.Status)
            .IsRequired();
          entity.Property(t => t.CreatedAt)
            .IsRequired();
          entity.Property(t => t.UpdatedAt)
            .IsRequired(false);
        });

    modelBuilder.Entity<TournamentTeam>(entity =>
        {
          entity.HasKey(tt => tt.Id);
          entity.Property(tt => tt.RegisteredAt)
              .IsRequired();
          entity.Property(tt => tt.CreatedAt)
          .IsRequired();
          entity.Property(tt => tt.UpdatedAt)
              .IsRequired(false);
          entity.HasOne(tt => tt.Tournament)
              .WithMany(t => t.TournamentTeams)
              .HasForeignKey(tt => tt.TournamentId)
              .OnDelete(DeleteBehavior.Cascade);
          entity.HasOne(tt => tt.Team)
              .WithMany(t => t.TournamentTeams)
              .HasForeignKey(tt => tt.TeamId)
              .OnDelete(DeleteBehavior.Cascade);
          entity.HasIndex(tt => new { tt.TournamentId, tt.TeamId })
              .IsUnique();
        });
  }
}
