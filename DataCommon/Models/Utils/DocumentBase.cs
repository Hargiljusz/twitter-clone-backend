using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCommon.Models.Utils
{
    public abstract class DocumentBase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public BackendType CreatedByBackendType { get; set; }
        public BackendType UpdatedByBackendType { get; set; }
        public DocumentBase()
        {

        }

        protected DocumentBase(DateTime createdAt, BackendType createdByBackendType, BackendType updatedByBackendType)
        {
            CreatedAt = createdAt;
            CreatedByBackendType = createdByBackendType;
            UpdatedByBackendType = updatedByBackendType;
        }

        protected DocumentBase(DateTime createdAt, BackendType createdByBackendType)
        {
            CreatedAt = createdAt;
            CreatedByBackendType = createdByBackendType;
        }

        protected DocumentBase(string id, DateTime createdAt, BackendType createdByBackendType, BackendType updatedByBackendType)
        {
            Id = id;
            CreatedAt = createdAt;
            CreatedByBackendType = createdByBackendType;
            UpdatedByBackendType = updatedByBackendType;
        }
    }
}
