using OficinaMecanica.Models;

namespace OficinaMecanica.Interfaces.Controllers;

// ---------------------------------------------------------------
// Interface base dos Controllers
//
// O Controller recebe eventos da View, chama o Service e
// devolve um resultado padronizado (ResultadoOperacao<T>).
//
// Por que ResultadoOperacao em vez de retornar o objeto direto?
// Para que a View sempre saiba se deu certo ou errado, e tenha
// uma mensagem de erro para mostrar ao usuário — sem precisar
// tratar exceções diretamente na tela.
// ---------------------------------------------------------------
public interface IBaseController
{
    // Indica se a última operação foi bem-sucedida.
    bool Sucesso { get; }

    // Mensagem para exibir na View (erro ou confirmação).
    string Mensagem { get; }
}

// Classe auxiliar que encapsula o resultado de qualquer operação.
// Usada como retorno dos métodos dos Controllers.
// T é o tipo do dado retornado (ex: Cliente, List<OrdemServico>).
public class ResultadoOperacao<T>
{
    public bool    Sucesso   { get; set; }
    public string  Mensagem  { get; set; } = string.Empty;
    public T?      Dados     { get; set; }

    // Fábrica estática — cria um resultado de sucesso.
    public static ResultadoOperacao<T> Ok(T dados, string mensagem = "Operação realizada com sucesso.")
        => new() { Sucesso = true, Mensagem = mensagem, Dados = dados };

    // Fábrica estática — cria um resultado de erro.
    public static ResultadoOperacao<T> Falha(string mensagem)
        => new() { Sucesso = false, Mensagem = mensagem };
}

// ---------------------------------------------------------------
// Usuário
// ---------------------------------------------------------------
public interface IUsuarioController : IBaseController
{
    Task<ResultadoOperacao<Usuario?>> LoginAsync(string login, string senha);
    Task<ResultadoOperacao<bool>> LogoutAsync(string login);
    Task<ResultadoOperacao<int>> CadastrarAsync(Usuario usuario, string senha);
}

// ---------------------------------------------------------------
// Cliente
// ---------------------------------------------------------------
public interface IClienteController : IBaseController
{
    Task<ResultadoOperacao<int>>          SalvarAsync(Cliente cliente);
    Task<ResultadoOperacao<bool>>         ExcluirAsync(int id);
    Task<ResultadoOperacao<Cliente?>>     BuscarPorIdAsync(int id);
    Task<ResultadoOperacao<List<Cliente>>> ListarAsync();
    Task<ResultadoOperacao<List<Cliente>>> BuscarAsync(string termo);
}

// ---------------------------------------------------------------
// Veículo
// ---------------------------------------------------------------
public interface IVeiculoController : IBaseController
{
    Task<ResultadoOperacao<int>>          SalvarAsync(Veiculo veiculo);
    Task<ResultadoOperacao<bool>>         ExcluirAsync(int id);
    Task<ResultadoOperacao<Veiculo?>>     BuscarPorIdAsync(int id);
    Task<ResultadoOperacao<List<Veiculo>>> ListarPorClienteAsync(int clienteId);
}

// ---------------------------------------------------------------
// Ordem de Serviço
// ---------------------------------------------------------------
public interface IOrdemServicoController : IBaseController
{
    Task<ResultadoOperacao<int>>               SalvarAsync(OrdemServico ordem);
    Task<ResultadoOperacao<bool>>              ExcluirAsync(int id);
    Task<ResultadoOperacao<OrdemServico?>>     BuscarCompletoAsync(int id);
    Task<ResultadoOperacao<List<OrdemServico>>> ListarAsync();
    Task<ResultadoOperacao<List<OrdemServico>>> ListarPorStatusAsync(StatusOrdem status);
    Task<ResultadoOperacao<bool>>              AlterarStatusAsync(int id, StatusOrdem status);
}

// ---------------------------------------------------------------
// Relatório
// ---------------------------------------------------------------
public interface IRelatorioController : IBaseController
{
    Task<ResultadoOperacao<byte[]>> GerarPdfOrdensAsync(DateTime inicio, DateTime fim);
    Task<ResultadoOperacao<byte[]>> GerarPdfClientesAsync();
    Task<ResultadoOperacao<bool>>   ExportarLogsXmlAsync(string caminhoArquivo);
    Task<ResultadoOperacao<bool>>   ExportarJsonAsync(string caminhoArquivo);
    Task<ResultadoOperacao<bool>>   ImportarJsonAsync(string caminhoArquivo);
}
