using DataCommon.Models.Utils;
using MongoDB.Bson.Serialization.Attributes;

namespace DataCommon.Models.Documents
{

    public class Post : DocumentBase
    {
        public DateTime UpdatedAt { get; set; }
        public string Content { get; set; }
        public PostMultimedia Multimedia { get; set; }
        public string CreateByUserId { get; set; }
        public string PostFor { get; set; } = String.Empty;
        public long LikeNumber { get; set; }
        public long ShareNumber { get; set; }
        public List<TagItem> Tags { get; set; }

    }

    public class PostMultimedia
    {
        public List<string> Files { get; set; } = new List<string>();
        public string Video { get; set; }
    }
}
