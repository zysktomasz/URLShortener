using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace URLShortener.Models
{
    public class BlockedDomain
    {
        public int BlockedDomainId { get; set; }
        public string Address { get; set; }
    }
}
