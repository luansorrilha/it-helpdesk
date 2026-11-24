using CentralChamados.Data;
using Microsoft.EntityFrameworkCore;

namespace CentralChamados.Models
{
    // Resumo com a quantidade de chamados por status.
    // Usado pelo streaming do SignalR e pelo endpoint com ETag (/api/chamados/resumo).
    public record ResumoChamados(int Abertos, int EmAndamento, int Resolvidos, int Total)
    {
        public static async Task<ResumoChamados> CalcularAsync(
            ApplicationDbContext context,
            CancellationToken cancellationToken = default)
        {
            var contagens = await context.Chamados
                .AsNoTracking()
                .GroupBy(c => c.Status)
                .Select(g => new { Status = g.Key, Quantidade = g.Count() })
                .ToListAsync(cancellationToken);

            int Contar(string status) =>
                contagens.Where(c => c.Status == status).Sum(c => c.Quantidade);

            return new ResumoChamados(
                Abertos: Contar("Aberto"),
                EmAndamento: Contar("Em andamento"),
                Resolvidos: Contar("Resolvido"),
                Total: contagens.Sum(c => c.Quantidade));
        }
    }
}
