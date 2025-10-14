using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Domain.Common;
 namespace WorkSpace.Domain.Entities
    {
        public class WorkSpaceRoomAmenity : AuditableBaseEntity
        {
            public int WorkSpaceRoomId { get; set; }
            public int AmenityId { get; set; }

            public bool IsAvailable { get; set; } = true;

            // Navigation properties
            public virtual WorkSpaceRoom? WorkSpaceRoom { get; set; }
            public virtual Amenity? Amenity { get; set; }
        }
    }
