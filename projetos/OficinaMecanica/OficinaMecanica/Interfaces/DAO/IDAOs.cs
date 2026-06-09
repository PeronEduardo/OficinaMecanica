using OficinaMecanica.Models;

namespace OficinaMecanica.Interfaces.DAO;

// Cada interface abaixo herda IBaseDAO<T> e adiciona apenas
// os métodos que são exclusivos daquela entidade.
// Métodos comuns (Inserir, Atualizar, etc.) já vêm do IBaseDAO.

// ---------------------------------------------------------------
// Usuário
// ---------------------------------------------------------------
public interface IUsuarioDAO : IBaseDAO<Usuario>
{
    // Busca pelo login — usado na tela de autenticação.
    Task<Usuario?> BuscarPorLoginAsync(string login);
}

// ---------------------------------------------------------------
// Cliente
// ---------------------------------------------------------------
public interface IClienteDAO : IBaseDAO<Cliente>
{
    // Busca por nome — usado na pesquisa e filtros da tela.
    Task<List<Cliente>> BuscarPorNomeAsync(string nome);

    // Busca por CPF ou CNPJ — verificação de duplicidade.
    Task<Cliente?> BuscarPorCpfCnpjAsync(string cpfCnpj);
}

// ---------------------------------------------------------------
// Veículo
// ---------------------------------------------------------------
public interface IVeiculoDAO : IBaseDAO<Veiculo>
{
    // Lista todos os veículos de um cliente específico.
    // Usado ao abrir uma OS — mostra os veículos do cliente selecionado.
    Task<List<Veiculo>> ListarPorClienteAsync(int clienteId);

    // Busca pela placa — verificação de duplicidade no cadastro.
    Task<Veiculo?> BuscarPorPlacaAsync(string placa);
}

// ---------------------------------------------------------------
// Serviço
// ---------------------------------------------------------------
public interface IServicoDAO : IBaseDAO<Servico>
{
    // Busca serviços por parte da descrição — usado no campo de busca.
    Task<List<Servico>> BuscarPorDescricaoAsync(string descricao);

    // Lista apenas serviços ativos — para o formulário de OS.
    Task<List<Servico>> ListarAtivosAsync();
}

// ---------------------------------------------------------------
// Peça
// ---------------------------------------------------------------
public interface IPecaDAO : IBaseDAO<Peca>
{
    // Busca por descrição ou código — usado no campo de busca.
    Task<List<Peca>> BuscarPorDescricaoAsync(string descricao);

    // Lista apenas peças ativas — para o formulário de OS.
    Task<List<Peca>> ListarAtivosAsync();

    // Lista peças com estoque abaixo do mínimo — para alertas.
    Task<List<Peca>> ListarEstoqueBaixoAsync();
}

// ---------------------------------------------------------------
// Ordem de Serviço
// ---------------------------------------------------------------
public interface IOrdemServicoDAO : IBaseDAO<OrdemServico>
{
    // Lista ordens de um cliente específico.
    Task<List<OrdemServico>> ListarPorClienteAsync(int clienteId);

    // Lista ordens de um veículo específico.
    Task<List<OrdemServico>> ListarPorVeiculoAsync(int veiculoId);

    // Lista ordens por status — ex: todas as "abertas".
    Task<List<OrdemServico>> ListarPorStatusAsync(StatusOrdem status);

    // Lista ordens dentro de um período — para relatórios.
    Task<List<OrdemServico>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim);

    // Adiciona um serviço à ordem (tabela intermediária).
    Task AdicionarServicoAsync(OrdemServicoServico item);

    // Adiciona uma peça à ordem (tabela intermediária).
    Task AdicionarPecaAsync(OrdemServicoPeca item);

    // Remove todos os serviços de uma ordem — usado ao editar.
    Task RemoverServicosAsync(int ordemId);

    // Remove todas as peças de uma ordem — usado ao editar.
    Task RemoverPecasAsync(int ordemId);

    // Carrega os serviços e peças de uma ordem completa.
    Task<OrdemServico?> BuscarCompletoAsync(int ordemId);
}
