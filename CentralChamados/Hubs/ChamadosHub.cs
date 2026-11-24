using System.Runtime.CompilerServices;
using CentralChamados.Data;
using CentralChamados.Models;
using Microsoft.AspNetCore.SignalR;

namespace CentralChamados.Hubs
{
    public class ChamadosHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChamadosHub(ApplicationDbContext context)
        {
            _context = context;
        }

        // STREAMING (servidor -> cliente):
        // enquanto o cliente estiver inscrito, envia o resumo a cada 2 segundos.
        // O cliente chama com: conexao.stream("StreamResumo")
        public async IAsyncEnumerable<ResumoChamados> StreamResumo(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                yield return await ResumoChamados.CalcularAsync(_context, cancellationToken);

                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            }
        }
    }
}
