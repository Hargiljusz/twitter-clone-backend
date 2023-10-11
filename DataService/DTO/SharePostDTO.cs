using DataCommon.Models.Utils;
using DataService.DTO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DTO
{
    public class SharePostDTO : BaseDocumentDTO
    {
        public string PostFor { get; set; }
        public string SharedByUserId { get; set; }
        public UserDTO SharedByUser { get; set; }
        public SharePostDTO()
        {
                
        }

        public SharePostDTO(string postFor,string sharedByUserId,DateTime createdAt, BackendType createdByBackendType) : base(createdAt, createdByBackendType)
        {
            PostFor = postFor;
            SharedByUserId = sharedByUserId;
        }
    }
}
