using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace URLShortener.Models.ViewModels
{
    public class UserDeleteViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Url> Urls { get; set; }

    }
}
