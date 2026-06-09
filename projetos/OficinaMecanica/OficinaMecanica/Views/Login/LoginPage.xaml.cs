using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Interfaces.Services;

namespace OficinaMecanica.Views.Login;

public partial class LoginPage : ContentPage
{
    private readonly IUsuarioController    _usuarioController;
    private readonly IConfiguracaoService  _configuracaoService;

    // O construtor recebe os objetos via injeção de dependência.
    // O MAUI entrega automaticamente quando a página é registrada no DI.
    public LoginPage(
        IUsuarioController usuarioController,
        IConfiguracaoService configuracaoService)
    {
        InitializeComponent();
        _usuarioController   = usuarioController;
        _configuracaoService = configuracaoService;
    }

    // Ao abrir a tela, preenche o login com o último usuário
    // que acessou — carregado do SQLite.
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            if (_configuracaoService is null) return;

            var ultimoUsuario = await _configuracaoService.ObterUltimoUsuarioAsync();
            if (!string.IsNullOrEmpty(ultimoUsuario))
                EntryLogin.Text = ultimoUsuario;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro OnAppearing: {ex.Message}");
        }
    }

    // Evento do botão "Entrar".
    private async void OnEntrarClicked(object sender, EventArgs e)
    {
        LabelErro.IsVisible = false;
        Carregando.IsVisible = true;
        Carregando.IsRunning = true;
        BtnEntrar.IsEnabled = false;

        if (_usuarioController is null)
        {
            LabelErro.Text = "Erro: controller não inicializado.";
            LabelErro.IsVisible = true;
            Carregando.IsRunning = false;
            Carregando.IsVisible = false;
            BtnEntrar.IsEnabled = true;
            return;
        }

        ResultadoOperacao<Models.Usuario?> resultado;
        try
        {
            resultado = await _usuarioController.LoginAsync(
                EntryLogin.Text?.Trim() ?? "",
                EntrySenha.Text ?? "");
        }
        catch (Exception ex)
        {
            LabelErro.Text = $"Erro de conexão: {ex.Message}";
            LabelErro.IsVisible = true;
            Carregando.IsRunning = false;
            Carregando.IsVisible = false;
            BtnEntrar.IsEnabled = true;
            return;
        }

        Carregando.IsRunning = false;
        Carregando.IsVisible = false;
        BtnEntrar.IsEnabled = true;

        if (!resultado.Sucesso)
        {
            LabelErro.Text = resultado.Mensagem;
            LabelErro.IsVisible = true;
            EntrySenha.Text = "";
            EntrySenha.Focus();
            return;
        }

        if (_configuracaoService is not null)
            await _configuracaoService.SalvarUltimoUsuarioAsync(EntryLogin.Text!.Trim());

        SessaoUsuario.UsuarioAtual = resultado.Dados!;
        // Navega para a tela principal após login
        var ordens = IPlatformApplication.Current!.Services
            .GetRequiredService<OficinaMecanica.Views.Ordens.OrdensListaPage>();

        App.IrParaPrincipal();
    }
}

// Classe estática simples para guardar o usuário logado durante a sessão.
// Não persiste — ao fechar o app, é limpa automaticamente.
public static class SessaoUsuario
{
    public static OficinaMecanica.Models.Usuario? UsuarioAtual { get; set; }
    public static bool EstaLogado => UsuarioAtual is not null;

    public static void Limpar() => UsuarioAtual = null;
}
