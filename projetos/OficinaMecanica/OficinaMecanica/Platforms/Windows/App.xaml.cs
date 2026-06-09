using Microsoft.UI.Xaml;

namespace OficinaMecanica.WinUI;

public partial class App : MauiWinUIApplication
{
    public App()
    {
        this.InitializeComponent();

        // Captura exceções não tratadas do WinUI
        this.UnhandledException += (sender, e) =>
        {
            e.Handled = true;
            var erro = e.Exception?.ToString() ?? "Erro desconhecido";

            // Salva o erro no Desktop
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "oficina_erro.txt");
            File.WriteAllText(path, erro);
        };
    }

    protected override MauiApp CreateMauiApp()
        => MauiProgram.CreateMauiApp();
}