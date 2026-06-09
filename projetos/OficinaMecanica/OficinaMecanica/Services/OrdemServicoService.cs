using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;

namespace OficinaMecanica.Services;

// OrdemServicoService é o Service mais complexo do sistema.
// Responsabilidades:
//   - Validar os dados da OS antes de salvar
//   - Salvar os itens N:N (serviços e peças) junto com a OS
//   - Calcular o total automaticamente
//   - Controlar as transições de status
//   - Registrar todas as ações no log

public class OrdemServicoService : IOrdemServicoService
{
    private readonly IOrdemServicoDAO _ordemDAO;
    private readonly ILogService      _logService;

    public OrdemServicoService(IOrdemServicoDAO ordemDAO, ILogService logService)
    {
        _ordemDAO   = ordemDAO;
        _logService = logService;
    }

    // Salva a OS completa: cabeçalho + serviços + peças.
    // Tudo em sequência lógica — sem transação explícita aqui,
    // pois o MySQL garante a integridade pelas FKs e CASCADE.
    public async Task<int> SalvarAsync(OrdemServico ordem)
    {
        Validar(ordem);

        // Calcula o total antes de inserir.
        ordem.Total = CalcularTotal(ordem);

        // 1. Insere o cabeçalho da OS e obtém o ID gerado.
        var id = await _ordemDAO.InserirAsync(ordem);
        ordem.Id = id;

        // 2. Insere cada serviço na tabela intermediária.
        foreach (var item in ordem.Servicos)
        {
            item.OrdemId = id;
            await _ordemDAO.AdicionarServicoAsync(item);
        }

        // 3. Insere cada peça na tabela intermediária.
        foreach (var item in ordem.Pecas)
        {
            item.OrdemId = id;
            await _ordemDAO.AdicionarPecaAsync(item);
        }

        await _logService.RegistrarAsync(
            "sistema", "CADASTRO",
            $"Ordem de serviço #{id} aberta para o cliente ID {ordem.ClienteId}. " +
            $"Total: R$ {ordem.Total:F2}.");

        return id;
    }

    // Atualiza a OS: recalcula o total e substitui os itens N:N.
    public async Task<bool> AtualizarAsync(OrdemServico ordem)
    {
        Validar(ordem);

        ordem.Total = CalcularTotal(ordem);

        // Remove os itens antigos e insere os novos.
        // Estratégia "delete and re-insert" — simples e confiável.
        await _ordemDAO.RemoverServicosAsync(ordem.Id);
        await _ordemDAO.RemoverPecasAsync(ordem.Id);

        foreach (var item in ordem.Servicos)
        {
            item.OrdemId = ordem.Id;
            await _ordemDAO.AdicionarServicoAsync(item);
        }

        foreach (var item in ordem.Pecas)
        {
            item.OrdemId = ordem.Id;
            await _ordemDAO.AdicionarPecaAsync(item);
        }

        var resultado = await _ordemDAO.AtualizarAsync(ordem);

        await _logService.RegistrarAsync(
            "sistema", "ALTERACAO",
            $"Ordem de serviço #{ordem.Id} atualizada. Total: R$ {ordem.Total:F2}.");

        return resultado;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var ordem = await _ordemDAO.BuscarPorIdAsync(id)
            ?? throw new InvalidOperationException("Ordem de serviço não encontrada.");

        // Regra de negócio: não permite excluir OS concluída.
        if (ordem.Status == StatusOrdem.Concluida)
            throw new InvalidOperationException(
                "Não é possível excluir uma ordem de serviço já concluída.");

        var resultado = await _ordemDAO.ExcluirAsync(id);

        await _logService.RegistrarAsync(
            "sistema", "EXCLUSAO",
            $"Ordem de serviço #{id} excluída.",
            TipoEvento.Aviso);

        return resultado;
    }

    // Muda o status da OS com validação das transições permitidas.
    // Exemplo: não pode voltar de "Concluida" para "Aberta".
    public async Task<bool> AlterarStatusAsync(int ordemId, StatusOrdem novoStatus)
    {
        var ordem = await _ordemDAO.BuscarPorIdAsync(ordemId)
            ?? throw new InvalidOperationException("Ordem de serviço não encontrada.");

        ValidarTransicaoStatus(ordem.Status, novoStatus);

        ordem.Status = novoStatus;

        // Se está sendo concluída, registra a data de fechamento.
        if (novoStatus == StatusOrdem.Concluida)
            ordem.DataFechamento = DateTime.Today;

        var resultado = await _ordemDAO.AtualizarAsync(ordem);

        await _logService.RegistrarAsync(
            "sistema", "ALTERACAO",
            $"OS #{ordemId} teve status alterado para '{novoStatus}'.");

        return resultado;
    }

    // Recalcula o total da OS buscando os itens atuais do banco.
    public async Task RecalcularTotalAsync(int ordemId)
    {
        var ordem = await _ordemDAO.BuscarCompletoAsync(ordemId)
            ?? throw new InvalidOperationException("Ordem de serviço não encontrada.");

        ordem.Total = CalcularTotal(ordem);
        await _ordemDAO.AtualizarAsync(ordem);
    }

    public async Task<OrdemServico?> BuscarPorIdAsync(int id)
        => await _ordemDAO.BuscarPorIdAsync(id);

    public async Task<OrdemServico?> BuscarCompletoAsync(int ordemId)
        => await _ordemDAO.BuscarCompletoAsync(ordemId);

    public async Task<List<OrdemServico>> ListarTodosAsync()
        => await _ordemDAO.ListarTodosAsync();

    public async Task<List<OrdemServico>> ListarPorClienteAsync(int clienteId)
        => await _ordemDAO.ListarPorClienteAsync(clienteId);

    public async Task<List<OrdemServico>> ListarPorStatusAsync(StatusOrdem status)
        => await _ordemDAO.ListarPorStatusAsync(status);

    public async Task<List<OrdemServico>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim)
        => await _ordemDAO.ListarPorPeriodoAsync(inicio, fim);

    // ── Métodos privados ─────────────────────────────────────────────────

    // Soma o subtotal de todos os serviços e peças e desconta o desconto.
    private static decimal CalcularTotal(OrdemServico ordem)
    {
        var totalServicos = ordem.Servicos.Sum(s => s.Subtotal);
        var totalPecas    = ordem.Pecas.Sum(p => p.Subtotal);
        var total         = totalServicos + totalPecas - ordem.Desconto;

        // O total nunca pode ser negativo.
        return Math.Max(0, total);
    }

    private static void Validar(OrdemServico ordem)
    {
        if (ordem.ClienteId <= 0)
            throw new ArgumentException("O cliente é obrigatório.");

        if (ordem.VeiculoId <= 0)
            throw new ArgumentException("O veículo é obrigatório.");

        if (ordem.UsuarioId <= 0)
            throw new ArgumentException("O usuário responsável é obrigatório.");

        if (ordem.DataAbertura == default)
            throw new ArgumentException("A data de abertura é obrigatória.");

        if (!ordem.Servicos.Any() && !ordem.Pecas.Any())
            throw new ArgumentException(
                "A ordem de serviço deve ter pelo menos um serviço ou peça.");

        if (ordem.Desconto < 0)
            throw new ArgumentException("O desconto não pode ser negativo.");
    }

    // Define quais transições de status são permitidas.
    // Impede voltar de Concluída para qualquer outro estado.
    private static void ValidarTransicaoStatus(StatusOrdem atual, StatusOrdem novo)
    {
        if (atual == StatusOrdem.Concluida)
            throw new InvalidOperationException(
                "Uma ordem de serviço concluída não pode ter seu status alterado.");

        if (atual == StatusOrdem.Cancelada)
            throw new InvalidOperationException(
                "Uma ordem de serviço cancelada não pode ter seu status alterado.");
    }
}
