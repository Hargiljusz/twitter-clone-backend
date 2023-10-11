using DataCommon.Models.Utils;

namespace DataCommon.Models.Documents
{
    public class Tag : DocumentBase
    {
        public string Name { get; set; }

        public Tag()
        {

        }

        public Tag(string name, DateTime createdAt, BackendType createdByBackendType) :base(createdAt,createdByBackendType)
        {
            Name = name;
        }
    }
}
