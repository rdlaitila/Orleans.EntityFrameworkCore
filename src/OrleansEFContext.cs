using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Logging;

namespace Orleans.EntityFrameworkCore
{
    public class OrleansEFContext : DbContext
    {
        public DbSet<OrleansEFMembership> Memberships {get; set;}

        public DbSet<OrleansEFMembershipVersion> MembershipVersions {get; set;}

        public DbSet<OrleansEFReminder> Reminders {get; set;}

        //public DbSet<Entities.OrleansStorage> Storage {get; set;}

        public OrleansEFContext(DbContextOptions<OrleansEFContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OrleansEFMembership>()
                .HasKey(a => new { a.DeploymentId, a.Address, a.Port, a.Generation });

            modelBuilder.Entity<OrleansEFMembershipVersion>()
                .HasKey(a => a.DeploymentId);

            modelBuilder.Entity<OrleansEFReminder>()
                .HasKey(a => new { a.ServiceId, a.GrainId, a.ReminderName});
        }

        public async Task AutoMigrate()
        {
            await Database.MigrateAsync();
        }
    }
}