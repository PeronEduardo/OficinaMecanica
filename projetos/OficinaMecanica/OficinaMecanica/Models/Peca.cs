namespace OficinaMecanica.Models;

// Representa a tabela "pecas" do MySQL.
// É o catálogo de peças disponíveis em estoque.
// Possui relacionamento N:N com OrdemServico
// através da tabela intermediária "ordem_servico_pecas".

public class Peca
{
    public int      Id              { get; set; }
    public string   Descricao       { get; set; } = string.Empty;
    public string?  Codigo          { get; set; }      // código do fabricante
    public decimal  Preco           { get; set; }
    public int      Estoque         { get; set; }
    public int      EstoqueMinimo   { get; set; } = 1; // abaixo disso → alerta
    public bool     Ativo           { get; set; } = true;
    public DateTime CriadoEm       { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm  { get; set; }

    // Propriedade calculada — não vem do banco.
    // Retorna true se o estoque está abaixo do mínimo.
    public bool EstoqueBaixo => Estoque < EstoqueMinimo;
}
