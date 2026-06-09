using CommunityToolkit.Maui;
using Dapper;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.Logging;
using OficinaMecanica.Controllers;
using OficinaMecanica.DAO.MongoDB;
using OficinaMecanica.DAO.MySQL;
using OficinaMecanica.DAO.SQLite;
using OficinaMecanica.Infrastructure;
using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Services;
using OficinaMecanica.Views.Clientes;
using OficinaMecanica.Views.Configuracoes;
using OficinaMecanica.Views.Graficos;
using OficinaMecanica.Views.Login;
using OficinaMecanica.Views.Ordens;
using OficinaMecanica.Views.Relatorios;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace OficinaMecanica;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // ↓ Adiciona AQUI — antes de tudo
        Dapper.SqlMapper.AddTypeHandler(new OficinaMecanica.Infrastructure.StatusOrdemTypeHandler());
        Dapper.SqlMapper.AddTypeHandler(new OficinaMecanica.Infrastructure.PerfilUsuarioTypeHandler());

        var builder = MauiApp.CreateBuilder();
        // ... resto do código existente
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        LiveChartsCore.LiveCharts.Configure(config =>
            config.AddSkiaSharp()
                  .AddDefaultMappers()
                  .AddLightTheme()
        );

        // Infraestrutura
        builder.Services.AddSingleton<MySqlConnectionFactory>();
        // ... resto dos registros

        // Infraestrutura
        builder.Services.AddSingleton<MySqlConnectionFactory>();
        builder.Services.AddSingleton<MongoDbConnectionFactory>();
        builder.Services.AddSingleton<SqliteConnectionFactory>();

        // DAOs
        builder.Services.AddTransient<IUsuarioDAO, UsuarioDAO>();
        builder.Services.AddTransient<IClienteDAO, ClienteDAO>();
        builder.Services.AddTransient<IVeiculoDAO, VeiculoDAO>();
        builder.Services.AddTransient<IServicoDAO, ServicoDAO>();
        builder.Services.AddTransient<IPecaDAO, PecaDAO>();
        builder.Services.AddTransient<IOrdemServicoDAO, OrdemServicoDAO>();
        builder.Services.AddTransient<ILogDAO, LogDAO>();
        builder.Services.AddTransient<IConfiguracaoDAO, ConfiguracaoDAO>();

        // Services
        builder.Services.AddTransient<ILogService, LogService>();
        builder.Services.AddTransient<IUsuarioService, UsuarioService>();
        builder.Services.AddTransient<IClienteService, ClienteService>();
        builder.Services.AddTransient<IVeiculoService, VeiculoService>();
        builder.Services.AddTransient<IServicoService, ServicoService>();
        builder.Services.AddTransient<IPecaService, PecaService>();
        builder.Services.AddTransient<IOrdemServicoService, OrdemServicoService>();
        builder.Services.AddTransient<IRelatorioService, RelatorioService>();
        builder.Services.AddTransient<IConfiguracaoService, ConfiguracaoService>();
        builder.Services.AddTransient<ExportacaoService>();
        builder.Services.AddTransient<XmlService>();

        // Controllers
        builder.Services.AddTransient<IUsuarioController, UsuarioController>();
        builder.Services.AddTransient<IClienteController, ClienteController>();
        builder.Services.AddTransient<IVeiculoController, VeiculoController>();
        builder.Services.AddTransient<IOrdemServicoController, OrdemServicoController>();
        builder.Services.AddTransient<IRelatorioController, RelatorioController>();

        // Views
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<ClientesListaPage>();
        builder.Services.AddTransient<ClienteFormPage>();
        builder.Services.AddTransient<OrdensListaPage>();
        builder.Services.AddTransient<OrdemFormPage>();
        builder.Services.AddTransient<GraficosPage>();
        builder.Services.AddTransient<RelatoriosPage>();
        builder.Services.AddTransient<ConfiguracoesPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        // Registra o TypeHandler do Dapper para o enum StatusOrdem
        SqlMapper.AddTypeHandler(new StatusOrdemTypeHandler());

        var app = builder.Build();

        // InicializarSqliteAsync(app.Services).GetAwaiter().GetResult();

        return app;
    }

    private static async Task InicializarSqliteAsync(IServiceProvider services)
    {
        try
        {
            var factory = services.GetRequiredService<SqliteConnectionFactory>();
            await factory.InicializarAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro SQLite: {ex.Message}");
        }
    }
}