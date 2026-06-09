namespace OficinaMecanica.Models;

// Representa a tabela "usuarios" do MySQL.
// Armazena os funcionários que acessam o sistema.
// O campo SenhaHash nunca guarda a senha em texto puro —
// apenas o resultado do BCrypt.

public class Usuario
{
    public int     Id            { get; set; }
    public string  Nome          { get; set; } = string.Empty;
    public string  Login         { get; set; } = string.Empty;
    public string  SenhaHash     { get; set; } = string.Empty;
    public PerfilUsuario Perfil  { get; set; } = PerfilUsuario.Atendente;
    public bool    Ativo         { get; set; } = true;
    public string? FotoPath      { get; set; }       // caminho da imagem (requisito 10)
    public DateTime CriadoEm    { get; set; } = DateTime.Now;
    public DateTime? AtualizadoEm { get; set; }
}

// Enum separado que representa os perfis possíveis.
// Espelha o ENUM('admin','mecanico','atendente') do MySQL.
public enum PerfilUsuario
{
    Admin,
    Mecanico,
    Atendente
}
