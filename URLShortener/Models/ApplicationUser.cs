using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace URLShortener.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual List<Url> Urls { get; set; }
    }
}
