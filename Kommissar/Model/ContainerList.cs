using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Kommissar.Model;

public class ContainerList
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Namespace { get; set; }
    public string Environment { get; set; }
    public List<Container> Containers { get; set; }
}