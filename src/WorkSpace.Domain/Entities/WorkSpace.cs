﻿// src/WorkSpace.Domain/Entities/WorkSpace.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities
{
    public class WorkSpace : AuditableBaseEntity
    {
        [Required]
        [MaxLength(255)]
        public required string Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int HostId { get; set; }

        [Required]
        public int AddressId { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;

        // Navigation properties
        public virtual Address? Address { get; set; }
        public virtual HostProfile? Host { get; set; }
        public virtual List<WorkSpaceRoom> WorkSpaceRooms { get; set; } = new();
        public virtual List<WorkSpaceFavorite> WorkSpaceFavorites { get; set; } = new();
    }
}