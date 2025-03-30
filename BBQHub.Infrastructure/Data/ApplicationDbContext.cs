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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<Event>()
                .HasOne<IdentityUser>()
                .WithMany()
                .HasForeignKey(e => e.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Bewertung>()
                .HasOne(b => b.Kriterium)
                .WithMany()
                .HasForeignKey(b => b.KriteriumId)
                .OnDelete(DeleteBehavior.Restrict); // oder NoAction

            builder.Entity<Bewertung>()
                .HasOne(b => b.Durchgang)
                .WithMany(d => d.Bewertungen)
                .HasForeignKey(b => b.DurchgangId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Bewertung>()
                .HasOne(b => b.Juror)
                .WithMany()
                .HasForeignKey(b => b.JurorId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Juror> Juroren { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Durchgang> Durchgaenge { get; set; }
        public DbSet<Kriterium> Kriterien { get; set; }
        public DbSet<Bewertung> Bewertungen { get; set; }


    }

}