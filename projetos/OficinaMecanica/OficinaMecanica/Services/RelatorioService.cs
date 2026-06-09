using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;
using IContainer = QuestPDF.Infrastructure.IContainer;

namespace OficinaMecanica.Services;

public class RelatorioService : IRelatorioService
{
    private readonly IOrdemServicoDAO _ordemDAO;
    private readonly IClienteDAO _clienteDAO;
    private readonly IPecaDAO _pecaDAO;

    private const string CorPrimaria = "#1a3a5c";
    private const string CorSecundaria = "#2e86c1";
    private const string CorAlterna = "#f2f3f4";
    private const string CorBranco = "#FFFFFF";
    private const string CorCinzaClaro = "#d5d8dc";
    private const string CorCinzaMedio = "#7f8c8d";
    private const string CorCinzaEscuro = "#566573";
    private const string CorVermelho = "#c0392b";

    public RelatorioService(
        IOrdemServicoDAO ordemDAO,
        IClienteDAO clienteDAO,
        IPecaDAO pecaDAO)
    {
        _ordemDAO = ordemDAO;
        _clienteDAO = clienteDAO;
        _pecaDAO = pecaDAO;

        QuestPDF.Settings.License = LicenseType.Community;
    }

    // ── Relatório de Ordens de Serviço ───────────────────────────────────
    public async Task<byte[]> GerarRelatorioOrdensAsync(DateTime inicio, DateTime fim)
    {
        var ordens = await _ordemDAO.ListarPorPeriodoAsync(inicio, fim);

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ConstruirCabecalho(
                    "Relatório de Ordens de Serviço",
                    $"Período: {inicio:dd/MM/yyyy} a {fim:dd/MM/yyyy}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Table(tabela =>
                    {
                        tabela.ColumnsDefinition(cols =>
                        {
                            cols.ConstantColumn(40);
                            cols.RelativeColumn(2);
                            cols.RelativeColumn(1.5f);
                            cols.ConstantColumn(70);
                            cols.ConstantColumn(80);
                            cols.ConstantColumn(80);
                        });

                        tabela.Header(header =>
                        {
                            CelulaHeader(header, "OS #");
                            CelulaHeader(header, "Cliente");
                            CelulaHeader(header, "Veículo");
                            CelulaHeader(header, "Data");
                            CelulaHeader(header, "Status");
                            CelulaHeader(header, "Total");
                        });

                        for (int i = 0; i < ordens.Count; i++)
                        {
                            var ordem = ordens[i];
                            var cor = i % 2 == 0 ? CorBranco : CorAlterna;

                            // Converte o enum para texto legível em português
                            var status = ordem.Status switch
                            {
                                StatusOrdem.EmAndamento => "Em Andamento",
                                StatusOrdem.AguardandoPeca => "Aguard. Peça",
                                StatusOrdem.Concluida => "Concluída",
                                StatusOrdem.Cancelada => "Cancelada",
                                _ => "Aberta"
                            };

                            // CORRIGIDO: usa ClienteNome e VeiculoMarca/Modelo da view
                            var nomeCliente = ordem.ClienteNome
                                ?? $"ID {ordem.ClienteId}";

                            var nomeVeiculo = !string.IsNullOrEmpty(ordem.VeiculoMarca)
                                ? $"{ordem.VeiculoMarca} {ordem.VeiculoModelo}".Trim()
                                : $"ID {ordem.VeiculoId}";

                            CelulaTexto(tabela, $"#{ordem.Id}", cor);
                            CelulaTexto(tabela, nomeCliente, cor);
                            CelulaTexto(tabela, nomeVeiculo, cor);
                            CelulaTexto(tabela, ordem.DataAbertura.ToString("dd/MM/yyyy"), cor);
                            CelulaTexto(tabela, status, cor);
                            CelulaTexto(tabela, $"R$ {ordem.Total:F2}", cor, negrito: true);
                        }
                    });

                    col.Item().PaddingTop(15).Row(row =>
                    {
                        row.RelativeItem();
                        row.ConstantItem(200)
                           .Border(1).BorderColor(CorSecundaria)
                           .Padding(8).Column(resumo =>
                           {
                               resumo.Item().Text("Resumo")
                                     .Bold().FontSize(11).FontColor(CorPrimaria);

                               resumo.Item().PaddingTop(4).Row(r =>
                               {
                                   r.RelativeItem().Text("Total de ordens:");
                                   r.AutoItem().Text($"{ordens.Count}").Bold();
                               });

                               resumo.Item().Row(r =>
                               {
                                   r.RelativeItem().Text("Valor total:");
                                   r.AutoItem()
                                    .Text($"R$ {ordens.Sum(o => o.Total):F2}")
                                    .Bold().FontColor(CorSecundaria);
                               });

                               resumo.Item().Row(r =>
                               {
                                   r.RelativeItem().Text("Concluídas:");
                                   r.AutoItem()
                                    .Text($"{ordens.Count(o => o.Status == StatusOrdem.Concluida)}")
                                    .Bold();
                               });

                               resumo.Item().Row(r =>
                               {
                                   r.RelativeItem().Text("Em andamento:");
                                   r.AutoItem()
                                    .Text($"{ordens.Count(o => o.Status == StatusOrdem.EmAndamento)}")
                                    .Bold();
                               });
                           });
                    });
                });

                page.Footer().Element(ConstruirRodape());
            });
        });

        return documento.GeneratePdf();
    }

    // ── Relatório de Clientes ────────────────────────────────────────────
    public async Task<byte[]> GerarRelatorioClientesAsync()
    {
        var clientes = await _clienteDAO.ListarTodosAsync();

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ConstruirCabecalho(
                    "Relatório de Clientes Cadastrados",
                    $"Total: {clientes.Count} cliente(s)"));

                page.Content().PaddingVertical(10).Table(tabela =>
                {
                    tabela.ColumnsDefinition(cols =>
                    {
                        cols.ConstantColumn(30);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(1.5f);
                        cols.RelativeColumn(2);
                        cols.RelativeColumn(1);
                    });

                    tabela.Header(header =>
                    {
                        CelulaHeader(header, "ID");
                        CelulaHeader(header, "Nome");
                        CelulaHeader(header, "CPF/CNPJ");
                        CelulaHeader(header, "Telefone");
                        CelulaHeader(header, "E-mail");
                        CelulaHeader(header, "Cidade/UF");
                    });

                    for (int i = 0; i < clientes.Count; i++)
                    {
                        var c = clientes[i];
                        var cor = i % 2 == 0 ? CorBranco : CorAlterna;

                        CelulaTexto(tabela, c.Id.ToString(), cor);
                        CelulaTexto(tabela, c.Nome, cor);
                        CelulaTexto(tabela, c.CpfCnpj, cor);
                        CelulaTexto(tabela, c.Telefone ?? "-", cor);
                        CelulaTexto(tabela, c.Email ?? "-", cor);
                        CelulaTexto(tabela,
                            string.IsNullOrEmpty(c.Cidade)
                                ? "-"
                                : $"{c.Cidade}/{c.Estado}", cor);
                    }
                });

                page.Footer().Element(ConstruirRodape());
            });
        });

        return documento.GeneratePdf();
    }

    // ── Relatório de Estoque Baixo ───────────────────────────────────────
    public async Task<byte[]> GerarRelatorioEstoqueAsync()
    {
        var pecas = await _pecaDAO.ListarEstoqueBaixoAsync();

        var documento = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ConstruirCabecalho(
                    "Relatório de Estoque Baixo",
                    $"Peças abaixo do mínimo — {DateTime.Now:dd/MM/yyyy}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    if (!pecas.Any())
                    {
                        col.Item().PaddingTop(20).AlignCenter()
                           .Text("Nenhuma peça com estoque abaixo do mínimo.")
                           .FontSize(12).Italic();
                        return;
                    }

                    col.Item().Table(tabela =>
                    {
                        tabela.ColumnsDefinition(cols =>
                        {
                            cols.RelativeColumn(2.5f);
                            cols.RelativeColumn(1);
                            cols.ConstantColumn(70);
                            cols.ConstantColumn(70);
                            cols.ConstantColumn(70);
                            cols.ConstantColumn(80);
                        });

                        tabela.Header(header =>
                        {
                            CelulaHeader(header, "Descrição");
                            CelulaHeader(header, "Código");
                            CelulaHeader(header, "Estoque");
                            CelulaHeader(header, "Mínimo");
                            CelulaHeader(header, "Repor");
                            CelulaHeader(header, "Preço unit.");
                        });

                        for (int i = 0; i < pecas.Count; i++)
                        {
                            var p = pecas[i];
                            var cor = i % 2 == 0 ? CorBranco : CorAlterna;
                            var qtdRepor = p.EstoqueMinimo - p.Estoque;

                            CelulaTexto(tabela, p.Descricao, cor);
                            CelulaTexto(tabela, p.Codigo ?? "-", cor);
                            CelulaTexto(tabela, p.Estoque.ToString(), cor, corTexto: CorVermelho);
                            CelulaTexto(tabela, p.EstoqueMinimo.ToString(), cor);
                            CelulaTexto(tabela, qtdRepor.ToString(), cor, negrito: true);
                            CelulaTexto(tabela, $"R$ {p.Preco:F2}", cor);
                        }
                    });
                });

                page.Footer().Element(ConstruirRodape());
            });
        });

        return documento.GeneratePdf();
    }

    // ════════════════════════════════════════════════════════════════
    // Métodos auxiliares
    // ════════════════════════════════════════════════════════════════

    private static Action<IContainer> ConstruirCabecalho(string titulo, string subtitulo)
        => container =>
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Text("Oficina Mecânica — Sistema de Gestão")
                         .FontSize(8).FontColor(CorCinzaMedio);
                        c.Item().Text(titulo)
                         .Bold().FontSize(16).FontColor(CorPrimaria);
                        c.Item().Text(subtitulo)
                         .FontSize(10).FontColor(CorCinzaEscuro);
                    });

                    row.ConstantItem(120).AlignRight().Column(c =>
                    {
                        c.Item().Text("Gerado em:").FontSize(8).FontColor(CorCinzaMedio);
                        c.Item().Text(DateTime.Now.ToString("dd/MM/yyyy HH:mm"))
                         .FontSize(9).Bold();
                    });
                });

                col.Item().PaddingTop(6)
                   .LineHorizontal(2).LineColor(CorSecundaria);
            });
        };

    private static Action<IContainer> ConstruirRodape()
        => container =>
        {
            container.Column(col =>
            {
                col.Item().PaddingTop(5)
                   .LineHorizontal(0.5f).LineColor(CorCinzaClaro);

                col.Item().Row(row =>
                {
                    row.RelativeItem()
                       .Text("Oficina Mecânica — Documento gerado automaticamente.")
                       .FontSize(8).FontColor(CorCinzaMedio);

                    row.ConstantItem(80).AlignRight()
                       .Text(text =>
                       {
                           text.Span("Página ").FontSize(8).FontColor(CorCinzaMedio);
                           text.CurrentPageNumber().FontSize(8);
                           text.Span(" de ").FontSize(8).FontColor(CorCinzaMedio);
                           text.TotalPages().FontSize(8);
                       });
                });
            });
        };

    private static void CelulaHeader(TableCellDescriptor header, string texto)
    {
        header.Cell()
              .Background(CorSecundaria)
              .Padding(5)
              .Text(texto)
              .Bold()
              .FontColor(CorBranco)
              .FontSize(9);
    }

    private static void CelulaTexto(
        TableDescriptor tabela,
        string texto,
        string corFundo,
        bool negrito = false,
        string? corTexto = null)
    {
        var cell = tabela.Cell()
                         .Background(corFundo)
                         .BorderBottom(0.5f)
                         .BorderColor(CorCinzaClaro)
                         .Padding(4);

        var textEl = cell.Text(texto).FontSize(9);
        if (negrito) textEl.Bold();
        if (corTexto is not null) textEl.FontColor(corTexto);
    }
}
