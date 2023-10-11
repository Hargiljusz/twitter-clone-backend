using DataCommon.Models.Utils;
using DataService.DTO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DTO
{
    public class FollowerDTO : BaseDocumentDTO
    {

        public string From { get; set; }

        public string To { get; set; }
        public FollowerDTO()
        {

        }

        public FollowerDTO(string from, string to, DateTime craetedAt, BackendType createdByBackendType) : base(craetedAt, createdByBackendType)
        {
            From = from;
            To = to;
        }
    }
}
