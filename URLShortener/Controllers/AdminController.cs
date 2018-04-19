﻿using System;
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
        
        // GET: Admin/Users/List
        [ActionName("List")]
        [Route("[controller]/Users/[action]", Name = "ListUsers")]
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

        // GET: Admin/Users/Delete/309ujpfm4jmweoiun
        [Route("[controller]/Users/[action]", Name = "DeleteUsers")]
        public async Task<IActionResult> Delete(string id, bool? saveChangesError = false)
        {
            if (String.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

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
    }
}
