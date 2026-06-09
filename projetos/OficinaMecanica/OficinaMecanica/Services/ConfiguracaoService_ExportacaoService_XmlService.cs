using System.Text.Json;
using System.Xml.Linq;
using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;

namespace OficinaMecanica.Services;

// ════════════════════════════════════════════════════════════════
// ConfiguracaoService — lê e salva preferências no SQLite
// ════════════════════════════════════════════════════════════════
public class ConfiguracaoService : IConfiguracaoService
{
    private readonly IConfiguracaoDAO _dao;

    public ConfiguracaoService(IConfiguracaoDAO dao)
    {
        _dao = dao;
    }

    public async Task<string?> ObterAsync(string chave)
        => await _dao.ObterAsync(chave);

    public async Task SalvarAsync(string chave, string valor)
        => await _dao.SalvarAsync(chave, valor);

    // Atalhos tipados para as chaves mais usadas no app.
    // Evita espalhar strings mágicas pelo código.
    public async Task<string> ObterTemaAsync()
        => await _dao.ObterAsync(Configuracao.ChaveTema) ?? "claro";

    public async Task SalvarTemaAsync(string tema)
        => await _dao.SalvarAsync(Configuracao.ChaveTema, tema);

    public async Task<string?> ObterUltimoUsuarioAsync()
        => await _dao.ObterAsync(Configuracao.ChaveUltimoUsuario);

    public async Task SalvarUltimoUsuarioAsync(string login)
        => await _dao.SalvarAsync(Configuracao.ChaveUltimoUsuario, login);
}

// ════════════════════════════════════════════════════════════════
// ExportacaoService — importa e exporta dados em JSON
// Requisito 4 do trabalho
// ════════════════════════════════════════════════════════════════
public class ExportacaoService
{
    private readonly IClienteDAO  _clienteDAO;
    private readonly IVeiculoDAO  _veiculoDAO;
    private readonly IPecaDAO     _pecaDAO;
    private readonly IServicoDAO  _servicoDAO;
    private readonly ILogService  _logService;

    // Opções do serializador JSON:
    // WriteIndented = true → JSON formatado com indentação (legível)
    // PropertyNamingPolicy → nomes em camelCase no arquivo
    private static readonly JsonSerializerOptions _opcoes = new()
    {
        WriteIndented       = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExportacaoService(
        IClienteDAO clienteDAO, IVeiculoDAO veiculoDAO,
        IPecaDAO pecaDAO, IServicoDAO servicoDAO,
        ILogService logService)
    {
        _clienteDAO = clienteDAO;
        _veiculoDAO = veiculoDAO;
        _pecaDAO    = pecaDAO;
        _servicoDAO = servicoDAO;
        _logService = logService;
    }

    // Exporta todos os dados para um único arquivo JSON.
    // O arquivo terá a estrutura:
    // { "clientes": [...], "veiculos": [...], "servicos": [...], "pecas": [...] }
    public async Task ExportarAsync(string caminhoArquivo)
    {
        var dados = new
        {
            exportadoEm = DateTime.Now,
            clientes    = await _clienteDAO.ListarTodosAsync(),
            veiculos    = await _veiculoDAO.ListarTodosAsync(),
            servicos    = await _servicoDAO.ListarTodosAsync(),
            pecas       = await _pecaDAO.ListarTodosAsync()
        };

        var json = JsonSerializer.Serialize(dados, _opcoes);
        await File.WriteAllTextAsync(caminhoArquivo, json);

        await _logService.RegistrarAsync(
            "sistema", "EXPORTACAO_JSON",
            $"Dados exportados para '{caminhoArquivo}'.");
    }

    // Importa clientes e peças de um arquivo JSON.
    // Valida a estrutura antes de tentar salvar.
    public async Task<ResultadoImportacao> ImportarAsync(string caminhoArquivo)
    {
        var resultado = new ResultadoImportacao();

        if (!File.Exists(caminhoArquivo))
            throw new FileNotFoundException("Arquivo não encontrado.", caminhoArquivo);

        var json = await File.ReadAllTextAsync(caminhoArquivo);

        JsonDocument doc;
        try
        {
            doc = JsonDocument.Parse(json);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("O arquivo não é um JSON válido.");
        }

        // Importa clientes se existir a chave "clientes" no JSON.
        if (doc.RootElement.TryGetProperty("clientes", out var clientesEl))
        {
            var clientes = JsonSerializer
                .Deserialize<List<Cliente>>(clientesEl.GetRawText(), _opcoes) ?? new();

            foreach (var cliente in clientes)
            {
                try
                {
                    // Zera o ID para forçar INSERT (não UPDATE).
                    cliente.Id = 0;
                    await _clienteDAO.InserirAsync(cliente);
                    resultado.ClientesImportados++;
                }
                catch
                {
                    resultado.Erros.Add($"Erro ao importar cliente '{cliente.Nome}'.");
                }
            }
        }

        // Importa peças se existir a chave "pecas" no JSON.
        if (doc.RootElement.TryGetProperty("pecas", out var pecasEl))
        {
            var pecas = JsonSerializer
                .Deserialize<List<Peca>>(pecasEl.GetRawText(), _opcoes) ?? new();

            foreach (var peca in pecas)
            {
                try
                {
                    peca.Id = 0;
                    await _pecaDAO.InserirAsync(peca);
                    resultado.PecasImportadas++;
                }
                catch
                {
                    resultado.Erros.Add($"Erro ao importar peça '{peca.Descricao}'.");
                }
            }
        }

        await _logService.RegistrarAsync(
            "sistema", "IMPORTACAO_JSON",
            $"Importação concluída. Clientes: {resultado.ClientesImportados}, " +
            $"Peças: {resultado.PecasImportadas}, Erros: {resultado.Erros.Count}.");

        return resultado;
    }
}

// Classe auxiliar que retorna o resumo da importação.
public class ResultadoImportacao
{
    public int          ClientesImportados { get; set; }
    public int          PecasImportadas    { get; set; }
    public List<string> Erros              { get; set; } = new();
    public bool         Sucesso            => !Erros.Any();
}

// ════════════════════════════════════════════════════════════════
// XmlService — exporta logs do MongoDB para XML
// Requisito 6 do trabalho
// ════════════════════════════════════════════════════════════════
public class XmlService
{
    private readonly ILogService _logService;

    public XmlService(ILogService logService)
    {
        _logService = logService;
    }

    // Exporta todos os logs para um arquivo XML estruturado.
    // Usa System.Xml.Linq (já incluso no .NET) — sem pacote extra.
    //
    // Estrutura do XML gerado:
    // <logs exportadoEm="...">
    //   <log>
    //     <usuario>admin</usuario>
    //     <acao>LOGIN</acao>
    //     <descricao>...</descricao>
    //     <tipoEvento>Info</tipoEvento>
    //     <dataHora>2024-01-01T10:00:00</dataHora>
    //   </log>
    //   ...
    // </logs>
    public async Task ExportarAsync(string caminhoArquivo)
    {
        var logs = await _logService.ListarTodosAsync();

        // XDocument e XElement são as classes do System.Xml.Linq
        // para criar XML de forma fluente (sem string concatenation).
        var xml = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("logs",
                new XAttribute("exportadoEm", DateTime.Now.ToString("o")),
                new XAttribute("total", logs.Count),
                logs.Select(log =>
                    new XElement("log",
                        new XElement("usuario",    log.Usuario),
                        new XElement("acao",       log.Acao),
                        new XElement("descricao",  log.Descricao),
                        new XElement("tipoEvento", log.TipoEvento.ToString()),
                        new XElement("dataHora",   log.DataHora.ToString("o"))
                    )
                )
            )
        );

        xml.Save(caminhoArquivo);

        await _logService.RegistrarAsync(
            "sistema", "EXPORTACAO_XML",
            $"Logs exportados para XML em '{caminhoArquivo}'. Total: {logs.Count} registros.");
    }

    // Exporta apenas os logs de um período específico.
    public async Task ExportarPorPeriodoAsync(
        string caminhoArquivo,
        DateTime inicio,
        DateTime fim)
    {
        var logs = await _logService.ListarPorPeriodoAsync(inicio, fim);

        var xml = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            new XElement("logs",
                new XAttribute("exportadoEm", DateTime.Now.ToString("o")),
                new XAttribute("periodo",     $"{inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}"),
                new XAttribute("total",       logs.Count),
                logs.Select(log =>
                    new XElement("log",
                        new XElement("usuario",    log.Usuario),
                        new XElement("acao",       log.Acao),
                        new XElement("descricao",  log.Descricao),
                        new XElement("tipoEvento", log.TipoEvento.ToString()),
                        new XElement("dataHora",   log.DataHora.ToString("o"))
                    )
                )
            )
        );

        xml.Save(caminhoArquivo);
    }
}
