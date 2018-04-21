using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShortener.Data;
using URLShortener.Models;
using URLShortener.Models.ViewModels;

namespace URLShortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Index
        public IActionResult Index()
        {
            return View(new UrlCreateViewModel{});
        }

        // POST: /Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async  Task<IActionResult> Index([Bind("TargetUrl,Name")] UrlCreateViewModel urlVM)
        {
            var createdName = urlVM.Name ?? Helper.GenerateRandomUrlName();

            if (await _context.Urls.AnyAsync(u => u.Name == createdName))
            {
                ModelState.AddModelError("", $"{createdName} name is already taken. Try different one.");
                return View();
            }

            
            Url url = new Url
            {
                Name = createdName,
                TargetUrl = urlVM.TargetUrl,
                User = await _userManager.GetUserAsync(User)
            };

            try
            {
                if (ModelState.IsValid)
                {
                    // domain blocked validation placed here, cause urlVM.TargetUrl can't be null
                    // in order to use Helper.GetUrlDomain (ModelState.IsValid has to be true)
                    string domain = Helper.GetUrlDomain(urlVM.TargetUrl);
                    if ((await _context.BlockedDomains.FirstOrDefaultAsync(d => d.Address == domain)) != null)
                    {
                        ModelState.AddModelError("", $"{domain} domain has been BLOCKED.");
                        return View();
                    }

                    _context.Add(url);
                    await _context.SaveChangesAsync();
                    TempData["shortenedUrl"] = createdName;

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Jakis przypal z baza :/");
            }

            return View();
        }


        // GET: /RedirectToAction
        public async Task<IActionResult> RedirectToTarget(string urlName)
        {
            var url = await _context.Urls.FirstOrDefaultAsync(u => u.Name == urlName);

            if (url == null)
                return RedirectToAction(nameof(Index));

            string domain = Helper.GetUrlDomain(url.TargetUrl);
            if ((await _context.BlockedDomains.FirstOrDefaultAsync(d => d.Address == domain)) != null)
            {
                TempData["Error"] = $"Domain ({domain}) you were being redirected to has been BLOCKED.";
                return View(nameof(Index));
            }

            return Redirect(url.TargetUrl);

        }


    }
}