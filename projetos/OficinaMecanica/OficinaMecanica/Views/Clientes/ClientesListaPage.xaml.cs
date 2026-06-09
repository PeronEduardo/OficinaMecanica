using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Models;

namespace OficinaMecanica.Views.Clientes;

public partial class ClientesListaPage : ContentPage
{
    private readonly IClienteController _controller;
    private List<Cliente> _todosClientes = new();

    public ClientesListaPage(IClienteController controller)
    {
        InitializeComponent();
        _controller = controller;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarClientesAsync();
    }

    private async Task CarregarClientesAsync()
    {
        var resultado = await _controller.ListarAsync();

        if (resultado.Sucesso)
        {
            _todosClientes = resultado.Dados ?? new();
            ListaClientes.ItemsSource = _todosClientes;
        }
        else
        {
            await DisplayAlertAsync("Erro", resultado.Mensagem, "OK");
        }

        RefreshClientes.IsRefreshing = false;
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var termo = e.NewTextValue?.Trim() ?? "";

        if (string.IsNullOrEmpty(termo))
        {
            ListaClientes.ItemsSource = _todosClientes;
            return;
        }

        if (termo.Length >= 2)
        {
            var resultado = await _controller.BuscarAsync(termo);
            if (resultado.Sucesso)
                ListaClientes.ItemsSource = resultado.Dados;
        }
    }

    private async void OnNovoClicked(object sender, EventArgs e)
    {
        var page = IPlatformApplication.Current!.Services
            .GetRequiredService<ClienteFormPage>();
        await Navigation.PushAsync(page);
    }

    private async void OnClienteTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not Cliente cliente) return;
        var page = IPlatformApplication.Current!.Services
            .GetRequiredService<ClienteFormPage>();
        page.ClienteId = cliente.Id.ToString();
        await Navigation.PushAsync(page);
    }

    private async void OnEditarSwipe(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipe) return;
        if (swipe.BindingContext is not Cliente cliente) return;
        var page = IPlatformApplication.Current!.Services
            .GetRequiredService<ClienteFormPage>();
        page.ClienteId = cliente.Id.ToString();
        await Navigation.PushAsync(page);
    }

    private async void OnExcluirSwipe(object sender, EventArgs e)
    {
        if (sender is not SwipeItem swipe) return;
        if (swipe.BindingContext is not Cliente cliente) return;

        bool confirmar = await DisplayAlertAsync(
            "Confirmar exclusão",
            $"Deseja excluir o cliente '{cliente.Nome}'?",
            "Sim, excluir", "Cancelar");

        if (!confirmar) return;

        var resultado = await _controller.ExcluirAsync(cliente.Id);

        await DisplayAlertAsync(
            resultado.Sucesso ? "Sucesso" : "Erro",
            resultado.Mensagem, "OK");

        if (resultado.Sucesso)
            await CarregarClientesAsync();
    }

    private async void OnRefreshing(object sender, EventArgs e)
        => await CarregarClientesAsync();
}
