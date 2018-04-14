using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace URLShortener.Models
{
    public class Url
    {
        [Key]
        public int UrlId { get; set; }
        public string Name { get; set; }
        [Display(Name = "Target Address")]
        public string TargetUrl { get; set; }

        public string Id { get; set; } // ApplicationUser.Id
        [ForeignKey("Id")]
        public virtual ApplicationUser User { get; set; }
    }
}
