using DataCommon.Models.Utils;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCommon.Models.Documents
{
    public class Follower : DocumentBase
    {
        [BsonElement("_from")]
        public string From { get; set; }

        [BsonElement("_to")]
        public string To { get; set; }
        public Follower()
        {
                
        }

        public Follower(string from, string to,DateTime craetedAt,BackendType createdByBackendType) :base(craetedAt, createdByBackendType)
        {
            From = from;
            To = to;
        }
    }
}
