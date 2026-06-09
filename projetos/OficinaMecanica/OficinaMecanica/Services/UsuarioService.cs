using OficinaMecanica.Interfaces.DAO;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;

namespace OficinaMecanica.Services;

// UsuarioService contém as regras de negócio de autenticação.
// A regra mais importante: NUNCA salvar senha em texto puro.
// BCrypt gera um hash seguro com salt automático.
//
// Como o BCrypt funciona:
//   "Admin@123"  →  BCrypt.HashPassword()  →  "$2a$12$K8Gp..."
//   Na verificação: BCrypt.Verify("Admin@123", "$2a$12$K8Gp...") → true
//   O hash é diferente a cada vez, mas a verificação sempre funciona.

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioDAO _usuarioDAO;
    private readonly ILogService _logService;

    public UsuarioService(IUsuarioDAO usuarioDAO, ILogService logService)
    {
        _usuarioDAO = usuarioDAO;
        _logService = logService;
    }

    // Autentica o usuário comparando a senha com o hash armazenado.
    // Retorna o objeto Usuario se autenticado, null se falhar.
    public async Task<Usuario?> AutenticarAsync(string login, string senha)
    {
        // resto do código...    {
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
            return null;

        var usuario = await _usuarioDAO.BuscarPorLoginAsync(login);

        // Se o usuário não existe ou está inativo, nega o acesso.
        if (usuario is null || !usuario.Ativo)
        {
            await _logService.RegistrarAsync(
                login, "LOGIN_FALHA",
                $"Tentativa de login com usuário inexistente ou inativo: {login}",
                TipoEvento.Aviso);
            return null;
        }

        // BCrypt.Verify compara a senha digitada com o hash do banco.
        // É seguro mesmo que o banco seja comprometido —
        // o hash não pode ser revertido para a senha original.
        var senhaCorreta = BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash);

        if (!senhaCorreta)
        {
            await _logService.RegistrarAsync(
                login, "LOGIN_FALHA",
                $"Senha incorreta para o usuário: {login}",
                TipoEvento.Aviso);
            return null;
        }

        await _logService.RegistrarAsync(
            login, "LOGIN",
            $"Usuário {usuario.Nome} fez login no sistema.");

        return usuario;
    }

    public async Task<int> SalvarAsync(Usuario usuario)
    {
        Validar(usuario);

        // Verifica se o login já está em uso.
        var existente = await _usuarioDAO.BuscarPorLoginAsync(usuario.Login);
        if (existente is not null && existente.Id != usuario.Id)
            throw new InvalidOperationException($"O login '{usuario.Login}' já está em uso.");

        var id = await _usuarioDAO.InserirAsync(usuario);

        await _logService.RegistrarAsync(
            usuario.Login, "CADASTRO",
            $"Usuário '{usuario.Nome}' cadastrado com perfil {usuario.Perfil}.");

        return id;
    }

    public async Task<bool> AtualizarAsync(Usuario usuario)
    {
        Validar(usuario);

        var resultado = await _usuarioDAO.AtualizarAsync(usuario);

        await _logService.RegistrarAsync(
            usuario.Login, "ALTERACAO",
            $"Dados do usuário '{usuario.Nome}' foram atualizados.");

        return resultado;
    }

    // Altera a senha aplicando BCrypt antes de salvar.
    public async Task<bool> AlterarSenhaAsync(int usuarioId, string novaSenha)
    {
        if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Length < 6)
            throw new ArgumentException("A senha deve ter no mínimo 6 caracteres.");

        var usuario = await _usuarioDAO.BuscarPorIdAsync(usuarioId)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        // WorkFactor 12 = número de rounds do BCrypt.
        // Maior = mais seguro, porém mais lento. 12 é o padrão recomendado.
        usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha, workFactor: 12);

        var resultado = await _usuarioDAO.AtualizarAsync(usuario);

        await _logService.RegistrarAsync(
            usuario.Login, "ALTERACAO",
            "Senha do usuário foi alterada.");

        return resultado;
    }

    public async Task<bool> ExcluirAsync(int id)
    {
        var usuario = await _usuarioDAO.BuscarPorIdAsync(id)
            ?? throw new InvalidOperationException("Usuário não encontrado.");

        var resultado = await _usuarioDAO.ExcluirAsync(id);

        await _logService.RegistrarAsync(
            usuario.Login, "EXCLUSAO",
            $"Usuário '{usuario.Nome}' foi excluído do sistema.",
            TipoEvento.Aviso);

        return resultado;
    }

    public async Task<Usuario?> BuscarPorIdAsync(int id)
        => await _usuarioDAO.BuscarPorIdAsync(id);

    public async Task<List<Usuario>> ListarTodosAsync()
        => await _usuarioDAO.ListarTodosAsync();

    // Validações básicas — lançam exceção com mensagem para o usuário.
    private static void Validar(Usuario usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.Nome))
            throw new ArgumentException("O nome do usuário é obrigatório.");

        if (string.IsNullOrWhiteSpace(usuario.Login))
            throw new ArgumentException("O login é obrigatório.");

        if (usuario.Login.Length < 3)
            throw new ArgumentException("O login deve ter no mínimo 3 caracteres.");
    }
}
