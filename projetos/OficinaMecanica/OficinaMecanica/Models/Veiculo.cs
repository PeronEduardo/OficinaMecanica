namespace OficinaMecanica.Models;

// Representa a tabela "veiculos" do MySQL.
// Relacionamento: cada veículo pertence a 1 cliente (ClienteId é FK).
// Um cliente pode ter N veículos.

public class Veiculo
{
    public int      Id             { get; set; }
    public int      ClienteId      { get; set; }      // FK → clientes.id
    public string   Placa          { get; set; } = string.Empty;
    public string   Marca          { get; set; } = string.Empty;
    public string   Modelo         { get; set; } = string.Empty;
    public int      Ano            { get; set; }
    public string?  Cor            { get; set; }
    public TipoCombustivel? Combustivel { get; set; }
    public int?     Quilometragem  { get; set; }
    public string?  Observacoes    { get; set; }
    public bool     Ativo          { get; set; } = true;
    public DateTime CriadoEm      { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }

    // Propriedade de navegação — carregada pelo Service quando necessário.
    public Cliente? Cliente        { get; set; }
    public string PlacaModelo => $"{Placa} — {Marca} {Modelo} {Ano}";
}

// Espelha o ENUM do MySQL para o campo "combustivel".
public enum TipoCombustivel
{
    Gasolina,
    Etanol,
    Flex,
    Diesel,
    Eletrico,
    Hibrido
}
