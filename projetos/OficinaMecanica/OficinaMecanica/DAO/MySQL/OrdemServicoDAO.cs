using Dapper;
using OficinaMecanica.Infrastructure;
using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Models;

namespace OficinaMecanica.DAO.MySQL;

// OrdemServicoDAO é o DAO mais complexo do sistema.
// Lida com 3 tabelas: ordens_servico, ordem_servico_servicos,
// ordem_servico_pecas — além de joins com clientes e veiculos.

public class OrdemServicoDAO : IOrdemServicoDAO
{
    private readonly MySqlConnectionFactory _factory;

    public OrdemServicoDAO(MySqlConnectionFactory factory)
    {
        _factory = factory;
    }

    public async Task<int> InserirAsync(OrdemServico ordem)
    {
        const string sql = @"
            INSERT INTO ordens_servico
                (cliente_id, veiculo_id, usuario_id, data_abertura,
                 data_previsao, status, quilometragem_entrada, observacoes, desconto, total)
            VALUES
                (@ClienteId, @VeiculoId, @UsuarioId, @DataAbertura,
                 @DataPrevisao, @Status, @QuilometragemEntrada, @Observacoes, @Desconto, @Total);
            SELECT LAST_INSERT_ID();";

        using var conn = await _factory.CriarAsync();
        return await conn.ExecuteScalarAsync<int>(sql, new
        {
            ordem.ClienteId,
            ordem.VeiculoId,
            ordem.UsuarioId,
            ordem.DataAbertura,
            ordem.DataPrevisao,
            Status = ordem.Status.ToString().ToLower(),
            ordem.QuilometragemEntrada,
            ordem.Observacoes,
            ordem.Desconto,
            ordem.Total
        });
    }

    public async Task<bool> AtualizarAsync(OrdemServico ordem)
    {
        const string sql = @"
            UPDATE ordens_servico
               SET cliente_id            = @ClienteId,
                   veiculo_id            = @VeiculoId,
                   data_previsao         = @DataPrevisao,
                   data_fechamento       = @DataFechamento,
                   status                = @Status,
                   quilometragem_entrada  = @QuilometragemEntrada,
                   observacoes           = @Observacoes,
                   desconto              = @Desconto,
                   total                 = @Total
             WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        var linhas = await conn.ExecuteAsync(sql, new
        {
            ordem.ClienteId,
            ordem.VeiculoId,
            ordem.DataPrevisao,
            ordem.DataFechamento,
            Status = ordem.Status.ToString().ToLower(),
            ordem.QuilometragemEntrada,
            ordem.Observacoes,
            ordem.Desconto,
            ordem.Total,
            ordem.Id
        });
        return linhas > 0;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        // As tabelas intermediárias serão removidas automaticamente
        // pelo ON DELETE CASCADE definido no script SQL.
        const string sql = "DELETE FROM ordens_servico WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        var linhas = await conn.ExecuteAsync(sql, new { Id = id });
        return linhas > 0;
    }

    public async Task<OrdemServico?> BuscarPorIdAsync(int id)
    {
        const string sql = "SELECT * FROM ordens_servico WHERE id = @Id";

        using var conn = await _factory.CriarAsync();
        return await conn.QueryFirstOrDefaultAsync<OrdemServico>(sql, new { Id = id });
    }

    // Busca a ordem com todos os dados relacionados:
    // cliente, veículo, lista de serviços e lista de peças.
    public async Task<OrdemServico?> BuscarCompletoAsync(int ordemId)
    {
        using var conn = await _factory.CriarAsync();

        // Busca a ordem com cliente e veículo em um único JOIN.
        const string sqlOrdem = @"
            SELECT os.*,
                   c.id, c.nome, c.cpf_cnpj, c.telefone,
                   v.id, v.placa, v.marca, v.modelo, v.ano
              FROM ordens_servico os
              JOIN clientes c ON c.id = os.cliente_id
              JOIN veiculos v ON v.id = os.veiculo_id
             WHERE os.id = @OrdemId";

        // O Dapper Multi-Mapping permite mapear o resultado de um JOIN
        // em múltiplos objetos de uma vez.
        // A string "id,id,id" informa onde começa cada objeto no resultado.
        var ordens = await conn.QueryAsync<OrdemServico, Cliente, Veiculo, OrdemServico>(
            sqlOrdem,
            (ordem, cliente, veiculo) =>
            {
                ordem.Cliente = cliente;
                ordem.Veiculo = veiculo;
                return ordem;
            },
            new { OrdemId = ordemId },
            splitOn: "id,id"
        );

        var ordem = ordens.FirstOrDefault();
        if (ordem is null) return null;

        // Busca os serviços da ordem com dados do catálogo.
        const string sqlServicos = @"
            SELECT oss.*, s.id, s.descricao, s.preco
              FROM ordem_servico_servicos oss
              JOIN servicos s ON s.id = oss.servico_id
             WHERE oss.ordem_id = @OrdemId";

        var servicos = await conn.QueryAsync<OrdemServicoServico, Servico, OrdemServicoServico>(
            sqlServicos,
            (item, servico) => { item.Servico = servico; return item; },
            new { OrdemId = ordemId },
            splitOn: "id"
        );
        ordem.Servicos = servicos.ToList();

        // Busca as peças da ordem com dados do catálogo.
        const string sqlPecas = @"
            SELECT osp.*, p.id, p.descricao, p.preco
              FROM ordem_servico_pecas osp
              JOIN pecas p ON p.id = osp.peca_id
             WHERE osp.ordem_id = @OrdemId";

        var pecas = await conn.QueryAsync<OrdemServicoPeca, Peca, OrdemServicoPeca>(
            sqlPecas,
            (item, peca) => { item.Peca = peca; return item; },
            new { OrdemId = ordemId },
            splitOn: "id"
        );
        ordem.Pecas = pecas.ToList();

        return ordem;
    }

    public async Task<List<OrdemServico>> ListarTodosAsync()
    {
        const string sql = @"
    SELECT Id, Status, DataAbertura, DataFechamento,
           Total, ClienteId, VeiculoId, UsuarioId,
           ClienteNome, VeiculoPlaca, VeiculoMarca,
           VeiculoModelo, Atendente
    FROM vw_ordens_resumo
    ORDER BY DataAbertura DESC";

        using var conn = await _factory.CriarAsync();
        var rows = await conn.QueryAsync(sql);

        return rows.Select(r => new OrdemServico
        {
            Id = (int)r.Id,
            ClienteId = (int)r.ClienteId,
            VeiculoId = (int)r.VeiculoId,
            UsuarioId = (int)r.UsuarioId,
            DataAbertura = (DateTime)r.DataAbertura,
            DataFechamento = r.DataFechamento == null ? (DateTime?)null : (DateTime)r.DataFechamento,
            Total = (decimal)r.Total,
            ClienteNome = (string?)r.ClienteNome,
            VeiculoPlaca = (string?)r.VeiculoPlaca,
            VeiculoMarca = (string?)r.VeiculoMarca,
            VeiculoModelo = (string?)r.VeiculoModelo,
            Atendente = (string?)r.Atendente,
            Status = ((string)r.Status) switch
            {
                "em_andamento" => StatusOrdem.EmAndamento,
                "aguardando_peca" => StatusOrdem.AguardandoPeca,
                "concluida" => StatusOrdem.Concluida,
                "cancelada" => StatusOrdem.Cancelada,
                _ => StatusOrdem.Aberta
            }
        }).ToList();
    }
    public async Task<List<OrdemServico>> ListarPorClienteAsync(int clienteId)
    {
        const string sql = @"
            SELECT * FROM ordens_servico
             WHERE cliente_id = @ClienteId
             ORDER BY data_abertura DESC";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<OrdemServico>(sql, new { ClienteId = clienteId });
        return resultado.ToList();
    }

    public async Task<List<OrdemServico>> ListarPorVeiculoAsync(int veiculoId)
    {
        const string sql = @"
            SELECT * FROM ordens_servico
             WHERE veiculo_id = @VeiculoId
             ORDER BY data_abertura DESC";

        using var conn = await _factory.CriarAsync();
        var resultado = await conn.QueryAsync<OrdemServico>(sql, new { VeiculoId = veiculoId });
        return resultado.ToList();
    }

    public async Task<List<OrdemServico>> ListarPorStatusAsync(StatusOrdem status)
    {
        const string sql = @"
    SELECT Id, Status, DataAbertura, DataFechamento,
           Total, ClienteId, VeiculoId, UsuarioId,
           ClienteNome, VeiculoPlaca, VeiculoMarca,
           VeiculoModelo, Atendente
    FROM vw_ordens_resumo
    WHERE Status = @Status
    ORDER BY DataAbertura DESC";

        var statusStr = status switch
        {
            StatusOrdem.EmAndamento => "em_andamento",
            StatusOrdem.AguardandoPeca => "aguardando_peca",
            StatusOrdem.Concluida => "concluida",
            StatusOrdem.Cancelada => "cancelada",
            _ => "aberta"
        };

        using var conn = await _factory.CriarAsync();
        var rows = await conn.QueryAsync(sql, new { Status = statusStr });

        return rows.Select(r => new OrdemServico
        {
            Id = (int)r.Id,
            ClienteId = (int)r.ClienteId,
            VeiculoId = (int)r.VeiculoId,
            UsuarioId = (int)r.UsuarioId,
            DataAbertura = (DateTime)r.DataAbertura,
            DataFechamento = r.DataFechamento == null ? (DateTime?)null : (DateTime)r.DataFechamento,
            Total = (decimal)r.Total,
            ClienteNome = (string?)r.ClienteNome,
            VeiculoPlaca = (string?)r.VeiculoPlaca,
            VeiculoMarca = (string?)r.VeiculoMarca,
            VeiculoModelo = (string?)r.VeiculoModelo,
            Atendente = (string?)r.Atendente,
            Status = ((string)r.Status) switch
            {
                "em_andamento" => StatusOrdem.EmAndamento,
                "aguardando_peca" => StatusOrdem.AguardandoPeca,
                "concluida" => StatusOrdem.Concluida,
                "cancelada" => StatusOrdem.Cancelada,
                _ => StatusOrdem.Aberta
            }
        }).ToList();
    }
    public async Task<List<OrdemServico>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim)
    {
        const string sql = @"
    SELECT Id, Status, DataAbertura, DataFechamento,
           Total, ClienteId, VeiculoId, UsuarioId,
           ClienteNome, VeiculoPlaca, VeiculoMarca,
           VeiculoModelo, Atendente
    FROM vw_ordens_resumo
    WHERE DataAbertura BETWEEN @Inicio AND @Fim
    ORDER BY DataAbertura DESC";

        using var conn = await _factory.CriarAsync();
        var rows = await conn.QueryAsync(sql, new { Inicio = inicio, Fim = fim });

        return rows.Select(r => new OrdemServico
        {
            Id = (int)r.Id,
            ClienteId = (int)r.ClienteId,
            VeiculoId = (int)r.VeiculoId,
            UsuarioId = (int)r.UsuarioId,
            DataAbertura = (DateTime)r.DataAbertura,
            DataFechamento = r.DataFechamento == null ? (DateTime?)null : (DateTime)r.DataFechamento,
            Total = (decimal)r.Total,
            ClienteNome = (string?)r.ClienteNome,
            VeiculoPlaca = (string?)r.VeiculoPlaca,
            VeiculoMarca = (string?)r.VeiculoMarca,
            VeiculoModelo = (string?)r.VeiculoModelo,
            Atendente = (string?)r.Atendente,
            Status = ((string)r.Status) switch
            {
                "em_andamento" => StatusOrdem.EmAndamento,
                "aguardando_peca" => StatusOrdem.AguardandoPeca,
                "concluida" => StatusOrdem.Concluida,
                "cancelada" => StatusOrdem.Cancelada,
                _ => StatusOrdem.Aberta
            }
        }).ToList();
    }
    // ── Tabelas intermediárias N:N ────────────────────────────────────────

    public async Task AdicionarServicoAsync(OrdemServicoServico item)
    {
        const string sql = @"
            INSERT INTO ordem_servico_servicos (ordem_id, servico_id, quantidade, preco_unitario, observacao)
            VALUES (@OrdemId, @ServicoId, @Quantidade, @PrecoUnitario, @Observacao)
            ON DUPLICATE KEY UPDATE
                quantidade     = @Quantidade,
                preco_unitario = @PrecoUnitario";

        // ON DUPLICATE KEY UPDATE: se o par (ordem_id, servico_id) já existir,
        // atualiza em vez de gerar erro — útil ao editar uma OS.
        using var conn = await _factory.CriarAsync();
        await conn.ExecuteAsync(sql, item);
    }

    public async Task AdicionarPecaAsync(OrdemServicoPeca item)
    {
        const string sql = @"
            INSERT INTO ordem_servico_pecas (ordem_id, peca_id, quantidade, preco_unitario, observacao)
            VALUES (@OrdemId, @PecaId, @Quantidade, @PrecoUnitario, @Observacao)
            ON DUPLICATE KEY UPDATE
                quantidade     = @Quantidade,
                preco_unitario = @PrecoUnitario";

        using var conn = await _factory.CriarAsync();
        await conn.ExecuteAsync(sql, item);
    }

    public async Task RemoverServicosAsync(int ordemId)
    {
        const string sql = "DELETE FROM ordem_servico_servicos WHERE ordem_id = @OrdemId";

        using var conn = await _factory.CriarAsync();
        await conn.ExecuteAsync(sql, new { OrdemId = ordemId });
    }

    public async Task RemoverPecasAsync(int ordemId)
    {
        const string sql = "DELETE FROM ordem_servico_pecas WHERE ordem_id = @OrdemId";

        using var conn = await _factory.CriarAsync();
        await conn.ExecuteAsync(sql, new { OrdemId = ordemId });
    }
}
