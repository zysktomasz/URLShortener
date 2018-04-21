using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace URLShortener.Models.ViewModels
{
    public class UrlViewViewModel : Url
    {
        public bool IsBlocked { get; set; }

    }
}
