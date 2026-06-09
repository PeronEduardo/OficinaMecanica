using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Models;

namespace OficinaMecanica.Views.Ordens;

public partial class OrdensListaPage : ContentPage
{
    private readonly IOrdemServicoController _controller;
    private string _filtroAtual = "todos";

    public OrdensListaPage(IOrdemServicoController controller)
    {
        InitializeComponent();
        _controller = controller;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarOrdensAsync();
    }

    private async Task CarregarOrdensAsync()
    {
        ResultadoOperacao<List<OrdemServico>> resultado;

        if (_filtroAtual == "todos")
        {
            resultado = await _controller.ListarAsync();
        }
        else
        {
            var status = _filtroAtual switch
            {
                "aberta"       => StatusOrdem.Aberta,
                "em_andamento" => StatusOrdem.EmAndamento,
                "concluida"    => StatusOrdem.Concluida,
                "cancelada"    => StatusOrdem.Cancelada,
                _              => StatusOrdem.Aberta
            };
            resultado = await _controller.ListarPorStatusAsync(status);
        }

        if (resultado.Sucesso)
            ListaOrdens.ItemsSource = resultado.Dados;
        else
            await DisplayAlertAsync("Erro", resultado.Mensagem, "OK");

        RefreshOrdens.IsRefreshing = false;
    }

    private async void OnFiltroClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn) return;
        _filtroAtual = btn.CommandParameter?.ToString() ?? "todos";
        await CarregarOrdensAsync();
    }

    private async void OnNovaOrdemClicked(object sender, EventArgs e)
    {
        var page = IPlatformApplication.Current!.Services
            .GetRequiredService<OrdemFormPage>();
        await Navigation.PushAsync(page);
    }

    private async void OnOrdemTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not OrdemServico ordem) return;
        var page = IPlatformApplication.Current!.Services
            .GetRequiredService<OrdemFormPage>();
        page.OrdemId = ordem.Id.ToString();
        await Navigation.PushAsync(page);
    }

    private async void OnVerSwipe(object sender, EventArgs e)
    {
        if (sender is not SwipeItem s) return;
        if (s.BindingContext is not OrdemServico o) return;
        var page = IPlatformApplication.Current!.Services
            .GetRequiredService<OrdemFormPage>();
        page.OrdemId = o.Id.ToString();
        await Navigation.PushAsync(page);
    }

    private async void OnExcluirSwipe(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipe) return;
        if (swipe.BindingContext is not OrdemServico ordem) return;

        bool confirmar = await DisplayAlertAsync(
            "Confirmar exclusão",
            $"Deseja excluir a OS #{ordem.Id}?",
            "Sim, excluir", "Cancelar");

        if (!confirmar) return;

        var resultado = await _controller.ExcluirAsync(ordem.Id);
        await DisplayAlertAsync(
            resultado.Sucesso ? "Sucesso" : "Erro",
            resultado.Mensagem, "OK");

        if (resultado.Sucesso)
            await CarregarOrdensAsync();
    }

    private async void OnRefreshing(object sender, EventArgs e)
        => await CarregarOrdensAsync();
}
