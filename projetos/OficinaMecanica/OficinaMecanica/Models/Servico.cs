namespace OficinaMecanica.Models;

// Representa a tabela "servicos" do MySQL.
// É o catálogo de serviços oferecidos pela oficina.
// Possui relacionamento N:N com OrdemServico
// através da tabela intermediária "ordem_servico_servicos".

public class Servico
{
    public int      Id                 { get; set; }
    public string   Descricao          { get; set; } = string.Empty;
    public decimal  Preco              { get; set; }
    public int?     TempoEstimadoMin   { get; set; }   // tempo em minutos
    public bool     Ativo              { get; set; } = true;
    public DateTime CriadoEm          { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm     { get; set; }
}
