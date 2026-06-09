using CommunityToolkit.Maui.Storage;

namespace OficinaMecanica.Services;

// PdfHelper é um utilitário estático para salvar e abrir
// arquivos PDF no dispositivo — funciona em Android, iOS e Windows.
//
// A View chama esse helper depois de receber os bytes do RelatorioService.
// Exemplo de uso na View:
//
//   var pdf = await _relatorioController.GerarPdfOrdensAsync(inicio, fim);
//   if (pdf.Sucesso)
//       await PdfHelper.SalvarEAbrirAsync(pdf.Dados!, "relatorio_ordens.pdf");

public static class PdfHelper
{
    // Salva os bytes do PDF em um arquivo temporário e abre
    // com o visualizador padrão do sistema operacional.
    public static async Task SalvarEAbrirAsync(byte[] pdfBytes, string nomeArquivo)
    {
        // CacheDirectory = pasta temporária do app — não aparece
        // no gerenciador de arquivos do usuário, mas é suficiente
        // para abrir e visualizar o PDF.
        var caminho = Path.Combine(
            FileSystem.CacheDirectory,
            nomeArquivo
        );

        // Grava o arquivo no disco.
        await File.WriteAllBytesAsync(caminho, pdfBytes);

        // Launcher.OpenAsync abre o arquivo com o app padrão
        // do dispositivo — leitor de PDF no celular, Acrobat no Windows.
        await Launcher.OpenAsync(new OpenFileRequest
        {
            File = new ReadOnlyFile(caminho)
        });
    }

    // Salva o PDF em uma pasta escolhida pelo usuário
    // e retorna o caminho do arquivo salvo.
    // Útil quando o usuário quer guardar o relatório permanentemente.
    public static async Task<string> SalvarComDialogoAsync(
        byte[] pdfBytes,
        string nomeArquivoSugerido)
    {
        // FileSaver é do CommunityToolkit.Maui.
        // Abre o diálogo "Salvar como" do sistema operacional.
        var resultado = await FileSaver.Default.SaveAsync(
            nomeArquivoSugerido,
            new MemoryStream(pdfBytes)
        );

        if (resultado.IsSuccessful)
            return resultado.FilePath;

        throw new InvalidOperationException("Operação de salvar cancelada pelo usuário.");
    }
}
