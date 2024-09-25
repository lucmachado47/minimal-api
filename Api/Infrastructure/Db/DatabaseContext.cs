using Microsoft.EntityFrameworkCore;
using minimal_api.Domain.Entities;

namespace minimal_api.Infrastructure.Db
{   
    
    public class DatabaseContext : DbContext
    {   
        private readonly IConfiguration _configuracaoAppSettings;
        public DatabaseContext(IConfiguration configuracaoAppSettings)
        {
            _configuracaoAppSettings = configuracaoAppSettings;
        }

        public DbSet<Adm> Administradores { get; set; }

        public DbSet<Vehicle> Veiculos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Adm>().HasData(
                new Adm {
                    Id = 1,
                    Email = "adm@teste.com",
                    Senha = "123456",
                    Perfil = "Adm"
                }
            );
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {   
            if(!optionsBuilder.IsConfigured)
            {
                var stringConexao = _configuracaoAppSettings.GetConnectionString("mysql").ToString();
                if(!string.IsNullOrEmpty(stringConexao))
                {
                    optionsBuilder.UseMySql(
                        stringConexao, 
                        ServerVersion.AutoDetect(stringConexao)
                    );
                }
            }
        }
    }
}