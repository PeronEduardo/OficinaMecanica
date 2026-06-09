using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;
using OficinaMecanica.Services;

namespace OficinaMecanica.Controllers;

// RelatorioController centraliza todas as operações de exportação:
//   - Relatórios em PDF (requisito 8)
//   - Exportação de logs em XML (requisito 6)
//   - Importação e exportação de dados em JSON (requisito 4)

public class RelatorioController : IRelatorioController
{
    private readonly IRelatorioService  _relatorioService;
    private readonly XmlService         _xmlService;
    private readonly ExportacaoService  _exportacaoService;
    private readonly ILogService        _logService;

    public bool   Sucesso  { get; private set; }
    public string Mensagem { get; private set; } = string.Empty;

    public RelatorioController(
        IRelatorioService relatorioService,
        XmlService xmlService,
        ExportacaoService exportacaoService,
        ILogService logService)
    {
        _relatorioService  = relatorioService;
        _xmlService        = xmlService;
        _exportacaoService = exportacaoService;
        _logService        = logService;
    }

    // Gera PDF das ordens de serviço de um período.
    // Retorna o arquivo como array de bytes — a View decide onde salvar.
    public async Task<ResultadoOperacao<byte[]>> GerarPdfOrdensAsync(
        DateTime inicio, DateTime fim)
    {
        try
        {
            var pdf = await _relatorioService.GerarRelatorioOrdensAsync(inicio, fim);

            Sucesso  = true;
            Mensagem = "Relatório gerado com sucesso!";
            return ResultadoOperacao<byte[]>.Ok(pdf, Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao gerar relatório PDF.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao gerar PDF de ordens: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<byte[]>.Falha(Mensagem);
        }
    }

    // Gera PDF da lista de clientes.
    public async Task<ResultadoOperacao<byte[]>> GerarPdfClientesAsync()
    {
        try
        {
            var pdf = await _relatorioService.GerarRelatorioClientesAsync();

            Sucesso  = true;
            Mensagem = "Relatório de clientes gerado!";
            return ResultadoOperacao<byte[]>.Ok(pdf, Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao gerar relatório de clientes.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao gerar PDF de clientes: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<byte[]>.Falha(Mensagem);
        }
    }

    // Exporta todos os logs para XML no caminho especificado.
    public async Task<ResultadoOperacao<bool>> ExportarLogsXmlAsync(string caminhoArquivo)
    {
        try
        {
            await _xmlService.ExportarAsync(caminhoArquivo);

            Sucesso  = true;
            Mensagem = $"Logs exportados para XML com sucesso!";
            return ResultadoOperacao<bool>.Ok(true, Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao exportar logs em XML.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao exportar XML: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
    }

    // Exporta todos os dados do banco para um arquivo JSON.
    public async Task<ResultadoOperacao<bool>> ExportarJsonAsync(string caminhoArquivo)
    {
        try
        {
            await _exportacaoService.ExportarAsync(caminhoArquivo);

            Sucesso  = true;
            Mensagem = "Dados exportados em JSON com sucesso!";
            return ResultadoOperacao<bool>.Ok(true, Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao exportar JSON.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao exportar JSON: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
    }

    // Importa dados de um arquivo JSON para o banco MySQL.
    public async Task<ResultadoOperacao<bool>> ImportarJsonAsync(string caminhoArquivo)
    {
        try
        {
            var resultado = await _exportacaoService.ImportarAsync(caminhoArquivo);

            Sucesso = resultado.Sucesso;

            // Monta mensagem com resumo da importação.
            Mensagem = $"Importação concluída. " +
                       $"Clientes: {resultado.ClientesImportados}, " +
                       $"Peças: {resultado.PecasImportadas}.";

            if (resultado.Erros.Any())
                Mensagem += $" {resultado.Erros.Count} erro(s) encontrado(s).";

            return ResultadoOperacao<bool>.Ok(resultado.Sucesso, Mensagem);
        }
        catch (FileNotFoundException ex)
        {
            Sucesso  = false;
            Mensagem = $"Arquivo não encontrado: {ex.FileName}";
            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
        catch (InvalidOperationException ex)
        {
            Sucesso  = false;
            Mensagem = ex.Message;
            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao importar JSON.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao importar JSON: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
    }
}
