using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;

namespace OficinaMecanica.Controllers;

// ════════════════════════════════════════════════════════════════
// ClienteController
// Mediador entre ClientesListaPage/ClienteFormPage e ClienteService.
// Todo try/catch fica aqui — a View só lê ResultadoOperacao.
// ════════════════════════════════════════════════════════════════
public class ClienteController : IClienteController
{
    private readonly IClienteService _clienteService;
    private readonly ILogService     _logService;

    public bool   Sucesso  { get; private set; }
    public string Mensagem { get; private set; } = string.Empty;

    public ClienteController(IClienteService clienteService, ILogService logService)
    {
        _clienteService = clienteService;
        _logService     = logService;
    }

    public async Task<ResultadoOperacao<int>> SalvarAsync(Cliente cliente)
    {
        try
        {
            // Se Id > 0 é atualização, se Id == 0 é inserção.
            int id;
            if (cliente.Id == 0)
            {
                id = await _clienteService.SalvarAsync(cliente);
                Mensagem = "Cliente cadastrado com sucesso!";
            }
            else
            {
                await _clienteService.AtualizarAsync(cliente);
                id = cliente.Id;
                Mensagem = "Cliente atualizado com sucesso!";
            }

            Sucesso = true;
            return ResultadoOperacao<int>.Ok(id, Mensagem);
        }
        catch (ArgumentException ex)
        {
            // Erro de validação — mensagem amigável para o usuário.
            Sucesso  = false;
            Mensagem = ex.Message;
            return ResultadoOperacao<int>.Falha(Mensagem);
        }
        catch (InvalidOperationException ex)
        {
            // Regra de negócio violada (ex: CPF duplicado).
            Sucesso  = false;
            Mensagem = ex.Message;
            return ResultadoOperacao<int>.Falha(Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro inesperado ao salvar cliente.";

            await _logService.RegistrarAsync(
                "sistema", "ERRO",
                $"Erro ao salvar cliente: {ex.Message}",
                TipoEvento.Erro);

            return ResultadoOperacao<int>.Falha(Mensagem);
        }
    }

    public async Task<ResultadoOperacao<bool>> ExcluirAsync(int id)
    {
        try
        {
            var resultado = await _clienteService.ExcluirAsync(id);

            Sucesso  = resultado;
            Mensagem = resultado
                ? "Cliente excluído com sucesso!"
                : "Cliente não encontrado.";

            return resultado
                ? ResultadoOperacao<bool>.Ok(true, Mensagem)
                : ResultadoOperacao<bool>.Falha(Mensagem);
        }
        catch (InvalidOperationException ex)
        {
            Sucesso  = false;
            Mensagem = ex.Message;
            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro inesperado ao excluir cliente.";

            await _logService.RegistrarAsync(
                "sistema", "ERRO",
                $"Erro ao excluir cliente ID {id}: {ex.Message}",
                TipoEvento.Erro);

            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
    }

    public async Task<ResultadoOperacao<Cliente?>> BuscarPorIdAsync(int id)
    {
        try
        {
            var cliente = await _clienteService.BuscarPorIdAsync(id);

            Sucesso  = cliente is not null;
            Mensagem = cliente is null ? "Cliente não encontrado." : string.Empty;

            return cliente is not null
                ? ResultadoOperacao<Cliente?>.Ok(cliente)
                : ResultadoOperacao<Cliente?>.Falha(Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao buscar cliente.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao buscar cliente ID {id}: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<Cliente?>.Falha(Mensagem);
        }
    }

    public async Task<ResultadoOperacao<List<Cliente>>> ListarAsync()
    {
        try
        {
            var lista = await _clienteService.ListarTodosAsync();
            Sucesso = true;
            return ResultadoOperacao<List<Cliente>>.Ok(lista);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao carregar clientes.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao listar clientes: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<List<Cliente>>.Falha(Mensagem);
        }
    }

    public async Task<ResultadoOperacao<List<Cliente>>> BuscarAsync(string termo)
    {
        try
        {
            var lista = await _clienteService.BuscarPorNomeAsync(termo);
            Sucesso = true;
            return ResultadoOperacao<List<Cliente>>.Ok(lista);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao buscar clientes.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao buscar clientes por '{termo}': {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<List<Cliente>>.Falha(Mensagem);
        }
    }
}

// ════════════════════════════════════════════════════════════════
// VeiculoController
// ════════════════════════════════════════════════════════════════
public class VeiculoController : IVeiculoController
{
    private readonly IVeiculoService _veiculoService;
    private readonly ILogService     _logService;

    public bool   Sucesso  { get; private set; }
    public string Mensagem { get; private set; } = string.Empty;

    public VeiculoController(IVeiculoService veiculoService, ILogService logService)
    {
        _veiculoService = veiculoService;
        _logService     = logService;
    }

    public async Task<ResultadoOperacao<int>> SalvarAsync(Veiculo veiculo)
    {
        try
        {
            int id;
            if (veiculo.Id == 0)
            {
                id = await _veiculoService.SalvarAsync(veiculo);
                Mensagem = "Veículo cadastrado com sucesso!";
            }
            else
            {
                await _veiculoService.AtualizarAsync(veiculo);
                id = veiculo.Id;
                Mensagem = "Veículo atualizado com sucesso!";
            }

            Sucesso = true;
            return ResultadoOperacao<int>.Ok(id, Mensagem);
        }
        catch (ArgumentException ex)
        {
            Sucesso  = false;
            Mensagem = ex.Message;
            return ResultadoOperacao<int>.Falha(Mensagem);
        }
        catch (InvalidOperationException ex)
        {
            Sucesso  = false;
            Mensagem = ex.Message;
            return ResultadoOperacao<int>.Falha(Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro inesperado ao salvar veículo.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao salvar veículo: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<int>.Falha(Mensagem);
        }
    }

    public async Task<ResultadoOperacao<bool>> ExcluirAsync(int id)
    {
        try
        {
            var resultado = await _veiculoService.ExcluirAsync(id);
            Sucesso  = resultado;
            Mensagem = resultado ? "Veículo excluído!" : "Veículo não encontrado.";

            return resultado
                ? ResultadoOperacao<bool>.Ok(true, Mensagem)
                : ResultadoOperacao<bool>.Falha(Mensagem);
        }
        catch (InvalidOperationException ex)
        {
            Sucesso  = false;
            Mensagem = ex.Message;
            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro inesperado ao excluir veículo.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao excluir veículo ID {id}: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
    }

    public async Task<ResultadoOperacao<Veiculo?>> BuscarPorIdAsync(int id)
    {
        try
        {
            var veiculo = await _veiculoService.BuscarPorIdAsync(id);
            Sucesso = veiculo is not null;
            Mensagem = veiculo is null ? "Veículo não encontrado." : string.Empty;

            return veiculo is not null
                ? ResultadoOperacao<Veiculo?>.Ok(veiculo)
                : ResultadoOperacao<Veiculo?>.Falha(Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao buscar veículo.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao buscar veículo ID {id}: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<Veiculo?>.Falha(Mensagem);
        }
    }

    public async Task<ResultadoOperacao<List<Veiculo>>> ListarPorClienteAsync(int clienteId)
    {
        try
        {
            var lista = await _veiculoService.ListarPorClienteAsync(clienteId);
            Sucesso = true;
            return ResultadoOperacao<List<Veiculo>>.Ok(lista);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao carregar veículos.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao listar veículos do cliente {clienteId}: {ex.Message}",
                TipoEvento.Erro);

            return ResultadoOperacao<List<Veiculo>>.Falha(Mensagem);
        }
    }
}
