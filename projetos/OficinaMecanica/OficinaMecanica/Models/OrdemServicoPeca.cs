namespace OficinaMecanica.Models;

// Representa a tabela intermediária "ordem_servico_pecas".
// Resolve o relacionamento N:N entre OrdemServico e Peca.
// Mesma lógica de snapshot de preço da OrdemServicoServico.

public class OrdemServicoPeca
{
    public int      OrdemId        { get; set; }   // FK → ordens_servico.id
    public int      PecaId         { get; set; }   // FK → pecas.id
    public int      Quantidade     { get; set; } = 1;
    public decimal  PrecoUnitario  { get; set; }   // snapshot do preço
    public string?  Observacao     { get; set; }

    // Propriedade de navegação.
    public Peca?    Peca           { get; set; }

    // Propriedade calculada — subtotal deste item.
    public decimal Subtotal => Quantidade * PrecoUnitario;
}
