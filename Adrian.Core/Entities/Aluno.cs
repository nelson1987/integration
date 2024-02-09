using MongoDB.Bson.Serialization.Attributes;

namespace Adrian.Core.Entities;

public class Aluno
{
    [BsonId]
    public Guid Id { get; set; }
    public required string Nome { get; set; }
    public required string Documento { get; set; }
    public required StatusAluno Status { get; set; }
}

public enum StatusAluno
{
    Criado = 1,
    Matriculado = 2,
    ProvaRealizada = 3,
    Aprovado = 4,
    Inscrito = 5,
    MatriculaMateria = 6,
    MatriculaCancelada = 7,
    Repovado = 8
}