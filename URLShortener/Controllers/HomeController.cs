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
            return View(new UrlViewModel{});
        }

        // POST: /Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async  Task<IActionResult> Index([Bind("TargetUrl,Name")] UrlViewModel urlVM)
        {
            var createdName = urlVM.Name ?? Helper.GenerateRandomUrlName();

            if (await _context.Urls.AnyAsync(u => u.Name == createdName))
            {
                ModelState.AddModelError("", $"{createdName} name is already taken. Try different one.");
                return View();
            }

            var url = new Url { };
            url.Name = createdName;
            url.TargetUrl = urlVM.TargetUrl;
            url.User = await _userManager.GetUserAsync(User);

            try
            {
                if (ModelState.IsValid)
                {
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

            if (url != null && url.TargetUrl != null)
                return Redirect(url.TargetUrl);

            return RedirectToAction(nameof(Index));
        }


    }
}