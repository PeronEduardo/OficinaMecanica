using MongoDB.Driver;
using OficinaMecanica.Infrastructure;
using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Models;

namespace OficinaMecanica.DAO.MongoDB;

// LogDAO acessa a coleção "logs" no MongoDB.
// A sintaxe é completamente diferente do SQL:
// em vez de WHERE, usamos Builders<T>.Filter.
// em vez de ORDER BY, usamos Sort.
//
// O MongoDB não usa tabelas — usa coleções de documentos JSON.
// Cada LogSistema vira um documento no MongoDB.

public class LogDAO : ILogDAO
{
    // IMongoCollection é o equivalente a "uma tabela" no MongoDB.
    private readonly IMongoCollection<LogSistema> _colecao;

    public LogDAO(MongoDbConnectionFactory factory)
    {
        // Obtém a coleção "logs" do banco "oficina_logs".
        // Se não existir, o MongoDB cria automaticamente.
        _colecao = factory.ObterColecao<LogSistema>("logs");
    }

    // Insere um novo documento na coleção.
    // InsertOneAsync é o equivalente ao INSERT do SQL.
    public async Task GravarAsync(LogSistema log)
    {
        await _colecao.InsertOneAsync(log);
    }

    // Retorna todos os documentos ordenados por data decrescente
    // (mais recente primeiro).
    public async Task<List<LogSistema>> ListarTodosAsync()
    {
        // FilterDefinition.Empty = sem filtro = SELECT * equivalente.
        var filtro = Builders<LogSistema>.Filter.Empty;

        // Sort.Descending = ORDER BY data_hora DESC.
        var ordenacao = Builders<LogSistema>.Sort.Descending(l => l.DataHora);

        var resultado = await _colecao
            .Find(filtro)
            .Sort(ordenacao)
            .ToListAsync();

        return resultado;
    }

    // Filtra logs de um usuário específico.
    // Eq = "equal" = WHERE usuario = @usuario
    public async Task<List<LogSistema>> ListarPorUsuarioAsync(string usuario)
    {
        var filtro = Builders<LogSistema>.Filter.Eq(l => l.Usuario, usuario);

        var resultado = await _colecao
            .Find(filtro)
            .Sort(Builders<LogSistema>.Sort.Descending(l => l.DataHora))
            .ToListAsync();

        return resultado;
    }

    // Filtra por período.
    // Gte = "greater than or equal" = >= (maior ou igual)
    // Lte = "less than or equal"    = <= (menor ou igual)
    // Combinar dois filtros com Filter.And = WHERE x AND y
    public async Task<List<LogSistema>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        var filtro = Builders<LogSistema>.Filter.And(
            Builders<LogSistema>.Filter.Gte(l => l.DataHora, inicio),
            Builders<LogSistema>.Filter.Lte(l => l.DataHora, fim)
        );

        var resultado = await _colecao
            .Find(filtro)
            .Sort(Builders<LogSistema>.Sort.Descending(l => l.DataHora))
            .ToListAsync();

        return resultado;
    }

    // Filtra por tipo de evento (Info, Aviso, Erro).
    public async Task<List<LogSistema>> ListarPorTipoAsync(TipoEvento tipo)
    {
        var filtro = Builders<LogSistema>.Filter.Eq(l => l.TipoEvento, tipo);

        var resultado = await _colecao
            .Find(filtro)
            .Sort(Builders<LogSistema>.Sort.Descending(l => l.DataHora))
            .ToListAsync();

        return resultado;
    }
}
