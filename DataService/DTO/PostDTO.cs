using DataCommon.Models.Utils;
using DataService.DTO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DTO
{
    public class PostDTO: BaseDocumentDTO
    {
        public DateTime UpdatedAt { get; set; }
        public string Content { get; set; }
        public PostMultimediaDTO MultimediaDTO { get; set; }
        public string CreateByUserId { get; set; }
        public UserDTO CreateByUser { get; set; }
        public string PostFor { get; set; }
        public long LikeNumber { get; set; }
        public long ShareNumber { get; set; }
        public IEnumerable<TagItem> Tags { get; set; } = Enumerable.Empty<TagItem>();
        public bool IsLiked { get; set; }
        public bool IsShared { get; set; }
    }

    public class PostMultimediaDTO
    {
        public List<string> Files { get; set; } = new List<string>();
        public string Video { get; set; }
    }
}
