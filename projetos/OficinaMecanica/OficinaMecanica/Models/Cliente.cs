namespace OficinaMecanica.Models;

// Representa a tabela "clientes" do MySQL.
// Um cliente possui N veículos e N ordens de serviço.
// Os campos com "?" podem ser nulos (preenchimento opcional).

public class Cliente
{
    public int      Id            { get; set; }
    public string   Nome          { get; set; } = string.Empty;
    public string   CpfCnpj       { get; set; } = string.Empty;
    public string?  Telefone      { get; set; }
    public string?  Email         { get; set; }
    public string?  Endereco      { get; set; }
    public string?  Cidade        { get; set; }
    public string?  Estado        { get; set; }
    public string?  Cep           { get; set; }
    public bool     Ativo         { get; set; } = true;
    public DateTime CriadoEm     { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }

    // Propriedade de navegação — não vem do banco diretamente.
    // É preenchida pelo Service quando necessário (lazy loading manual).
    public List<Veiculo> Veiculos { get; set; } = new();
}
