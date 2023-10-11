using DataCommon.Models.Utils;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCommon.Models.Documents
{
    public class SharePost : DocumentBase
    {
        public string PostFor { get; set; }
        
        [BsonElement("SharedBy")]
        public string SharedByUserId { get; set; }
    }
}
