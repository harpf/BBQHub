using BBQHub.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BBQHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BBQHub.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Juror> Juroren { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Durchgang> Durchgaenge { get; set; }
        public DbSet<Kriterium> Kriterien { get; set; }
        public DbSet<Bewertung> Bewertungen { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<EventTeamAssignment> EventTeamAssignments { get; set; }
        public DbSet<EventLogo> EventLogos { get; set; }
        public DbSet<SpontanTeilnahme> spontanTeilnahmen { get; set; }
        public DbSet<SmtpSettings> SmtpSettings { get; set; }
        public DbSet<LogEntry> Logs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 🔐 Beziehung: Event -> Manager
            builder.Entity<Event>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            // ⚖️ Bewertung -> Kriterium
            builder.Entity<Bewertung>()
                .HasOne(b => b.Kriterium)
                .WithMany()
                .HasForeignKey(b => b.KriteriumId)
                .OnDelete(DeleteBehavior.Restrict);

            // 📋 Bewertung -> Durchgang
            builder.Entity<Bewertung>()
                .HasOne(b => b.Durchgang)
                .WithMany(d => d.Bewertungen)
                .HasForeignKey(b => b.DurchgangId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🧑‍⚖️ Bewertung -> Juror
            builder.Entity<Bewertung>()
                .HasOne(b => b.Juror)
                .WithMany()
                .HasForeignKey(b => b.JurorId)
                .OnDelete(DeleteBehavior.Cascade);

            // 📄 Serilog Logs-Tabelle einbinden
            builder.Entity<LogEntry>(entity =>
            {
                entity.ToTable("Logs"); // Name wie von Serilog erzeugt
                entity.HasNoKey(); // Falls kein Primärschlüssel existiert
                entity.Metadata.SetIsTableExcludedFromMigrations(true);
            });
        }
    }
}