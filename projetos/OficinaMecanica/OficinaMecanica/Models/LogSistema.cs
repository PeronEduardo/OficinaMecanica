using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace OficinaMecanica.Models;

// Representa um documento na coleção "logs" do MongoDB.
// Diferente dos outros Models, este NÃO tem tabela no MySQL.
// O MongoDB armazena documentos — não linhas — por isso o Id
// é um ObjectId (tipo nativo do MongoDB, não um INT simples).
//
// O atributo [BsonId] diz ao driver do MongoDB qual campo é o _id.
// O atributo [BsonElement("...")] mapeia o nome do campo no documento.

public class LogSistema
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string?   Id          { get; set; }    // _id do MongoDB (gerado automaticamente)

    [BsonElement("usuario")]
    public string    Usuario     { get; set; } = string.Empty;

    [BsonElement("acao")]
    public string    Acao        { get; set; } = string.Empty;   // ex: "LOGIN", "CADASTRO"

    [BsonElement("descricao")]
    public string    Descricao   { get; set; } = string.Empty;   // detalhes da ação

    [BsonElement("tipo_evento")]
    public TipoEvento TipoEvento { get; set; } = TipoEvento.Info;

    [BsonElement("data_hora")]
    public DateTime  DataHora    { get; set; } = DateTime.Now;

    [BsonElement("ip")]
    public string?   Ip          { get; set; }    // opcional — IP do dispositivo
}

// Tipos de evento para categorizar os logs.
// Usado tanto no MongoDB quanto na exportação XML.
public enum TipoEvento
{
    Info,       // operação normal (login, cadastro, alteração)
    Aviso,      // algo inesperado mas não crítico
    Erro        // exceção ou falha no sistema
}
