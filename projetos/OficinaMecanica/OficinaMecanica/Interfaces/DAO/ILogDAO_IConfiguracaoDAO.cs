using OficinaMecanica.Models;

namespace OficinaMecanica.Interfaces.DAO;

// ---------------------------------------------------------------
// Log — MongoDB
// NÃO herda IBaseDAO porque logs nunca são editados ou deletados.
// Log é append-only: só se grava, nunca se altera.
// ---------------------------------------------------------------
public interface ILogDAO
{
    // Grava um novo log no MongoDB.
    Task GravarAsync(LogSistema log);

    // Lista todos os logs — para exportação XML e tela de logs.
    Task<List<LogSistema>> ListarTodosAsync();

    // Lista logs de um usuário específico.
    Task<List<LogSistema>> ListarPorUsuarioAsync(string usuario);

    // Lista logs dentro de um período — para exportação filtrada.
    Task<List<LogSistema>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim);

    // Lista logs por tipo de evento — ex: só os Erros.
    Task<List<LogSistema>> ListarPorTipoAsync(TipoEvento tipo);
}

// ---------------------------------------------------------------
// Configuração — SQLite
// Também não herda IBaseDAO porque funciona como chave-valor:
// não tem "listar todos" com paginação, não tem "buscar por id".
// ---------------------------------------------------------------
public interface IConfiguracaoDAO
{
    // Lê o valor de uma chave. Retorna null se não existir.
    Task<string?> ObterAsync(string chave);

    // Salva ou atualiza o valor de uma chave (INSERT OR REPLACE).
    Task SalvarAsync(string chave, string valor);

    // Remove uma chave do SQLite.
    Task RemoverAsync(string chave);
}
