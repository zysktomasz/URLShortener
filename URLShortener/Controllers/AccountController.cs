using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using URLShortener.Models;
using URLShortener.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using URLShortener.Data;
using Microsoft.EntityFrameworkCore;


namespace URLShortener.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
                    ApplicationDbContext context,
                    UserManager<ApplicationUser> userManager, 
                    SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }


        // GET: Account/List
        public async Task<IActionResult> List(int? page)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            int pageSize = 6;

            var urls = await PaginatedList<Url>.CreateAsync(
                _context.Urls
                .Where(u => u.User == currentUser)
                .OrderByDescending(u => u.UrlId), page ?? 1, pageSize);

            return View(urls);
        }

        // GET: Account/Edit/420
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var url = await _context.Urls.FirstOrDefaultAsync(u => u.UrlId == id && u.User == currentUser);

            if (url == null)
                return NotFound();

            var urlVM = new UrlEditViewModel
            {
                UrlId = (int)id,
                TargetUrl = url.TargetUrl,
                Name = url.Name
            };


            return View(urlVM);
        }

        // GET: Account/Delete/59
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
                return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var url = await _context.Urls.FirstOrDefaultAsync(u => u.UrlId == id && u.User == currentUser);

            if (url == null)
                return NotFound();

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }

            return View(url);
        }

        // POST: Account/Delete/59
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var url = await _context.Urls.FirstOrDefaultAsync(u => u.UrlId == id && u.User == currentUser);

            if (url == null)
                return RedirectToAction(nameof(List));

            try
            {
                _context.Urls.Remove(url);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(List));
            }
            catch (DbUpdateException /* ex */)
            {
                return RedirectToAction(nameof(Delete), new { id = id, saveChangesError = true });
            }
        }


        // POST: Account/Edit/420
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("UrlId,TargetUrl,Name")] UrlEditViewModel urlEditVM)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var urlToUpdate = await _context.Urls.SingleOrDefaultAsync(u => u.UrlId == urlEditVM.UrlId && u.User == currentUser);
            var createdName = urlEditVM.Name ?? Helper.GenerateRandomUrlName();

            if (await _context.Urls.AnyAsync(u => u.Name == createdName))
            {
                ModelState.AddModelError("Name", $"{createdName} name is already taken. Try different one.");
                return View();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    urlToUpdate.Name = createdName;
                    urlToUpdate.TargetUrl = urlEditVM.TargetUrl;

                    _context.Urls.Update(urlToUpdate);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(List));
                }
                return View(urlEditVM);
                
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Jakis przypal z baza :/");
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        // GET: Account/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
           
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home", new { area = "" });

            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("UserName,Password")] LoginViewModel model)
        {
            var userid = User.Identity.Name;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                        model.UserName, model.Password, isPersistent: false, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                else
                {
                    ModelState.AddModelError("", "Invalid login attempt");
                    return View(model); // redisplays form
                }
            }
            return View(model);
        }

        
        // GET: Account/Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            if(_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home", new { area = "" });

            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Email,Password,ConfirmPassword")] RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
                AddErrors(result);
            }
            return View(model); // if failed, redisplay form
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        // helper method displaying all occured errors
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}