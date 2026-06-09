using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;

namespace OficinaMecanica.Views.Graficos;

public partial class GraficosPage : ContentPage
{
    private readonly IOrdemServicoController _ordemController;
    private readonly IClienteController      _clienteController;
    private readonly IPecaService            _pecaService;

    public GraficosPage(
        IOrdemServicoController ordemController,
        IClienteController clienteController,
        IPecaService pecaService)
    {
        InitializeComponent();
        _ordemController   = ordemController;
        _clienteController = clienteController;
        _pecaService       = pecaService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarDadosAsync();
    }

    private async Task CarregarDadosAsync()
    {
        Carregando.IsVisible = true;
        Carregando.IsRunning = true;

        try
        {
            // Carrega ordens
            var resultOrdens = await _ordemController.ListarAsync();
            var ordens = resultOrdens.Dados ?? new();

            // Carrega clientes
            var resultClientes = await _clienteController.ListarAsync();
            var totalClientes = resultClientes.Dados?.Count ?? 0;

            // Carrega peças com estoque baixo
            var pecasBaixas = await _pecaService.ListarEstoqueBaixoAsync();

            // ── Cards de resumo ──────────────────────────────────────────
            var totalOS        = ordens.Count;
            var faturamento    = ordens.Sum(o => o.Total);
            var osAbertas      = ordens.Count(o => o.Status == StatusOrdem.Aberta);
            var osConcluidas   = ordens.Count(o => o.Status == StatusOrdem.Concluida);
            var osAndamento    = ordens.Count(o => o.Status == StatusOrdem.EmAndamento);
            var osCanceladas   = ordens.Count(o => o.Status == StatusOrdem.Cancelada);

            LabelTotalOS.Text       = totalOS.ToString();
            LabelFaturamento.Text   = $"R$ {faturamento:N2}";
            LabelOSAbertas.Text     = osAbertas.ToString();
            LabelOSConcluidas.Text  = osConcluidas.ToString();
            LabelTotalClientes.Text = totalClientes.ToString();
            LabelEstoqueBaixo.Text  = pecasBaixas.Count.ToString();

            // ── Barras de status ─────────────────────────────────────────
            if (totalOS > 0)
            {
                double larguraMax = 300; // largura máxima da barra em pixels

                double pctAbertas    = (double)osAbertas    / totalOS;
                double pctAndamento  = (double)osAndamento  / totalOS;
                double pctConcluidas = (double)osConcluidas / totalOS;
                double pctCanceladas = (double)osCanceladas / totalOS;

                LabelPctAbertas.Text    = $"{pctAbertas:P0}";
                LabelPctAndamento.Text  = $"{pctAndamento:P0}";
                LabelPctConcluidas.Text = $"{pctConcluidas:P0}";
                LabelPctCanceladas.Text = $"{pctCanceladas:P0}";

                BarraAbertas.WidthRequest    = pctAbertas    * larguraMax;
                BarraAndamento.WidthRequest  = pctAndamento  * larguraMax;
                BarraConcluidas.WidthRequest = pctConcluidas * larguraMax;
                BarraCanceladas.WidthRequest = pctCanceladas * larguraMax;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro gráficos: {ex.Message}");
        }
        finally
        {
            Carregando.IsRunning = false;
            Carregando.IsVisible = false;
        }
    }
}
