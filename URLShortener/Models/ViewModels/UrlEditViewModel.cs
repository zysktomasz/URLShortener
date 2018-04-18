using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace URLShortener.Models.ViewModels
{
    public class UrlEditViewModel
    {
        public int UrlId { get; set; }
        [Required(ErrorMessage = "Your target address field is required.")]
        [Url] // **temporary** url validation, will prolly replace with something custom
        [Display(Name = "Target Url")]
        public string TargetUrl { get; set; }
        [StringLength(50, MinimumLength = 1)]
        [RegularExpression(@"^\S+$", ErrorMessage = "No white space allowed in Custom Name field")]
        [Display(Name = "Custom Name")]
        public string Name { get; set; }
    }
}
