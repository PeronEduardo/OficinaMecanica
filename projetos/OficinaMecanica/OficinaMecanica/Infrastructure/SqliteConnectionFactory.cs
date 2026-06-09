using SQLite;
using OficinaMecanica.Models;

namespace OficinaMecanica.Infrastructure;

// Fábrica de conexão com o SQLite local.
//
// O SQLite é diferente do MySQL e do MongoDB:
//   - Não é um servidor — é um arquivo no dispositivo do usuário.
//   - O arquivo é criado automaticamente se não existir.
//   - Ideal para configurações locais que não precisam ir ao servidor.
//
// No .NET MAUI, cada plataforma tem um caminho diferente
// para armazenar arquivos do app:
//   Android → /data/data/com.empresa.app/files/
//   iOS     → /var/mobile/.../Documents/
//   Windows → C:\Users\...\AppData\Local\...
//
// FileSystem.AppDataDirectory resolve isso automaticamente —
// retorna o caminho correto para cada plataforma.

public class SqliteConnectionFactory
{
    // Conexão única reutilizada — o SQLite também é thread-safe
    // quando configurado com SQLiteOpenFlags corretos.
    private SQLiteAsyncConnection? _conexao;

    // Nome do arquivo de banco de dados no dispositivo.
    private const string NomeArquivo = "oficina_config.db";

    // Retorna a conexão, criando-a na primeira chamada (lazy init).
    // "Lazy" significa: só cria quando for necessário pela 1ª vez.
    public SQLiteAsyncConnection ObterConexao()
    {
        if (_conexao is null)
        {
            // Monta o caminho completo do arquivo no dispositivo.
            var caminho = Path.Combine(
                FileSystem.AppDataDirectory,
                NomeArquivo
            );

            // Abre (ou cria) o arquivo com permissões de leitura e escrita.
            _conexao = new SQLiteAsyncConnection(
                caminho,
                SQLiteOpenFlags.ReadWrite |
                SQLiteOpenFlags.Create    |
                SQLiteOpenFlags.SharedCache
            );
        }

        return _conexao;
    }

    // Cria a tabela de configurações no SQLite se ainda não existir.
    // Este método é chamado uma vez na inicialização do app (MauiProgram.cs).
    // "CreateTableAsync" verifica se a tabela existe antes de criar —
    // nunca sobrescreve dados existentes.
    public async Task InicializarAsync()
    {
        var conn = ObterConexao();
        await conn.CreateTableAsync<Configuracao>();
    }
}
