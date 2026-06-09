using MongoDB.Driver;

namespace OficinaMecanica.Infrastructure;

// Fábrica de conexão com o MongoDB.
//
// O MongoDB funciona diferente do MySQL:
//   - Não há "abrir e fechar conexão" a cada operação.
//   - O MongoClient é criado UMA vez e reutilizado durante
//     toda a vida do aplicativo (é thread-safe por design).
//   - Por isso usamos o padrão Singleton aqui:
//     uma única instância do cliente para todo o app.
//
// Estrutura do MongoDB neste projeto:
//   Servidor  → mongodb://localhost:27017
//   Database  → oficina_logs
//   Coleção   → logs  (equivale a uma "tabela" no MySQL)

public class MongoDbConnectionFactory
{
    // Cliente único — criado uma vez, reutilizado sempre.
    private readonly MongoClient _client;

    // Nome do banco de dados no MongoDB.
    private const string NomeBanco = "oficina_logs";

    public MongoDbConnectionFactory()
    {
        // Cria o cliente apontando para o servidor local.
        // Se o MongoDB estiver em outro servidor, troque a URL.
        _client = new MongoClient("mongodb://localhost:27017");
    }

    // Retorna o banco de dados.
    // Se o banco não existir, o MongoDB cria automaticamente
    // na primeira inserção — não precisa criar manualmente.
    public IMongoDatabase ObterDatabase()
    {
        return _client.GetDatabase(NomeBanco);
    }

    // Retorna uma coleção tipada pelo nome.
    // T é o tipo do documento — no nosso caso, LogSistema.
    // Uso: ObterColecao<LogSistema>("logs")
    public IMongoCollection<T> ObterColecao<T>(string nomeColecao)
    {
        return ObterDatabase().GetCollection<T>(nomeColecao);
    }
}
