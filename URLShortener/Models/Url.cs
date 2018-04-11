using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace URLShortener.Models
{
    public class Url
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string TargetUrl { get; set; }
    }
}
