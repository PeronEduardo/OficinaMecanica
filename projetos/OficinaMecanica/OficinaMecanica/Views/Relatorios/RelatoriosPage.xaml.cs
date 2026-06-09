using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Services;

namespace OficinaMecanica.Views.Relatorios;

public partial class RelatoriosPage : ContentPage
{
    private readonly IRelatorioController _controller;

    public RelatoriosPage(IRelatorioController controller)
    {
        InitializeComponent();
        _controller = controller;

        // Define período padrão: mês atual
        DateInicio.Date = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        DateFim.Date    = DateTime.Today;
    }

    // ── PDF de Ordens ────────────────────────────────────────────────────
    private async void OnPdfOrdensClicked(object sender, EventArgs e)
    {
        await ExecutarComCarregandoAsync(async () =>
        {
            var resultado = await _controller.GerarPdfOrdensAsync(
                (DateTime)DateInicio.Date,
                (DateTime)DateFim.Date);

            if (!resultado.Sucesso)
            {
                await DisplayAlertAsync("Erro", resultado.Mensagem, "OK");
                return;
            }

            await PdfHelper.SalvarEAbrirAsync(resultado.Dados!,
                $"ordens_{DateInicio.Date:yyyyMM}_{DateFim.Date:yyyyMM}.pdf");
        });
    }

    // ── PDF de Clientes ──────────────────────────────────────────────────
    private async void OnPdfClientesClicked(object sender, EventArgs e)
    {
        await ExecutarComCarregandoAsync(async () =>
        {
            var resultado = await _controller.GerarPdfClientesAsync();

            if (!resultado.Sucesso)
            {
                await DisplayAlertAsync("Erro", resultado.Mensagem, "OK");
                return;
            }

            await PdfHelper.SalvarEAbrirAsync(resultado.Dados!,
                $"clientes_{DateTime.Now:yyyyMMdd}.pdf");
        });
    }

    // ── Exportar JSON ────────────────────────────────────────────────────
    private async void OnExportarJsonClicked(object sender, EventArgs e)
    {
        await ExecutarComCarregandoAsync(async () =>
        {
            // FileSaver abre diálogo "Salvar como" no dispositivo.
            var caminho = Path.Combine(
                FileSystem.CacheDirectory,
                $"oficina_export_{DateTime.Now:yyyyMMddHHmm}.json");

            var resultado = await _controller.ExportarJsonAsync(caminho);

            await DisplayAlertAsync(
                resultado.Sucesso ? "Exportação concluída" : "Erro",
                resultado.Sucesso
                    ? $"Arquivo salvo em:\n{caminho}"
                    : resultado.Mensagem,
                "OK");
        });
    }

    // ── Importar JSON ────────────────────────────────────────────────────
    private async void OnImportarJsonClicked(object sender, EventArgs e)
    {
        await ExecutarComCarregandoAsync(async () =>
        {
            // FilePicker abre o seletor de arquivos do dispositivo.
            var opcoes = new PickOptions
            {
                PickerTitle         = "Selecione o arquivo JSON",
                FileTypes           = new FilePickerFileType(
                    new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.iOS,     new[] { "public.json" } },
                        { DevicePlatform.Android, new[] { "application/json" } },
                        { DevicePlatform.WinUI,   new[] { ".json" } },
                        { DevicePlatform.macOS,   new[] { "json" } }
                    })
            };

            var arquivo = await FilePicker.Default.PickAsync(opcoes);
            if (arquivo is null) return;

            var resultado = await _controller.ImportarJsonAsync(arquivo.FullPath);

            await DisplayAlertAsync(
                resultado.Sucesso ? "Importação concluída" : "Importação com erros",
                resultado.Mensagem,
                "OK");
        });
    }

    // ── Exportar Logs XML ────────────────────────────────────────────────
    private async void OnExportarXmlClicked(object sender, EventArgs e)
    {
        await ExecutarComCarregandoAsync(async () =>
        {
            var caminho = Path.Combine(
                FileSystem.CacheDirectory,
                $"logs_{DateTime.Now:yyyyMMddHHmm}.xml");

            var resultado = await _controller.ExportarLogsXmlAsync(caminho);

            await DisplayAlertAsync(
                resultado.Sucesso ? "Exportação concluída" : "Erro",
                resultado.Sucesso
                    ? $"Logs exportados para:\n{caminho}"
                    : resultado.Mensagem,
                "OK");
        });
    }

    // Método auxiliar: exibe o indicador de carregamento durante a operação.
    private async Task ExecutarComCarregandoAsync(Func<Task> operacao)
    {
        Carregando.IsVisible = true;
        Carregando.IsRunning = true;

        try
        {
            await operacao();
        }
        finally
        {
            Carregando.IsRunning = false;
            Carregando.IsVisible = false;
        }
    }
}
