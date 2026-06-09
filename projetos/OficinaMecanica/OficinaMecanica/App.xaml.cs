using Microsoft.Maui.Controls;
using OficinaMecanica.Views.Clientes;
using OficinaMecanica.Views.Configuracoes;
using OficinaMecanica.Views.Graficos;
using OficinaMecanica.Views.Login;
using OficinaMecanica.Views.Ordens;
using OficinaMecanica.Views.Relatorios;

namespace OficinaMecanica;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }
    protected override Window CreateWindow(IActivationState? activationState)
    {
        var loginPage = IPlatformApplication.Current!.Services
            .GetRequiredService<LoginPage>();
        return new Window(new NavigationPage(loginPage));
    }

    // Navega para a tela principal com TabBar após o login
    public static void IrParaPrincipal()
    {
        var services = IPlatformApplication.Current!.Services;

        var tabPage = new TabbedPage
        {
            Children =
    {
        new NavigationPage(services.GetRequiredService<OrdensListaPage>())
            { Title = "Ordens" },
        new NavigationPage(services.GetRequiredService<ClientesListaPage>())
            { Title = "Clientes" },
         new NavigationPage(services.GetRequiredService<GraficosPage>())
             { Title = "Gráficos" },
        new NavigationPage(services.GetRequiredService<RelatoriosPage>())
            { Title = "Relatórios" },
        new NavigationPage(services.GetRequiredService<ConfiguracoesPage>())
            { Title = "Config" },
    }
        };
        if (Current?.Windows[0] is Window window)
            window.Page = tabPage;
    }
}