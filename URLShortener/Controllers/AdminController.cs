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
        public async Task<IActionResult> ListLinks(int? page)
        {
            int pageSize = 10;

            var blockedDomains = await _context.BlockedDomains.Select(d => d.Address).ToListAsync();

            var urls = await PaginatedList<UrlViewViewModel>.CreateAsync(
                _context.Urls
                .Include(url => url.User)
                .Select(u => new UrlViewViewModel
                {
                    UrlId = u.UrlId,
                    Name = u.Name,
                    TargetUrl = (u.TargetUrl.Length > 60) ? u.TargetUrl.Substring(0,60) + "..." : u.TargetUrl,
                    Id = u.Id,
                    User = u.User,
                    IsBlocked = (blockedDomains.Contains(Helper.GetUrlDomain(u.TargetUrl)))
                }),
                page ?? 1, pageSize);

            return View("~/Views/Admin/Links/List.cshtml", urls);
        }


        [Route("[controller]/Links/[action]")]
        public async Task<IActionResult> BlockedDomains(int? page)
        {
            int pageSize = 10;

            var blockedDomains = await PaginatedList<BlockedDomain>.CreateAsync(
                _context.BlockedDomains, page ?? 1, pageSize);

            return View("~/Views/Admin/Links/BlockedDomains.cshtml", blockedDomains);
        }

        // POST: Admin/Links/BlockDomain
        // Request from /Admin/Links/ListLinks
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Links/[action]")]
        public async Task<IActionResult> BlockDomain([FromForm(Name = "item.TargetUrl")] string TargetUrl)
        {
            string domain = Helper.GetUrlDomain(TargetUrl);
            BlockedDomain blockedDomain = new BlockedDomain
            {
                Address = domain
            };

            if ((await _context.BlockedDomains.FirstOrDefaultAsync(d => d.Address == domain)) != null)
            {
                TempData["error"] = "This domain is already blocked";
                return RedirectToAction(nameof(ListLinks));
            }

            try
            {
                if(ModelState.IsValid)
                {
                    _context.Add(blockedDomain);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ListLinks));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Jakis przypal z baza :/");
            }

            return RedirectToAction(nameof(ListLinks));
        }

        // POST: Admin/Links/BlockDomain
        // Request from /Admin/Links/BlockedDomains
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Links/[action]")]
        public async Task<IActionResult> BlockDomainBD([FromForm(Name = "DomainToBlock"), Required] string domainToBlock)
        {
            if (String.IsNullOrEmpty(domainToBlock))
                return RedirectToAction(nameof(BlockedDomains));

            try
            {
                if (ModelState.IsValid)
                {
                    if ((await _context.BlockedDomains.FirstOrDefaultAsync(d => d.Address == domainToBlock)) != null)
                    {
                        ModelState.AddModelError("", "This domain is already blocked");
                        return RedirectToAction(nameof(BlockedDomains));
                    }

                    BlockedDomain blockedDomain = new BlockedDomain
                    {
                        Address = domainToBlock
                    };

                    _context.Add(blockedDomain);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(BlockedDomains));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Jakis przypal z baza :/");
            }

            return RedirectToAction(nameof(BlockedDomains));
        }

        // POST: Admin/Links/UnblockDomain
        // Request from /Admin/Links/ListLinks
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Links/[action]")]
        public async Task<IActionResult> UnblockDomain([FromForm(Name = "item.TargetUrl")] string TargetUrl)
        {

            var domainToUnblock = await _context.BlockedDomains.FirstOrDefaultAsync(d => d.Address == Helper.GetUrlDomain(TargetUrl));

            if (domainToUnblock == null)
                return RedirectToAction(nameof(ListLinks));

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Remove(domainToUnblock);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(ListLinks));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Jakis przypal z baza :/");
            }

            return RedirectToAction(nameof(ListLinks));
        }

        // POST: Admin/Links/UnblockDomain
        // Request from /Admin/Links/BlockedDomains
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("[controller]/Links/[action]")]
        public async Task<IActionResult> UnblockDomainBD([FromForm(Name = "item.Address")] string Address)
        {
            var domainToUnblock = await _context.BlockedDomains.FirstOrDefaultAsync(d => d.Address == Address);

            if (domainToUnblock == null)
                return RedirectToAction(nameof(ListLinks));

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Remove(domainToUnblock);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(BlockedDomains));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Jakis przypal z baza :/");
            }

            return RedirectToAction(nameof(BlockedDomains));
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
            catch (DbUpdateException)
            {
                return RedirectToAction(nameof(DeleteUser), new { id = id, saveChangesError = true});
            }
        }
    }
}

