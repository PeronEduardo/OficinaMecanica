using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Models;

namespace OficinaMecanica.Views.Clientes;

public partial class ClienteFormPage : ContentPage
{
    private readonly IClienteController _controller;
    private Cliente _cliente = new();

    public string? ClienteId
    {
        set
        {
            if (int.TryParse(value, out int id) && id > 0)
                CarregarClienteAsync(id).ConfigureAwait(false);
        }
    }

    public ClienteFormPage(IClienteController controller)
    {
        InitializeComponent();
        _controller = controller;
    }

    private async Task CarregarClienteAsync(int id)
    {
        var resultado = await _controller.BuscarPorIdAsync(id);

        if (!resultado.Sucesso || resultado.Dados is null)
        {
            await DisplayAlertAsync("Erro", "Cliente não encontrado.", "OK");
            await Navigation.PopAsync();
            return;
        }

        _cliente = resultado.Dados;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            Title              = "Editar Cliente";
            EntryNome.Text     = _cliente.Nome;
            EntryCpfCnpj.Text  = _cliente.CpfCnpj;
            EntryTelefone.Text = _cliente.Telefone;
            EntryEmail.Text    = _cliente.Email;
            EntryEndereco.Text = _cliente.Endereco;
            EntryCidade.Text   = _cliente.Cidade;
            EntryEstado.Text   = _cliente.Estado;
        });
    }

    private async void OnSalvarClicked(object sender, EventArgs e)
    {
        LabelErro.IsVisible  = false;
        Carregando.IsVisible = true;
        Carregando.IsRunning = true;
        BtnSalvar.IsEnabled  = false;

        _cliente.Nome     = EntryNome.Text?.Trim() ?? "";
        _cliente.CpfCnpj  = EntryCpfCnpj.Text?.Trim() ?? "";
        _cliente.Telefone = EntryTelefone.Text?.Trim();
        _cliente.Email    = EntryEmail.Text?.Trim();
        _cliente.Endereco = EntryEndereco.Text?.Trim();
        _cliente.Cidade   = EntryCidade.Text?.Trim();
        _cliente.Estado   = EntryEstado.Text?.Trim()?.ToUpper();

        var resultado = await _controller.SalvarAsync(_cliente);

        Carregando.IsRunning = false;
        Carregando.IsVisible = false;
        BtnSalvar.IsEnabled  = true;

        if (!resultado.Sucesso)
        {
            LabelErro.Text      = resultado.Mensagem;
            LabelErro.IsVisible = true;
            return;
        }

        await DisplayAlertAsync("Sucesso", resultado.Mensagem, "OK");
        await Navigation.PopAsync();
    }
}
