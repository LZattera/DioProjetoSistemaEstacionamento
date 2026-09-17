using Microsoft.EntityFrameworkCore;
using SistemaEstacionamento.Features.Carro;
using SistemaEstacionamento.Features.Registro;

namespace SistemaEstacionamento.Data // Verifique se este é o seu namespace correto
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        // Suas tabelas
        public DbSet<Carro> Carros { get; set; }
        public DbSet<Registro> Registros { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Suas configurações de tabelas (se houver)
        }
    }
}