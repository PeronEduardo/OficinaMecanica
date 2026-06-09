using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;

namespace OficinaMecanica.Services;

// ════════════════════════════════════════════════════════════════
// ServicoService
// ════════════════════════════════════════════════════════════════
public class ServicoService : IServicoService
{
    private readonly IServicoDAO _servicoDAO;
    private readonly ILogService _logService;

    public ServicoService(IServicoDAO servicoDAO, ILogService logService)
    {
        _servicoDAO = servicoDAO;
        _logService = logService;
    }

    public async Task<int> SalvarAsync(Servico servico)
    {
        Validar(servico);
        var id = await _servicoDAO.InserirAsync(servico);

        await _logService.RegistrarAsync(
            "sistema", "CADASTRO",
            $"Serviço '{servico.Descricao}' cadastrado com preço R$ {servico.Preco:F2}.");

        return id;
    }

    public async Task<bool> AtualizarAsync(Servico servico)
    {
        Validar(servico);
        var resultado = await _servicoDAO.AtualizarAsync(servico);

        await _logService.RegistrarAsync(
            "sistema", "ALTERACAO",
            $"Serviço '{servico.Descricao}' atualizado.");

        return resultado;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var servico = await _servicoDAO.BuscarPorIdAsync(id)
            ?? throw new InvalidOperationException("Serviço não encontrado.");

        try
        {
            var resultado = await _servicoDAO.ExcluirAsync(id);

            await _logService.RegistrarAsync(
                "sistema", "EXCLUSAO",
                $"Serviço '{servico.Descricao}' excluído.",
                TipoEvento.Aviso);

            return resultado;
        }
        catch (Exception ex) when (ex.Message.Contains("foreign key"))
        {
            throw new InvalidOperationException(
                "Não é possível excluir este serviço pois ele está vinculado a ordens de serviço.");
        }
    }

    public async Task<Servico?> BuscarPorIdAsync(int id)
        => await _servicoDAO.BuscarPorIdAsync(id);

    public async Task<List<Servico>> ListarTodosAsync()
        => await _servicoDAO.ListarTodosAsync();

    public async Task<List<Servico>> BuscarPorDescricaoAsync(string descricao)
        => await _servicoDAO.BuscarPorDescricaoAsync(descricao);

    public async Task<List<Servico>> ListarAtivosAsync()
        => await _servicoDAO.ListarAtivosAsync();

    private static void Validar(Servico servico)
    {
        if (string.IsNullOrWhiteSpace(servico.Descricao))
            throw new ArgumentException("A descrição do serviço é obrigatória.");

        if (servico.Preco < 0)
            throw new ArgumentException("O preço não pode ser negativo.");
    }
}

// ════════════════════════════════════════════════════════════════
// PecaService
// ════════════════════════════════════════════════════════════════
public class PecaService : IPecaService
{
    private readonly IPecaDAO _pecaDAO;
    private readonly ILogService _logService;

    public PecaService(IPecaDAO pecaDAO, ILogService logService)
    {
        _pecaDAO = pecaDAO;
        _logService = logService;
    }

    public async Task<int> SalvarAsync(Peca peca)
    {
        Validar(peca);
        var id = await _pecaDAO.InserirAsync(peca);

        await _logService.RegistrarAsync(
            "sistema", "CADASTRO",
            $"Peça '{peca.Descricao}' cadastrada. Estoque: {peca.Estoque}.");

        return id;
    }

    public async Task<bool> AtualizarAsync(Peca peca)
    {
        Validar(peca);
        var resultado = await _pecaDAO.AtualizarAsync(peca);

        // Registra alerta se estoque ficou abaixo do mínimo após atualização.
        if (peca.EstoqueBaixo)
        {
            await _logService.RegistrarAsync(
                "sistema", "ALERTA_ESTOQUE",
                $"Peça '{peca.Descricao}' com estoque baixo: {peca.Estoque} unidades " +
                $"(mínimo: {peca.EstoqueMinimo}).",
                TipoEvento.Aviso);
        }
        else
        {
            await _logService.RegistrarAsync(
                "sistema", "ALTERACAO",
                $"Peça '{peca.Descricao}' atualizada.");
        }

        return resultado;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var peca = await _pecaDAO.BuscarPorIdAsync(id)
            ?? throw new InvalidOperationException("Peça não encontrada.");

        try
        {
            var resultado = await _pecaDAO.ExcluirAsync(id);

            await _logService.RegistrarAsync(
                "sistema", "EXCLUSAO",
                $"Peça '{peca.Descricao}' excluída.",
                TipoEvento.Aviso);

            return resultado;
        }
        catch (Exception ex) when (ex.Message.Contains("foreign key"))
        {
            throw new InvalidOperationException(
                "Não é possível excluir esta peça pois ela está vinculada a ordens de serviço.");
        }
    }

    public async Task<Peca?> BuscarPorIdAsync(int id)
        => await _pecaDAO.BuscarPorIdAsync(id);

    public async Task<List<Peca>> ListarTodosAsync()
        => await _pecaDAO.ListarTodosAsync();

    public async Task<List<Peca>> BuscarPorDescricaoAsync(string descricao)
        => await _pecaDAO.BuscarPorDescricaoAsync(descricao);

    public async Task<List<Peca>> ListarAtivosAsync()
        => await _pecaDAO.ListarAtivosAsync();

    public async Task<List<Peca>> ListarEstoqueBaixoAsync()
        => await _pecaDAO.ListarEstoqueBaixoAsync();

    private static void Validar(Peca peca)
    {
        if (string.IsNullOrWhiteSpace(peca.Descricao))
            throw new ArgumentException("A descrição da peça é obrigatória.");

        if (peca.Preco < 0)
            throw new ArgumentException("O preço não pode ser negativo.");

        if (peca.Estoque < 0)
            throw new ArgumentException("O estoque não pode ser negativo.");

        if (peca.EstoqueMinimo < 0)
            throw new ArgumentException("O estoque mínimo não pode ser negativo.");
    }
}
