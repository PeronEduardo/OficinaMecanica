using OficinaMecanica.Views.Clientes;
using OficinaMecanica.Views.Login;
using OficinaMecanica.Views.Ordens;

namespace OficinaMecanica;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
        Routing.RegisterRoute(nameof(ClienteFormPage), typeof(ClienteFormPage));
        Routing.RegisterRoute(nameof(OrdemFormPage), typeof(OrdemFormPage));
    }
}