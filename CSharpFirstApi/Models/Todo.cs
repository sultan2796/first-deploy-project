using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CSharpFirstApi.Models
{
    public class Todo
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public bool Done { get; set; }
    }
}