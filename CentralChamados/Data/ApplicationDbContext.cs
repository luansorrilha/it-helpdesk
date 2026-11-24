
using CentralChamados.Models;
using Microsoft.EntityFrameworkCore;

namespace CentralChamados.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Chamado> Chamados { get; set; } = null!;
    }
}
