using DataCommon.Models.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataCommon.Models.Documents
{
    public class Ignored : DocumentBase
    {
        public string UserId { get; set; }
        public DateTime UpdetedAt { get; set; }
        public IEnumerable<IgnoredTag> IgnoredTags { get; set; } = Enumerable.Empty<IgnoredTag>();
        public IEnumerable<IgnoredUser> IgnoredUsers { get; set; } = Enumerable.Empty<IgnoredUser>();
        public IEnumerable<IgnoredPost> IgnoredPosts { get; set; } = Enumerable.Empty<IgnoredPost>();
    }
    public record IgnoredTag(string TagId,string Name,DateTime AddedAt);
    public record IgnoredUser(string UserId,DateTime AddedAt);
    public record IgnoredPost(string PostId,DateTime AddedAt);
}
