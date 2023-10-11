using DataCommon.Models.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCommon.Models.Documents
{
    public class Likes : DocumentBase
    {
        public string PostFor { get; set; }

        [BsonElement("LikedBy")]
        public string LikedByUserId { get; set; }
        
    }
}
