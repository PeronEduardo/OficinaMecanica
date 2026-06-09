using MySqlConnector;
using OficinaMecanica.Models;

namespace OficinaMecanica.Infrastructure;

public class MySqlConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory()
    {
        _connectionString =
            "Server=localhost;" +
            "Database=oficina_mecanica;" +
            "User=root;" +
            "Password=Ana@1234;" +
            "Port=3306;" +
            "CharSet=utf8mb4;" +
            "Connection Timeout=5;";
    }

    public async Task<MySqlConnection> CriarAsync()
    {
        var conexao = new MySqlConnection(_connectionString);
        await conexao.OpenAsync();
        return conexao;
    }

    public string ObterConnectionString() => _connectionString;
}

// Converte string do banco (ex: "aberta") para enum StatusOrdem
public class StatusOrdemTypeHandler : Dapper.SqlMapper.TypeHandler<StatusOrdem>
{
    public override void SetValue(System.Data.IDbDataParameter parameter, StatusOrdem value)
        => parameter.Value = value.ToString().ToLower();

    public override StatusOrdem Parse(object value)
    {
        return (value?.ToString() ?? "aberta").ToLower() switch
        {
            "em_andamento" => StatusOrdem.EmAndamento,
            "aguardando_peca" => StatusOrdem.AguardandoPeca,
            "concluida" => StatusOrdem.Concluida,
            "cancelada" => StatusOrdem.Cancelada,
            _ => StatusOrdem.Aberta
        };
    }
}

// Converte string do banco (ex: "admin") para enum PerfilUsuario
public class PerfilUsuarioTypeHandler : Dapper.SqlMapper.TypeHandler<PerfilUsuario>
{
    public override void SetValue(System.Data.IDbDataParameter parameter, PerfilUsuario value)
        => parameter.Value = value.ToString().ToLower();

    public override PerfilUsuario Parse(object value)
    {
        return (value?.ToString() ?? "atendente").ToLower() switch
        {
            "admin" => PerfilUsuario.Admin,
            "mecanico" => PerfilUsuario.Mecanico,
            _ => PerfilUsuario.Atendente
        };
    }
}