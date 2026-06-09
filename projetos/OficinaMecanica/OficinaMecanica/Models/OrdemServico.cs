namespace OficinaMecanica.Models;

// Representa a tabela "ordens_servico" do MySQL.
// É a entidade central do sistema — vincula cliente, veículo
// e usuário responsável, além de listar serviços e peças.
//
// Relacionamentos:
//   Cliente  1:N  OrdemServico  (ClienteId é FK)
//   Veiculo  1:N  OrdemServico  (VeiculoId é FK)
//   Usuario  1:N  OrdemServico  (UsuarioId é FK)
//   OrdemServico N:N Servico    (via OrdemServicoServico)
//   OrdemServico N:N Peca       (via OrdemServicoPeca)

public class OrdemServico
{
    public int       Id                    { get; set; }
    public int       ClienteId             { get; set; }   // FK → clientes.id
    public int       VeiculoId             { get; set; }   // FK → veiculos.id
    public int       UsuarioId             { get; set; }   // FK → usuarios.id
    public DateTime  DataAbertura          { get; set; } = DateTime.Today;
    public DateTime? DataPrevisao          { get; set; }
    public DateTime? DataFechamento        { get; set; }
    public StatusOrdem Status             { get; set; } = StatusOrdem.Aberta;
    public int?      QuilometragemEntrada  { get; set; }
    public string?   Observacoes           { get; set; }
    public decimal   Desconto              { get; set; } = 0;
    public decimal   Total                 { get; set; } = 0;
    public DateTime  CriadoEm             { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm         { get; set; }

    // Propriedades de navegação — preenchidas pelo Service.
    public Cliente?  Cliente               { get; set; }
    public Veiculo?  Veiculo               { get; set; }
    public Usuario?  Usuario               { get; set; }

    // Listas dos itens N:N — preenchidas pelo DAO quando solicitado.
    public List<OrdemServicoServico> Servicos { get; set; } = new();
    public List<OrdemServicoPeca>    Pecas    { get; set; } = new();

    // Propriedades extras vindas da view vw_ordens_resumo
    public string? ClienteNome { get; set; }
    public string? VeiculoPlaca { get; set; }
    public string? VeiculoMarca { get; set; }
    public string? VeiculoModelo { get; set; }
    public string? Atendente { get; set; }
}

// Espelha o ENUM do MySQL para o campo "status".
public enum StatusOrdem
{
    Aberta,
    EmAndamento,
    AguardandoPeca,
    Concluida,
    Cancelada
}
