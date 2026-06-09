using Dapper;
using OficinaMecanica.Infrastructure;
using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Models;

namespace OficinaMecanica.DAO.MySQL;

public class UsuarioDAO : IUsuarioDAO
{
    private readonly MySqlConnectionFactory _factory;

    public UsuarioDAO(MySqlConnectionFactory factory)
    {
        _factory = factory;
    }

    // SQL com aliases garante mapeamento correto entre
    // snake_case do banco e PascalCase do Model C#
    private const string SelectBase = @"
        SELECT 
            id            AS Id,
            nome          AS Nome,
            login         AS Login,
            senha_hash    AS SenhaHash,
            perfil        AS Perfil,
            ativo         AS Ativo,
            foto_path     AS FotoPath,
            criado_em     AS CriadoEm,
            atualizado_em AS AtualizadoEm
        FROM usuarios";

    public async Task<int> InserirAsync(Usuario usuario)
    {
        const string sql = @"
            INSERT INTO usuarios (nome, login, senha_hash, perfil, ativo, foto_path)
            VALUES (@Nome, @Login, @SenhaHash, @Perfil, @Ativo, @FotoPath);
            SELECT LAST_INSERT_ID();";

        using var conn = await _factory.CriarAsync();
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            usuario.Nome,
            usuario.Login,
            usuario.SenhaHash,
            Perfil = usuario.Perfil.ToString().ToLower(),
            usuario.Ativo,
            usuario.FotoPath
        });
    }

    public async Task<bool> AtualizarAsync(Usuario usuario)
    {
        const string sql = @"
            UPDATE usuarios
               SET nome      = @Nome,
                   login     = @Login,
                   perfil    = @Perfil,
                   ativo     = @Ativo,
                   foto_path = @FotoPath
             WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        var linhas = await conn.ExecuteAsync(sql, new
        {
            usuario.Nome,
            usuario.Login,
            Perfil = usuario.Perfil.ToString().ToLower(),
            usuario.Ativo,
            usuario.FotoPath,
            usuario.Id
        });
        return linhas > 0;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        const string sql = "DELETE FROM usuarios WHERE id = @Id";
        using var conn = await _factory.CriarAsync();
        var linhas = await conn.ExecuteAsync(sql, new { Id = id });
        return linhas > 0;
    }

    public async Task<Usuario?> BuscarPorIdAsync(int id)
    {
        var sql = $"{SelectBase} WHERE id = @Id";
        using var conn = await _factory.CriarAsync();
        return await conn.QueryFirstOrDefaultAsync<Usuario>(sql, new { Id = id });
    }

    public async Task<List<Usuario>> ListarTodosAsync()
    {
        var sql = $"{SelectBase} ORDER BY nome";
        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<Usuario>(sql);
        return resultado.ToList();
    }

    public async Task<Usuario?> BuscarPorLoginAsync(string login)
    {
        var sql = $"{SelectBase} WHERE login = @Login AND ativo = 1";
        using var conn = await _factory.CriarAsync();
        return await conn.QueryFirstOrDefaultAsync<Usuario>(sql, new { Login = login });
    }
}
