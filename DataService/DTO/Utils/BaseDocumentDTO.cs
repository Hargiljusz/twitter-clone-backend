using DataCommon.Models.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.DTO.Utils
{
    public class BaseDocumentDTO
    {
        public string Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public BackendType CreatedByBackendType { get; set; }
        public BackendType UpdatedByBackendType { get; set; }

        public BaseDocumentDTO()
        {

        }

        public BaseDocumentDTO(DateTime createdAt, BackendType createdByBackendType, BackendType updatedByBackendType)
        {
            CreatedAt = createdAt;
            CreatedByBackendType = createdByBackendType;
            UpdatedByBackendType = updatedByBackendType;
        }

        protected BaseDocumentDTO(DateTime createdAt, BackendType createdByBackendType)
        {
            CreatedAt = createdAt;
            CreatedByBackendType = createdByBackendType;
        }

        protected BaseDocumentDTO(string id, DateTime createdAt, BackendType createdByBackendType, BackendType updatedByBackendType)
        {
            Id = id;
            CreatedAt = createdAt;
            CreatedByBackendType = createdByBackendType;
            UpdatedByBackendType = updatedByBackendType;
        }
    }
}
