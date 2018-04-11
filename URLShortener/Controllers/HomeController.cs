using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShortener.Data;
using URLShortener.Models;

namespace URLShortener.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Index
        public IActionResult Index()
        {
            return View(new UrlViewModel{});
        }

        // POST: /Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async  Task<IActionResult> Index([Bind("TargetUrl,CustomName")] UrlViewModel urlVM)
        {
            var url = new Url { };

            var createdName = urlVM.CustomName ?? GenerateRandomUrlName();

            url.Name = createdName;
            url.TargetUrl = urlVM.TargetUrl;
            
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
            var url = await _context.Urls.SingleOrDefaultAsync(u => u.Name == urlName);

            if (url != null && url.TargetUrl != null)
                return Redirect(url.TargetUrl);

            return RedirectToAction(nameof(Index));
        }

        private string GenerateRandomUrlName()
        {
            // brak kontroli nad zawartoscia generowanego ciagu
            // return Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 10);

            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[8];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

    }
}