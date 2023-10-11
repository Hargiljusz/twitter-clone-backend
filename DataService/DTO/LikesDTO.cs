using DataService.DTO.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DTO
{
    public class LikesDTO: BaseDocumentDTO
    {
        public string PostFor { get; set; }
        public string LikedByUserId { get; set; }
        public UserDTO LikedByUser { get; set; }
    }
}
