using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;

namespace OficinaMecanica.Services;

// LogService é o primeiro a ser criado porque TODOS os outros
// Services dependem dele para registrar suas ações.
//
// Ele é simples: recebe uma descrição e grava no MongoDB via LogDAO.
// Nunca lança exceção para o chamador — um erro de log não pode
// derrubar a operação principal.

public class LogService : ILogService
{
    private readonly ILogDAO _logDAO;

    public LogService(ILogDAO logDAO)
    {
        _logDAO = logDAO;
    }

    // Método principal — usado por todos os outros Services.
    // Exemplo de uso:
    //   await _logService.RegistrarAsync("admin", "LOGIN", "Usuário fez login");
    public async Task RegistrarAsync(
        string usuario,
        string acao,
        string descricao,
        TipoEvento tipo = TipoEvento.Info)
    {
        try
        {
            var log = new LogSistema
            {
                Usuario   = usuario,
                Acao      = acao.ToUpper(),
                Descricao = descricao,
                TipoEvento = tipo,
                DataHora  = DateTime.Now
            };

            await _logDAO.GravarAsync(log);
        }
        catch
        {
            // Engole o erro silenciosamente.
            // Falha no log não deve interromper a operação do sistema.
        }
    }

    public async Task<List<LogSistema>> ListarTodosAsync()
    {
        return await _logDAO.ListarTodosAsync();
    }

    public async Task<List<LogSistema>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        return await _logDAO.ListarPorPeriodoAsync(inicio, fim);
    }
}
