using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowManager.Domain.Entities
{
    public class FlowStepItemUser
    {
        public Guid FlowStepItemId { get; set; }
        public Guid UserId { get; set; } 

        // navigation properties
        public virtual FlowStepItem FlowStepItem { get; set; } = null!;
        public virtual User User { get; set; } = null!; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
