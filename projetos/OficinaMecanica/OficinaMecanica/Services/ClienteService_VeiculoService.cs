using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;

namespace OficinaMecanica.Services;

// ════════════════════════════════════════════════════════════════
// ClienteService
// Regras de negócio: validação de CPF/CNPJ, duplicidade, campos
// ════════════════════════════════════════════════════════════════
public class ClienteService : IClienteService
{
    private readonly IClienteDAO _clienteDAO;
    private readonly ILogService _logService;

    public ClienteService(IClienteDAO clienteDAO, ILogService logService)
    {
        _clienteDAO = clienteDAO;
        _logService = logService;
    }

    public async Task<int> SalvarAsync(Cliente cliente)
    {
        Validar(cliente);

        // Verifica duplicidade de CPF/CNPJ — não pode ter dois iguais.
        var existente = await _clienteDAO.BuscarPorCpfCnpjAsync(cliente.CpfCnpj);
        if (existente is not null && existente.Id != cliente.Id)
            throw new InvalidOperationException(
                $"Já existe um cliente cadastrado com o CPF/CNPJ '{cliente.CpfCnpj}'.");

        var id = await _clienteDAO.InserirAsync(cliente);

        await _logService.RegistrarAsync(
            "sistema", "CADASTRO",
            $"Cliente '{cliente.Nome}' (CPF/CNPJ: {cliente.CpfCnpj}) cadastrado.");

        return id;
    }

    public async Task<bool> AtualizarAsync(Cliente cliente)
    {
        Validar(cliente);

        var existente = await _clienteDAO.BuscarPorCpfCnpjAsync(cliente.CpfCnpj);
        if (existente is not null && existente.Id != cliente.Id)
            throw new InvalidOperationException(
                $"Já existe outro cliente com o CPF/CNPJ '{cliente.CpfCnpj}'.");

        var resultado = await _clienteDAO.AtualizarAsync(cliente);

        await _logService.RegistrarAsync(
            "sistema", "ALTERACAO",
            $"Dados do cliente '{cliente.Nome}' foram atualizados.");

        return resultado;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var cliente = await _clienteDAO.BuscarPorIdAsync(id)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        // O MySQL lançará exceção se houver veículos ou ordens vinculados.
        // Capturamos e traduzimos para mensagem amigável.
        try
        {
            var resultado = await _clienteDAO.ExcluirAsync(id);

            await _logService.RegistrarAsync(
                "sistema", "EXCLUSAO",
                $"Cliente '{cliente.Nome}' excluído.",
                TipoEvento.Aviso);

            return resultado;
        }
        catch (Exception ex) when (ex.Message.Contains("foreign key"))
        {
            throw new InvalidOperationException(
                "Não é possível excluir este cliente pois ele possui veículos ou ordens de serviço vinculados.");
        }
    }

    public async Task<Cliente?> BuscarPorIdAsync(int id)
        => await _clienteDAO.BuscarPorIdAsync(id);

    public async Task<List<Cliente>> ListarTodosAsync()
        => await _clienteDAO.ListarTodosAsync();

    public async Task<List<Cliente>> BuscarPorNomeAsync(string nome)
        => await _clienteDAO.BuscarPorNomeAsync(nome);

    public async Task<Cliente?> BuscarPorCpfCnpjAsync(string cpfCnpj)
        => await _clienteDAO.BuscarPorCpfCnpjAsync(cpfCnpj);

    private static void Validar(Cliente cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.Nome))
            throw new ArgumentException("O nome do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(cliente.CpfCnpj))
            throw new ArgumentException("O CPF ou CNPJ é obrigatório.");

        // Remove formatação para validar somente os dígitos.
        var soDigitos = new string(cliente.CpfCnpj.Where(char.IsDigit).ToArray());

        if (soDigitos.Length != 11 && soDigitos.Length != 14)
            throw new ArgumentException("CPF deve ter 11 dígitos e CNPJ deve ter 14 dígitos.");
    }
}

// ════════════════════════════════════════════════════════════════
// VeiculoService
// Regras de negócio: placa única, vínculo com cliente válido
// ════════════════════════════════════════════════════════════════
public class VeiculoService : IVeiculoService
{
    private readonly IVeiculoDAO _veiculoDAO;
    private readonly IClienteDAO _clienteDAO;
    private readonly ILogService _logService;

    public VeiculoService(IVeiculoDAO veiculoDAO, IClienteDAO clienteDAO, ILogService logService)
    {
        _veiculoDAO = veiculoDAO;
        _clienteDAO = clienteDAO;
        _logService = logService;
    }

    public async Task<int> SalvarAsync(Veiculo veiculo)
    {
        Validar(veiculo);

        // Verifica se o cliente existe antes de vincular.
        var cliente = await _clienteDAO.BuscarPorIdAsync(veiculo.ClienteId)
            ?? throw new InvalidOperationException("Cliente não encontrado.");

        // Verifica duplicidade de placa.
        var existente = await _veiculoDAO.BuscarPorPlacaAsync(veiculo.Placa);
        if (existente is not null && existente.Id != veiculo.Id)
            throw new InvalidOperationException(
                $"Já existe um veículo cadastrado com a placa '{veiculo.Placa}'.");

        var id = await _veiculoDAO.InserirAsync(veiculo);

        await _logService.RegistrarAsync(
            "sistema", "CADASTRO",
            $"Veículo {veiculo.Marca} {veiculo.Modelo} (placa: {veiculo.Placa}) " +
            $"cadastrado para o cliente '{cliente.Nome}'.");

        return id;
    }

    public async Task<bool> AtualizarAsync(Veiculo veiculo)
    {
        Validar(veiculo);

        var existente = await _veiculoDAO.BuscarPorPlacaAsync(veiculo.Placa);
        if (existente is not null && existente.Id != veiculo.Id)
            throw new InvalidOperationException(
                $"Já existe outro veículo com a placa '{veiculo.Placa}'.");

        var resultado = await _veiculoDAO.AtualizarAsync(veiculo);

        await _logService.RegistrarAsync(
            "sistema", "ALTERACAO",
            $"Veículo placa '{veiculo.Placa}' atualizado.");

        return resultado;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var veiculo = await _veiculoDAO.BuscarPorIdAsync(id)
            ?? throw new InvalidOperationException("Veículo não encontrado.");

        try
        {
            var resultado = await _veiculoDAO.ExcluirAsync(id);

            await _logService.RegistrarAsync(
                "sistema", "EXCLUSAO",
                $"Veículo placa '{veiculo.Placa}' excluído.",
                TipoEvento.Aviso);

            return resultado;
        }
        catch (Exception ex) when (ex.Message.Contains("foreign key"))
        {
            throw new InvalidOperationException(
                "Não é possível excluir este veículo pois ele possui ordens de serviço vinculadas.");
        }
    }

    public async Task<Veiculo?> BuscarPorIdAsync(int id)
        => await _veiculoDAO.BuscarPorIdAsync(id);

    public async Task<List<Veiculo>> ListarTodosAsync()
        => await _veiculoDAO.ListarTodosAsync();

    public async Task<List<Veiculo>> ListarPorClienteAsync(int clienteId)
        => await _veiculoDAO.ListarPorClienteAsync(clienteId);

    public async Task<Veiculo?> BuscarPorPlacaAsync(string placa)
        => await _veiculoDAO.BuscarPorPlacaAsync(placa);

    private static void Validar(Veiculo veiculo)
    {
        if (string.IsNullOrWhiteSpace(veiculo.Placa))
            throw new ArgumentException("A placa do veículo é obrigatória.");

        if (string.IsNullOrWhiteSpace(veiculo.Marca))
            throw new ArgumentException("A marca do veículo é obrigatória.");

        if (string.IsNullOrWhiteSpace(veiculo.Modelo))
            throw new ArgumentException("O modelo do veículo é obrigatório.");

        if (veiculo.Ano < 1900 || veiculo.Ano > DateTime.Now.Year + 1)
            throw new ArgumentException("Ano do veículo inválido.");

        if (veiculo.ClienteId <= 0)
            throw new ArgumentException("O cliente é obrigatório.");
    }
}
