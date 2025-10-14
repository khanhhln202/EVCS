using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.Models.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public bool IsActive { get; set; } = true; // mapping logic app side
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
