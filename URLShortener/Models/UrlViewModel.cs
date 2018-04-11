using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace URLShortener.Models
{
    public class UrlViewModel
    {
        [Required(ErrorMessage = "Your target address field is required.")]
        [Url] // **temporary** url validation, will prolly replace with something custom
        public string TargetUrl { get; set; }
        [StringLength(50, MinimumLength = 1)]
        [RegularExpression(@"^\S+$", ErrorMessage = "No white space allowed")]
        public string CustomName { get; set; }

    }
}
