using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVCS.DataAccess.Data.Identity
{
    public class EvcsUser : IdentityUser<Guid>
    {
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
