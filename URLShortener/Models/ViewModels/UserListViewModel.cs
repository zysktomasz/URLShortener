using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace URLShortener.Models.ViewModels
{
    public class UserListViewModel
    {
        public string Id { get; set; }
        [Display(Name = "Username")]
        public string UserName { get; set; }
        [Display(Name = "Urls Created")]
        public int UrlCount { get; set; }

    }
}
