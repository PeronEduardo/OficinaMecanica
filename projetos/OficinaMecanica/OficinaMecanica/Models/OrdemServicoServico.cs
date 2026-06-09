namespace OficinaMecanica.Models;

// Representa a tabela intermediária "ordem_servico_servicos".
// Resolve o relacionamento N:N entre OrdemServico e Servico.
//
// Por que PrecoUnitario aqui e não pegar do Servico diretamente?
// Porque o preço do serviço pode mudar no futuro. Ao guardar o
// preço no momento da venda (snapshot), o histórico fica correto
// mesmo que o preço seja alterado depois.

public class OrdemServicoServico
{
    public int      OrdemId        { get; set; }   // FK → ordens_servico.id
    public int      ServicoId      { get; set; }   // FK → servicos.id
    public int      Quantidade     { get; set; } = 1;
    public decimal  PrecoUnitario  { get; set; }   // snapshot do preço
    public string?  Observacao     { get; set; }

    // Propriedades de navegação.
    public Servico? Servico        { get; set; }

    // Propriedade calculada — subtotal deste item.
    public decimal Subtotal => Quantidade * PrecoUnitario;

    // Propriedades extras mapeadas da view vw_ordens_resumo
    public string? ClienteNome { get; set; }
    public string? VeiculoPlaca { get; set; }
    public string? VeiculoMarca { get; set; }
    public string? VeiculoModelo { get; set; }
    public string? Atendente { get; set; }
}
