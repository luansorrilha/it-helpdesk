using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CentralChamados.Data;
using CentralChamados.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace CentralChamados.Controllers
{
    // Endpoint de LEITURA usado para demonstrar cache HTTP, ETag e 304.
    // Fica separado da listagem do SignalR para o cache não atrapalhar o tempo real.
    [ApiController]
    [Route("api/chamados")]
    public class ApiChamadosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiChamadosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET /api/chamados/resumo
        // [ResponseCache] adiciona "Cache-Control: public,max-age=5" e permite que o
        // middleware UseResponseCaching guarde a resposta no servidor por 5 segundos.
        [HttpGet("resumo")]
        [ResponseCache(Duration = 5, Location = ResponseCacheLocation.Any)]
        public async Task<IActionResult> ObterResumo(CancellationToken cancellationToken)
        {
            var resumo = await ResumoChamados.CalcularAsync(_context, cancellationToken);

            // 1) ETag = "impressão digital" do conteúdo (hash do JSON do resumo)
            var json = JsonSerializer.Serialize(resumo);
            var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json)))[..16];
            var etag = new EntityTagHeaderValue($"\"{hash}\"");

            Response.GetTypedHeaders().ETag = etag;

            // 2) Validação condicional: se o cliente enviou If-None-Match com o mesmo ETag,
            //    o conteúdo não mudou -> 304 Not Modified, sem corpo.
            var enviados = Request.GetTypedHeaders().IfNoneMatch;
            if (enviados.Any(e => e.Compare(etag, useStrongComparison: false)))
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            // 3) Caso contrário: 200 OK com o conteúdo e o ETag no cabeçalho.
            return Ok(resumo);
        }
    }
}
