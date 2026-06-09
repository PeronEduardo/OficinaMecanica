using OficinaMecanica.Models;

namespace OficinaMecanica.Interfaces.Services;

// ---------------------------------------------------------------
// Interface base dos Services
// O Service adiciona validação ao CRUD básico do DAO.
// A diferença principal: Salvar() valida antes de chamar o DAO.
// ---------------------------------------------------------------
public interface IBaseService<T>
{
    // Valida e insere. Lança exceção se os dados forem inválidos.
    Task<int> SalvarAsync(T entidade);

    // Valida e atualiza.
    Task<bool> AtualizarAsync(T entidade);

    // Regras de negócio antes de excluir (ex: verificar dependências).
    Task<bool> ExcluirAsync(int id);

    // Busca por ID.
    Task<T?> BuscarPorIdAsync(int id);

    // Lista todos.
    Task<List<T>> ListarTodosAsync();
}

// ---------------------------------------------------------------
// Usuário
// ---------------------------------------------------------------
public interface IUsuarioService : IBaseService<Usuario>
{
    // Autentica login + senha. Retorna o usuário ou null.
    Task<Usuario?> AutenticarAsync(string login, string senha);

    // Altera a senha — aplica o hash antes de salvar.
    Task<bool> AlterarSenhaAsync(int usuarioId, string novaSenha);
}

// ---------------------------------------------------------------
// Cliente
// ---------------------------------------------------------------
public interface IClienteService : IBaseService<Cliente>
{
    Task<List<Cliente>> BuscarPorNomeAsync(string nome);
    Task<Cliente?> BuscarPorCpfCnpjAsync(string cpfCnpj);
}

// ---------------------------------------------------------------
// Veículo
// ---------------------------------------------------------------
public interface IVeiculoService : IBaseService<Veiculo>
{
    Task<List<Veiculo>> ListarPorClienteAsync(int clienteId);
    Task<Veiculo?> BuscarPorPlacaAsync(string placa);
}

// ---------------------------------------------------------------
// Serviço
// ---------------------------------------------------------------
public interface IServicoService : IBaseService<Servico>
{
    Task<List<Servico>> BuscarPorDescricaoAsync(string descricao);
    Task<List<Servico>> ListarAtivosAsync();
}

// ---------------------------------------------------------------
// Peça
// ---------------------------------------------------------------
public interface IPecaService : IBaseService<Peca>
{
    Task<List<Peca>> BuscarPorDescricaoAsync(string descricao);
    Task<List<Peca>> ListarAtivosAsync();
    Task<List<Peca>> ListarEstoqueBaixoAsync();
}

// ---------------------------------------------------------------
// Ordem de Serviço
// ---------------------------------------------------------------
public interface IOrdemServicoService : IBaseService<OrdemServico>
{
    Task<List<OrdemServico>> ListarPorClienteAsync(int clienteId);
    Task<List<OrdemServico>> ListarPorStatusAsync(StatusOrdem status);
    Task<List<OrdemServico>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<OrdemServico?> BuscarCompletoAsync(int ordemId);

    // Muda o status da OS (ex: Aberta → EmAndamento).
    Task<bool> AlterarStatusAsync(int ordemId, StatusOrdem novoStatus);

    // Recalcula e salva o total da OS.
    Task RecalcularTotalAsync(int ordemId);
}

// ---------------------------------------------------------------
// Log — MongoDB
// ---------------------------------------------------------------
public interface ILogService
{
    Task RegistrarAsync(string usuario, string acao, string descricao, TipoEvento tipo = TipoEvento.Info);
    Task<List<LogSistema>> ListarTodosAsync();
    Task<List<LogSistema>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim);
}

// ---------------------------------------------------------------
// Relatório — gera PDF
// ---------------------------------------------------------------
public interface IRelatorioService
{
    // Gera PDF das ordens dentro de um período.
    Task<byte[]> GerarRelatorioOrdensAsync(DateTime inicio, DateTime fim);

    // Gera PDF de clientes cadastrados.
    Task<byte[]> GerarRelatorioClientesAsync();

    // Gera PDF de peças com estoque baixo.
    Task<byte[]> GerarRelatorioEstoqueAsync();
}

// ---------------------------------------------------------------
// Configuração — SQLite
// ---------------------------------------------------------------
public interface IConfiguracaoService
{
    Task<string?> ObterAsync(string chave);
    Task SalvarAsync(string chave, string valor);

    // Atalhos tipados para as configurações mais usadas.
    Task<string> ObterTemaAsync();
    Task SalvarTemaAsync(string tema);
    Task<string?> ObterUltimoUsuarioAsync();
    Task SalvarUltimoUsuarioAsync(string login);
}
