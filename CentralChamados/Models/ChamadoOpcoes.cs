namespace CentralChamados.Models
{
    // Valores aceitos para prioridade e status (usados no Controller e nas Views)
    public static class ChamadoOpcoes
    {
        public static readonly string[] PrioridadesValidas = { "Baixa", "Média", "Alta" };

        public static readonly string[] StatusValidos = { "Aberto", "Em andamento", "Resolvido" };

        // Cor do badge do Bootstrap conforme o status
        public static string ClasseBadge(string status) => status switch
        {
            "Aberto" => "bg-primary",
            "Em andamento" => "bg-warning text-dark",
            "Resolvido" => "bg-success",
            _ => "bg-secondary"
        };
    }
}
