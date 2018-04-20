using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using URLShortener.Data;
using URLShortener.Models;
using URLShortener.Models.ViewModels;

namespace URLShortener.Controllers
{
    [Authorize(Policy = "RequireAdministratorRole")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Admin/Links/ListLinks
        [Route("[controller]/Links/[action]")]
        public async Task<IActionResult> ListLinks()
        {
            var links = await _context.Urls.Include(url => url.User).OrderByDescending(u => u.UrlId).ToListAsync();

            return View("~/Views/Admin/Links/List.cshtml", links);
        }

        // POST: Admin/Links/DeleteLink
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Links/[action]/{id}")]
        public async Task<IActionResult> DeleteLink(int? id)
        {
            if (id == null)
                return RedirectToAction(nameof(ListLinks));

            var urlToDelete = await _context.Urls.SingleOrDefaultAsync(u => u.UrlId == id);

            if (urlToDelete == null)
                return RedirectToAction(nameof(ListLinks));

            try
            {
                _context.Urls.Remove(urlToDelete);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListLinks));
            }
            catch (DbUpdateException)
            {
                // todo do something if caught exception
                return RedirectToAction(nameof(ListLinks));
            }
        }

        // GET: Admin/Users/ListUsers
        [Route("[controller]/Users/[action]", Name = "ListUsers")] 
        // doesnt seem to bind asp-route in Delete.cshtml
        public async Task<IActionResult> ListUsers()
        {

            var users = await _userManager.Users
                .Include(u => u.Urls)
                .Select(u => new UserListViewModel
                {
                    Id = u.Id,
                    UserName = u.Email,
                    UrlCount = u.Urls.Where(l => l.Id == u.Id).Count()
                }).ToListAsync();

            return View("~/Views/Admin/Users/List.cshtml", users);
        }

        // GET: Admin/Users/DeleteUser/309ujpfm4jmweoiun
        [Route("[controller]/Users/[action]/{id}")]
        public async Task<IActionResult> DeleteUser(string id, bool? saveChangesError = false)
        {
            if (String.IsNullOrEmpty(id))
                RedirectToAction(nameof(ListUsers));

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return RedirectToAction(nameof(ListUsers));

            var userUrls = await _context.Urls.Where(u => u.Id == user.Id).ToListAsync();

            UserDeleteViewModel userDeleteVM = new UserDeleteViewModel
            {
                User = user,
                Urls = userUrls
            };

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }

            return View("~/Views/Admin/Users/Delete.cshtml", userDeleteVM);
        }

        // POST: Admin/Users/DeleteUser/234568984567
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Users/[action]/{id}"), ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (String.IsNullOrEmpty(id))
                return RedirectToAction(nameof(ListUsers));

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return RedirectToAction(nameof(ListUsers));

            var userUrls = await _context.Urls.Where(u => u.Id == id).ToListAsync();

            try
            {
                _context.Users.Remove(user);
                if (userUrls.Count != 0)
                {
                    _context.Urls.RemoveRange(userUrls);
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ListUsers));
            }
            catch (DbUpdateException ex)
            {
                return RedirectToAction(nameof(DeleteUser), new { id = id, saveChangesError = true});
            }
        }
    }
}

