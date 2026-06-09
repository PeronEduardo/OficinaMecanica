using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;

namespace OficinaMecanica.Controllers;

// OrdemServicoController é o Controller central do sistema.
// Todas as operações da tela de ordens passam por aqui.

public class OrdemServicoController : IOrdemServicoController
{
    private readonly IOrdemServicoService _ordemService;
    private readonly ILogService          _logService;

    public bool   Sucesso  { get; private set; }
    public string Mensagem { get; private set; } = string.Empty;

    public OrdemServicoController(IOrdemServicoService ordemService, ILogService logService)
    {
        _ordemService = ordemService;
        _logService   = logService;
    }

    public async Task<ResultadoOperacao<int>> SalvarAsync(OrdemServico ordem)
    {
        try
        {
            int id;
            if (ordem.Id == 0)
            {
                id = await _ordemService.SalvarAsync(ordem);
                Mensagem = $"Ordem de serviço #{id} aberta com sucesso!";
            }
            else
            {
                await _ordemService.AtualizarAsync(ordem);
                id = ordem.Id;
                Mensagem = $"Ordem de serviço #{id} atualizada!";
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
            Mensagem = "Erro inesperado ao salvar ordem de serviço.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao salvar OS: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<int>.Falha(Mensagem);
        }
    }

    public async Task<ResultadoOperacao<bool>> ExcluirAsync(int id)
    {
        try
        {
            var resultado = await _ordemService.ExcluirAsync(id);
            Sucesso  = resultado;
            Mensagem = resultado ? "Ordem de serviço excluída!" : "Ordem não encontrada.";

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
            Mensagem = "Erro ao excluir ordem de serviço.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao excluir OS #{id}: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
    }

    public async Task<ResultadoOperacao<OrdemServico?>> BuscarCompletoAsync(int id)
    {
        try
        {
            var ordem = await _ordemService.BuscarCompletoAsync(id);
            Sucesso = ordem is not null;
            Mensagem = ordem is null ? "Ordem não encontrada." : string.Empty;

            return ordem is not null
                ? ResultadoOperacao<OrdemServico?>.Ok(ordem)
                : ResultadoOperacao<OrdemServico?>.Falha(Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao buscar ordem de serviço.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao buscar OS #{id}: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<OrdemServico?>.Falha(Mensagem);
        }
    }

    public async Task<ResultadoOperacao<List<OrdemServico>>> ListarAsync()
    {
        try
        {
            var lista = await _ordemService.ListarTodosAsync();
            Sucesso = true;
            return ResultadoOperacao<List<OrdemServico>>.Ok(lista);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao carregar ordens de serviço.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao listar OS: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<List<OrdemServico>>.Falha(Mensagem);
        }
    }

    public async Task<ResultadoOperacao<List<OrdemServico>>> ListarPorStatusAsync(StatusOrdem status)
    {
        try
        {
            var lista = await _ordemService.ListarPorStatusAsync(status);
            Sucesso = true;
            return ResultadoOperacao<List<OrdemServico>>.Ok(lista);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao filtrar ordens.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao filtrar OS por status: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<List<OrdemServico>>.Falha(Mensagem);
        }
    }

    public async Task<ResultadoOperacao<bool>> AlterarStatusAsync(int id, StatusOrdem status)
    {
        try
        {
            var resultado = await _ordemService.AlterarStatusAsync(id, status);
            Sucesso  = resultado;
            Mensagem = resultado
                ? $"Status alterado para '{status}' com sucesso!"
                : "Não foi possível alterar o status.";

            return resultado
                ? ResultadoOperacao<bool>.Ok(true, Mensagem)
                : ResultadoOperacao<bool>.Falha(Mensagem);
        }
        catch (InvalidOperationException ex)
        {
            // Regra de negócio: transição de status inválida.
            Sucesso  = false;
            Mensagem = ex.Message;
            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao alterar status.";

            await _logService.RegistrarAsync("sistema", "ERRO",
                $"Erro ao alterar status da OS #{id}: {ex.Message}", TipoEvento.Erro);

            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
    }
}
