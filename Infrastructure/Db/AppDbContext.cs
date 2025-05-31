using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;

namespace minimal_api.Infrastructure.Db
{
    public class AppDbContext : DbContext
    {
        private readonly IConfiguration _configurationAppSettings;
        public AppDbContext(IConfiguration configurationAppSettings) //Injeção de dependência
        {
            _configurationAppSettings = configurationAppSettings;
        }

        //Mapeando as entidades para o banco de dados
        public DbSet<Administrator> Administrators { get; set; } = default!;
        public DbSet<Vehicle> Vehicles { get; set; } = default!;

        //Seed -> Função que popula o banco de dados com dados iniciais
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Administrator>().HasData(
                new Administrator
                {
                    Email = "adm@teste.com",
                    Password = "123456",
                    Profile = "Adm"
                }
            );
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configurationAppSettings.GetConnectionString("DefaultConnection")?.ToString();

                if (!string.IsNullOrEmpty(connectionString))
                    optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            }
        }
    }
}