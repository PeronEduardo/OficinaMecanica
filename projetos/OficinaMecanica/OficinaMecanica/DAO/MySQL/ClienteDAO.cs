using Dapper;
using OficinaMecanica.Infrastructure;
using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Models;

namespace OficinaMecanica.DAO.MySQL;

public class ClienteDAO : IClienteDAO
{
    private readonly MySqlConnectionFactory _factory;

    public ClienteDAO(MySqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<int> InserirAsync(Cliente cliente)
    {
        const string sql = @"
            INSERT INTO clientes (nome, cpf_cnpj, telefone, email, endereco, cidade, estado, cep, ativo)
            VALUES (@Nome, @CpfCnpj, @Telefone, @Email, @Endereco, @Cidade, @Estado, @Cep, @Ativo);
            SELECT LAST_INSERT_ID();";

        using var conn = await _factory.CriarAsync();
        return await conn.ExecuteScalarAsync<int>(sql, cliente);
    }

    public async Task<bool> AtualizarAsync(Cliente cliente)
    {
        const string sql = @"
            UPDATE clientes
               SET nome      = @Nome,
                   cpf_cnpj  = @CpfCnpj,
                   telefone  = @Telefone,
                   email     = @Email,
                   endereco  = @Endereco,
                   cidade    = @Cidade,
                   estado    = @Estado,
                   cep       = @Cep,
                   ativo     = @Ativo
             WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        var linhas = await conn.ExecuteAsync(sql, cliente);
        return linhas > 0;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        // O MySQL vai bloquear a exclusão se existir veículo ou
        // ordem vinculada — graças ao ON DELETE RESTRICT do script SQL.
        // O Service trata essa exceção e exibe mensagem amigável.
        const string sql = "DELETE FROM clientes WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        var linhas = await conn.ExecuteAsync(sql, new { Id = id });
        return linhas > 0;
    }

    public async Task<Cliente?> BuscarPorIdAsync(int id)
    {
        const string sql = "SELECT * FROM clientes WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        return await conn.QueryFirstOrDefaultAsync<Cliente>(sql, new { Id = id });
    }

    public async Task<List<Cliente>> ListarTodosAsync()
    {
        const string sql = "SELECT * FROM clientes WHERE ativo = 1 ORDER BY nome";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<Cliente>(sql);
        return resultado.ToList();
    }

    // Busca por parte do nome — o % é o coringa do SQL LIKE.
    // "LIKE '%joão%'" encontra qualquer nome que contenha "joão".
    public async Task<List<Cliente>> BuscarPorNomeAsync(string nome)
    {
        const string sql = @"
            SELECT * FROM clientes
             WHERE nome LIKE @Nome
               AND ativo = 1
             ORDER BY nome";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<Cliente>(sql, new { Nome = $"%{nome}%" });
        return resultado.ToList();
    }

    public async Task<Cliente?> BuscarPorCpfCnpjAsync(string cpfCnpj)
    {
        const string sql = "SELECT * FROM clientes WHERE cpf_cnpj = @CpfCnpj";

        using var conn = await _factory.CriarAsync();
        return await conn.QueryFirstOrDefaultAsync<Cliente>(sql, new { CpfCnpj = cpfCnpj });
    }
}
