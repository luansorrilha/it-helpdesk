using CentralChamados.Data;
using CentralChamados.Hubs;
using CentralChamados.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CentralChamados.Controllers
{
    public class ChamadosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ChamadosHub> _hubContext;

        public ChamadosController(
            ApplicationDbContext context,
            IHubContext<ChamadosHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Listagem. NoStore: não pode ser cacheada, pois o SignalR busca esta página
        // para atualizar a tabela e precisa sempre dos dados mais recentes.
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> Index()
        {
            var chamados = await _context.Chamados
                .AsNoTracking()
                .OrderByDescending(c => c.DataAbertura)
                .ToListAsync();

            return View(chamados);
        }

        // Detalhes de um chamado
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var chamado = await _context.Chamados.FindAsync(id);

            if (chamado == null)
            {
                return NotFound();
            }

            return View(chamado);
        }

        // Formulário de cadastro
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Salva um novo chamado e avisa todos os navegadores via SignalR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Titulo,Descricao,Solicitante,Prioridade")] Chamado chamado)
        {
            if (!ChamadoOpcoes.PrioridadesValidas.Contains(chamado.Prioridade))
            {
                ModelState.AddModelError(nameof(Chamado.Prioridade), "Prioridade inválida.");
            }

            if (!ModelState.IsValid)
            {
                return View(chamado);
            }

            chamado.Status = "Aberto";
            chamado.DataAbertura = DateTime.UtcNow;

            _context.Chamados.Add(chamado);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync(
                "ChamadoCriado",
                new { id = chamado.Id, titulo = chamado.Titulo });

            return RedirectToAction(nameof(Index));
        }

        // Formulário de edição
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var chamado = await _context.Chamados.FindAsync(id);

            if (chamado == null)
            {
                return NotFound();
            }

            return View(chamado);
        }

        // Salva a edição. Se o status mudou, notifica "StatusAlterado"; senão, "ChamadoEditado".
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Titulo,Descricao,Solicitante,Prioridade,Status")] Chamado formulario)
        {
            if (id != formulario.Id)
            {
                return NotFound();
            }

            if (!ChamadoOpcoes.PrioridadesValidas.Contains(formulario.Prioridade))
            {
                ModelState.AddModelError(nameof(Chamado.Prioridade), "Prioridade inválida.");
            }

            if (!ChamadoOpcoes.StatusValidos.Contains(formulario.Status))
            {
                ModelState.AddModelError(nameof(Chamado.Status), "Status inválido.");
            }

            if (!ModelState.IsValid)
            {
                return View(formulario);
            }

            var chamado = await _context.Chamados.FindAsync(id);

            if (chamado == null)
            {
                return NotFound();
            }

            var statusAnterior = chamado.Status;

            // Copia só os campos editáveis (a DataAbertura é preservada)
            chamado.Titulo = formulario.Titulo;
            chamado.Descricao = formulario.Descricao;
            chamado.Solicitante = formulario.Solicitante;
            chamado.Prioridade = formulario.Prioridade;
            chamado.Status = formulario.Status;

            await _context.SaveChangesAsync();

            if (statusAnterior != chamado.Status)
            {
                await _hubContext.Clients.All.SendAsync(
                    "StatusAlterado",
                    new { id = chamado.Id, status = chamado.Status });
            }
            else
            {
                await _hubContext.Clients.All.SendAsync(
                    "ChamadoEditado",
                    new { id = chamado.Id, titulo = chamado.Titulo });
            }

            return RedirectToAction(nameof(Index));
        }

        // Tela de confirmação de exclusão
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var chamado = await _context.Chamados.FindAsync(id);

            if (chamado == null)
            {
                return NotFound();
            }

            return View(chamado);
        }

        // Exclui o chamado e avisa os navegadores
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chamado = await _context.Chamados.FindAsync(id);

            if (chamado == null)
            {
                return NotFound();
            }

            _context.Chamados.Remove(chamado);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync(
                "ChamadoExcluido",
                new { id = chamado.Id, titulo = chamado.Titulo });

            return RedirectToAction(nameof(Index));
        }

        // Altera apenas o status (tela de detalhes)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarStatus(int id, string status)
        {
            if (!ChamadoOpcoes.StatusValidos.Contains(status))
            {
                return BadRequest("Status inválido.");
            }

            var chamado = await _context.Chamados.FindAsync(id);

            if (chamado == null)
            {
                return NotFound();
            }

            chamado.Status = status;
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync(
                "StatusAlterado",
                new { id = chamado.Id, status = chamado.Status });

            return RedirectToAction(nameof(Details), new { id });
        }

        // Página de demonstração do cache HTTP / ETag / 304
        [HttpGet]
        public IActionResult DemoHttp()
        {
            return View();
        }
    }
}
