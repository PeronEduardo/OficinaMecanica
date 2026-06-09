using Dapper;
using OficinaMecanica.Infrastructure;
using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Models;

namespace OficinaMecanica.DAO.MySQL;

// ════════════════════════════════════════════════════════════════
// ServicoDAO
// ════════════════════════════════════════════════════════════════
public class ServicoDAO : IServicoDAO
{
    private readonly MySqlConnectionFactory _factory;

    public ServicoDAO(MySqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<int> InserirAsync(Servico servico)
    {
        const string sql = @"
            INSERT INTO servicos (descricao, preco, tempo_estimado_min, ativo)
            VALUES (@Descricao, @Preco, @TempoEstimadoMin, @Ativo);
            SELECT LAST_INSERT_ID();";

        using var conn = await _factory.CriarAsync();
        return await conn.ExecuteScalarAsync<int>(sql, servico);
    }

    public async Task<bool> AtualizarAsync(Servico servico)
    {
        const string sql = @"
            UPDATE servicos
               SET descricao         = @Descricao,
                   preco             = @Preco,
                   tempo_estimado_min = @TempoEstimadoMin,
                   ativo             = @Ativo
             WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        var linhas = await conn.ExecuteAsync(sql, servico);
        return linhas > 0;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        const string sql = "DELETE FROM servicos WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        var linhas = await conn.ExecuteAsync(sql, new { Id = id });
        return linhas > 0;
    }

    public async Task<Servico?> BuscarPorIdAsync(int id)
    {
        const string sql = "SELECT * FROM servicos WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        return await conn.QueryFirstOrDefaultAsync<Servico>(sql, new { Id = id });
    }

    public async Task<List<Servico>> ListarTodosAsync()
    {
        const string sql = "SELECT * FROM servicos ORDER BY descricao";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<Servico>(sql);
        return resultado.ToList();
    }

    public async Task<List<Servico>> BuscarPorDescricaoAsync(string descricao)
    {
        const string sql = @"
            SELECT * FROM servicos
             WHERE descricao LIKE @Descricao
               AND ativo = 1
             ORDER BY descricao";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<Servico>(sql, new { Descricao = $"%{descricao}%" });
        return resultado.ToList();
    }

    public async Task<List<Servico>> ListarAtivosAsync()
    {
        const string sql = "SELECT * FROM servicos WHERE ativo = 1 ORDER BY descricao";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<Servico>(sql);
        return resultado.ToList();
    }
}

// ════════════════════════════════════════════════════════════════
// PecaDAO
// ════════════════════════════════════════════════════════════════
public class PecaDAO : IPecaDAO
{
    private readonly MySqlConnectionFactory _factory;

    public PecaDAO(MySqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<int> InserirAsync(Peca peca)
    {
        const string sql = @"
            INSERT INTO pecas (descricao, codigo, preco, estoque, estoque_minimo, ativo)
            VALUES (@Descricao, @Codigo, @Preco, @Estoque, @EstoqueMinimo, @Ativo);
            SELECT LAST_INSERT_ID();";

        using var conn = await _factory.CriarAsync();
        return await conn.ExecuteScalarAsync<int>(sql, peca);
    }

    public async Task<bool> AtualizarAsync(Peca peca)
    {
        const string sql = @"
            UPDATE pecas
               SET descricao      = @Descricao,
                   codigo         = @Codigo,
                   preco          = @Preco,
                   estoque        = @Estoque,
                   estoque_minimo = @EstoqueMinimo,
                   ativo          = @Ativo
             WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        var linhas = await conn.ExecuteAsync(sql, peca);
        return linhas > 0;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        const string sql = "DELETE FROM pecas WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        var linhas = await conn.ExecuteAsync(sql, new { Id = id });
        return linhas > 0;
    }

    public async Task<Peca?> BuscarPorIdAsync(int id)
    {
        const string sql = "SELECT * FROM pecas WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        return await conn.QueryFirstOrDefaultAsync<Peca>(sql, new { Id = id });
    }

    public async Task<List<Peca>> ListarTodosAsync()
    {
        const string sql = "SELECT * FROM pecas ORDER BY descricao";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<Peca>(sql);
        return resultado.ToList();
    }

    public async Task<List<Peca>> BuscarPorDescricaoAsync(string descricao)
    {
        const string sql = @"
            SELECT * FROM pecas
             WHERE (descricao LIKE @Termo OR codigo LIKE @Termo)
               AND ativo = 1
             ORDER BY descricao";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<Peca>(sql, new { Termo = $"%{descricao}%" });
        return resultado.ToList();
    }

    public async Task<List<Peca>> ListarAtivosAsync()
    {
        const string sql = "SELECT * FROM pecas WHERE ativo = 1 ORDER BY descricao";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<Peca>(sql);
        return resultado.ToList();
    }

    public async Task<List<Peca>> ListarEstoqueBaixoAsync()
    {
        // Busca as peças onde o estoque atual está abaixo do mínimo.
        // Usamos a view que criamos no script SQL.
        const string sql = "SELECT * FROM vw_pecas_estoque_baixo";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<Peca>(sql);
        return resultado.ToList();
    }
}
