using OficinaMecanica.Infrastructure;
using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Models;

namespace OficinaMecanica.DAO.SQLite;

// ConfiguracaoDAO acessa a tabela "configuracoes" no SQLite local.
// Usa sqlite-net-pcl — biblioteca diferente do Dapper.
// Aqui não escrevemos SQL manualmente: a biblioteca gera
// automaticamente a partir dos atributos do Model Configuracao.
//
// Funciona como um dicionário persistente:
//   SalvarAsync("tema", "escuro")
//   ObterAsync("tema") → "escuro"

public class ConfiguracaoDAO : IConfiguracaoDAO
{
    private readonly SqliteConnectionFactory _factory;

    public ConfiguracaoDAO(SqliteConnectionFactory factory)
    {
        _factory = factory;
    }

    // Busca o valor de uma chave.
    // GetAsync<T> faz SELECT WHERE PrimaryKey = valor.
    // Retorna null se a chave não existir.
    public async Task<string?> ObterAsync(string chave)
    {
        var conn = _factory.ObterConexao();

        // GetAsync busca pelo valor da chave primária.
        var config = await conn.GetAsync<Configuracao>(chave);
        return config?.Valor;
    }

    // Salva ou atualiza um valor.
    // InsertOrReplaceAsync = INSERT OR REPLACE no SQLite:
    // se a chave já existe, atualiza; se não, insere.
    public async Task SalvarAsync(string chave, string valor)
    {
        var conn = _factory.ObterConexao();

        await conn.InsertOrReplaceAsync(new Configuracao
        {
            Chave = chave,
            Valor = valor
        });
    }

    // Remove uma chave do banco local.
    // DeleteAsync busca pelo valor da chave primária e deleta.
    public async Task RemoverAsync(string chave)
    {
        var conn = _factory.ObterConexao();

        var config = await conn.GetAsync<Configuracao>(chave);
        if (config is not null)
            await conn.DeleteAsync(config);
    }
}
