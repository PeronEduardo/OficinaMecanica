namespace OficinaMecanica.Interfaces.DAO;

// Interface genérica que define as operações CRUD básicas.
// O "T" é substituído pelo Model concreto na hora de usar.
//
// Exemplo:
//   IBaseDAO<Cliente>  → DAO de clientes
//   IBaseDAO<Peca>     → DAO de peças
//
// "Task" indica que os métodos são assíncronos — eles não travam
// a tela enquanto aguardam a resposta do banco de dados.
// Isso é essencial em aplicativos mobile (MAUI).

public interface IBaseDAO<T>
{
    // Insere um novo registro no banco.
    // Retorna o ID gerado automaticamente pelo banco (AUTO_INCREMENT).
    Task<int> InserirAsync(T entidade);

    // Atualiza um registro existente.
    // Retorna true se atualizou com sucesso, false se não encontrou.
    Task<bool> AtualizarAsync(T entidade);

    // Remove um registro pelo ID.
    // Retorna true se removeu, false se não encontrou.
    Task<bool> ExcluirAsync(int id);

    // Busca um único registro pelo ID.
    // Retorna null se não encontrar (por isso o "T?").
    Task<T?> BuscarPorIdAsync(int id);

    // Retorna todos os registros da tabela.
    Task<List<T>> ListarTodosAsync();
}
