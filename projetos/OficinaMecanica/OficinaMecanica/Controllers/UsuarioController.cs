using OficinaMecanica.Interfaces.Controllers;
using OficinaMecanica.Interfaces.Services;
using OficinaMecanica.Models;

namespace OficinaMecanica.Controllers;

// UsuarioController gerencia autenticação.
// É o primeiro Controller a ser chamado — na tela de Login.
//
// Implementa IUsuarioController (contrato obrigatório do trabalho)
// e IBaseController (propriedades Sucesso e Mensagem).

public class UsuarioController : IUsuarioController
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogService     _logService;

    // Propriedades do IBaseController — refletem o estado
    // da última operação executada.
    public bool   Sucesso   { get; private set; }
    public string Mensagem  { get; private set; } = string.Empty;

    public UsuarioController(IUsuarioService usuarioService, ILogService logService)
    {
        _usuarioService = usuarioService;
        _logService     = logService;
    }

    // Autentica o usuário.
    // Retorna o objeto Usuario dentro de ResultadoOperacao se OK,
    // ou null com mensagem de erro se falhar.
    public async Task<ResultadoOperacao<Usuario?>> LoginAsync(string login, string senha)
    {
        try
        {
            // Validação básica antes de chamar o Service.
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
            {
                Sucesso  = false;
                Mensagem = "Preencha o login e a senha.";
                return ResultadoOperacao<Usuario?>.Falha(Mensagem);
            }

            var usuario = await _usuarioService.AutenticarAsync(login, senha);

            if (usuario is null)
            {
                Sucesso  = false;
                Mensagem = "Login ou senha incorretos.";
                return ResultadoOperacao<Usuario?>.Falha(Mensagem);
            }

            Sucesso  = true;
            Mensagem = $"Bem-vindo, {usuario.Nome}!";
            return ResultadoOperacao<Usuario?>.Ok(usuario, Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao realizar login. Verifique a conexão.";

            await _logService.RegistrarAsync(
                login, "ERRO",
                $"Erro no login: {ex.Message}",
                TipoEvento.Erro);

            return ResultadoOperacao<Usuario?>.Falha(Mensagem);
        }
    }

    // Registra o logout do usuário no log.
    public async Task<ResultadoOperacao<bool>> LogoutAsync(string login)
    {
        try
        {
            await _logService.RegistrarAsync(login, "LOGOUT", $"Usuário {login} saiu do sistema.");

            Sucesso  = true;
            Mensagem = "Logout realizado.";
            return ResultadoOperacao<bool>.Ok(true, Mensagem);
        }
        catch (Exception ex)
        {
            Sucesso  = false;
            Mensagem = "Erro ao registrar logout.";

            await _logService.RegistrarAsync(
                login, "ERRO",
                $"Erro no logout: {ex.Message}",
                TipoEvento.Erro);

            return ResultadoOperacao<bool>.Falha(Mensagem);
        }
    }

    // Cadastra um novo usuário com senha já aplicando o hash via Service.
    public async Task<ResultadoOperacao<int>> CadastrarAsync(Usuario usuario, string senha)
    {
        try
        {
            // O Service aplica o BCrypt antes de salvar.
            await _usuarioService.AlterarSenhaAsync(0, senha);
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha, workFactor: 12);

            var id = await _usuarioService.SalvarAsync(usuario);

            Sucesso  = true;
            Mensagem = "Usuário cadastrado com sucesso!";
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
            Mensagem = "Erro inesperado ao cadastrar usuário.";

            await _logService.RegistrarAsync(
                usuario.Login, "ERRO",
                $"Erro ao cadastrar usuário: {ex.Message}",
                TipoEvento.Erro);

            return ResultadoOperacao<int>.Falha(Mensagem);
        }
    }
}
