using SQLite;

namespace OficinaMecanica.Models;

// Representa a tabela "configuracoes" do SQLite local.
// Armazena preferências do aplicativo no dispositivo do usuário.
// Funciona como um dicionário chave-valor persistente.
//
// Exemplos de uso:
//   Chave = "tema"          → Valor = "escuro"
//   Chave = "ultimo_login"  → Valor = "admin"
//   Chave = "ultimo_filtro" → Valor = "aberta"
//
// O atributo [Table] define o nome da tabela no SQLite.
// O atributo [PrimaryKey] define a chave primária.
// O atributo [MaxLength] limita o tamanho do campo.

[Table("configuracoes")]
public class Configuracao
{
    [PrimaryKey]
    [MaxLength(100)]
    public string Chave  { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Valor  { get; set; } = string.Empty;

    // Constantes para evitar erros de digitação ao usar as chaves.
    // Em vez de escrever "tema" espalhado pelo código,
    // usa-se Configuracao.ChaveTema em qualquer lugar.
    public const string ChaveTema          = "tema";
    public const string ChaveUltimoUsuario = "ultimo_usuario";
    public const string ChaveUltimoFiltro  = "ultimo_filtro";
    public const string ChaveUltimaBusca   = "ultima_busca";
}
