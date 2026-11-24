using System;
using System.ComponentModel.DataAnnotations;

namespace CentralChamados.Models
{
    public class Chamado
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Informe o título.")]
        [StringLength(100)]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a descrição.")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o solicitante.")]
        public string Solicitante { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a prioridade.")]
        public string Prioridade { get; set; } = "Média";

        [Required(ErrorMessage = "Informe o status.")]
        public string Status { get; set; } = "Aberto";

        [Display(Name = "Data de abertura")]
        public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
    }
}
