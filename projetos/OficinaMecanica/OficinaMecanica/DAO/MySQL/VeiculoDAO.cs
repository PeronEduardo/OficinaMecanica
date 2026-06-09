using Dapper;
using OficinaMecanica.Infrastructure;
using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Models;

namespace OficinaMecanica.DAO.MySQL;

public class VeiculoDAO : IVeiculoDAO
{
    private readonly MySqlConnectionFactory _factory;

    public VeiculoDAO(MySqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<int> InserirAsync(Veiculo veiculo)
    {
        const string sql = @"
            INSERT INTO veiculos (cliente_id, placa, marca, modelo, ano, cor, combustivel, quilometragem, observacoes, ativo)
            VALUES (@ClienteId, @Placa, @Marca, @Modelo, @Ano, @Cor, @Combustivel, @Quilometragem, @Observacoes, @Ativo);
            SELECT LAST_INSERT_ID();";

        using var conn = await _factory.CriarAsync();
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            veiculo.ClienteId,
            veiculo.Placa,
            veiculo.Marca,
            veiculo.Modelo,
            veiculo.Ano,
            veiculo.Cor,
            Combustivel = veiculo.Combustivel?.ToString().ToLower(),
            veiculo.Quilometragem,
            veiculo.Observacoes,
            veiculo.Ativo
        });
    }

    public async Task<bool> AtualizarAsync(Veiculo veiculo)
    {
        const string sql = @"
            UPDATE veiculos
               SET cliente_id    = @ClienteId,
                   placa         = @Placa,
                   marca         = @Marca,
                   modelo        = @Modelo,
                   ano           = @Ano,
                   cor           = @Cor,
                   combustivel   = @Combustivel,
                   quilometragem = @Quilometragem,
                   observacoes   = @Observacoes,
                   ativo         = @Ativo
             WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        var linhas = await conn.ExecuteAsync(sql, new
        {
            veiculo.ClienteId,
            veiculo.Placa,
            veiculo.Marca,
            veiculo.Modelo,
            veiculo.Ano,
            veiculo.Cor,
            Combustivel = veiculo.Combustivel?.ToString().ToLower(),
            veiculo.Quilometragem,
            veiculo.Observacoes,
            veiculo.Ativo,
            veiculo.Id
        });
        return linhas > 0;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        const string sql = "DELETE FROM veiculos WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        var linhas = await conn.ExecuteAsync(sql, new { Id = id });
        return linhas > 0;
    }

    public async Task<Veiculo?> BuscarPorIdAsync(int id)
    {
        const string sql = "SELECT * FROM veiculos WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        return await conn.QueryFirstOrDefaultAsync<Veiculo>(sql, new { Id = id });
    }

    public async Task<List<Veiculo>> ListarTodosAsync()
    {
        const string sql = "SELECT * FROM veiculos WHERE ativo = 1 ORDER BY marca, modelo";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<Veiculo>(sql);
        return resultado.ToList();
    }

    public async Task<List<Veiculo>> ListarPorClienteAsync(int clienteId)
    {
        const string sql = @"
            SELECT * FROM veiculos
             WHERE cliente_id = @ClienteId
               AND ativo = 1
             ORDER BY marca, modelo";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<Veiculo>(sql, new { ClienteId = clienteId });
        return resultado.ToList();
    }

    public async Task<Veiculo?> BuscarPorPlacaAsync(string placa)
    {
        const string sql = "SELECT * FROM veiculos WHERE placa = @Placa";

        using var conn = await _factory.CriarAsync();
        return await conn.QueryFirstOrDefaultAsync<Veiculo>(sql, new { Placa = placa });
    }
}
