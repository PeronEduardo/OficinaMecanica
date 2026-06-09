using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Views.Login;

namespace OficinaMecanica.Views.Configuracoes;

public partial class ConfiguracoesPage : ContentPage
{
    private readonly IConfiguracaoService _configuracaoService;
    private readonly IUsuarioController   _usuarioController;

    public ConfiguracoesPage(
        IConfiguracaoService configuracaoService,
        IUsuarioController usuarioController)
    {
        InitializeComponent();
        _configuracaoService = configuracaoService;
        _usuarioController   = usuarioController;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CarregarConfiguracoesAsync();
    }

    private async Task CarregarConfiguracoesAsync()
    {
        try
        {
            var tema = await _configuracaoService.ObterTemaAsync();
            SwitchTema.IsToggled = tema == "escuro";
        }
        catch { }

        if (SessaoUsuario.UsuarioAtual is not null)
        {
            LabelUsuario.Text = SessaoUsuario.UsuarioAtual.Nome;
            LabelPerfil.Text  = $"Perfil: {SessaoUsuario.UsuarioAtual.Perfil}";
        }

        try
        {
            var ultimaBusca = await _configuracaoService.ObterAsync(
                Models.Configuracao.ChaveUltimaBusca);
            LabelUltimaBusca.Text = string.IsNullOrEmpty(ultimaBusca)
                ? "Nenhuma pesquisa salva."
                : ultimaBusca;
        }
        catch { }
    }

    private async void OnTemaToggled(object sender, ToggledEventArgs e)
    {
        try
        {
            var tema = e.Value ? "escuro" : "claro";
            await _configuracaoService.SalvarTemaAsync(tema);
            Application.Current!.UserAppTheme = e.Value ? AppTheme.Dark : AppTheme.Light;
        }
        catch { }
    }

    private async void OnLimparHistoricoClicked(object sender, EventArgs e)
    {
        try
        {
            await _configuracaoService.SalvarAsync(
                Models.Configuracao.ChaveUltimaBusca, "");
        }
        catch { }

        LabelUltimaBusca.Text = "Nenhuma pesquisa salva.";
        await DisplayAlertAsync("Feito", "Histórico de pesquisas limpo.", "OK");
    }

    private async void OnSairClicked(object sender, EventArgs e)
    {
        bool confirmar = await DisplayAlertAsync(
            "Sair", "Deseja sair do sistema?", "Sim", "Cancelar");

        if (!confirmar) return;

        var login = SessaoUsuario.UsuarioAtual?.Login ?? "desconhecido";
        await _usuarioController.LogoutAsync(login);
        SessaoUsuario.Limpar();

        // Volta para o login substituindo a página raiz
        var loginPage = IPlatformApplication.Current!.Services
            .GetRequiredService<LoginPage>();

        if (Application.Current?.Windows[0] is Window window)
            window.Page = new NavigationPage(loginPage);
    }
}
