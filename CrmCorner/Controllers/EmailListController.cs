using System;
using CrmCorner.Extensions;
using CrmCorner.Models;
using CrmCorner.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class EmailListController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;
        public EmailListController(CrmCornerContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public async Task<IActionResult> EmailList(List<EmailList>? emailsLists, string? searchTerm)
		{
            ViewBag.CheckedSend = false;
            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
            try
            {

                //Gönderilen kutusu
                if (emailsLists != null && emailsLists.Any(m => m.IsSend == true))
                {
                    var emailsListGet = _context.EmailList
                           .Where(e => e.To == currentUser.Email)
                           .ToList();
                    ViewBag.CheckedSend = true;
                    ViewBag.SendCount = emailsLists.Count();
                    ViewBag.GetCount = emailsListGet.Count();
                    return View(emailsLists);

                }
                //Gelen kutusu
                else
                {
                    List<EmailList> emailsList;
                    if (currentUser == null)
                    {
                        ErrorHelper.HandleError(this, "Geçerli kullanıcı bilgisi bulunamadı.");
                        return RedirectToAction("NotFound", "Error");
                    }
                    if (currentUser != null)
                    {
                        emailsList = _context.EmailList
                            .Where(e => e.To == currentUser.Email)
                            .ToList();
                        ViewBag.GetCount = emailsList.Count();
                        if (!string.IsNullOrEmpty(searchTerm) && emailsList!=null)
                        {
                            emailsList = _context.EmailList
                            .Where(e => e.Subject.Contains(searchTerm) &&e.To == currentUser.Email)
                            .ToList(); ;
                        }
                        var emailsListSend = _context.EmailList
                       .Where(e => e.From == currentUser.Email)
                       .ToList();
                        ViewBag.SendCount = emailsListSend.Count();

                        return View(emailsList);
                    }
                }

            }
            catch (NullReferenceException ex)
            {
                ErrorHelper.HandleError(this, "Veri bulunamadı: " + ex.Message);
                return RedirectToAction("NotFound", "Error");
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorHelper.HandleError(this, "Yetkisiz erişim: " + ex.Message);
                return RedirectToAction("Unauthorized", "Error");
            }
            catch (Exception ex)
            {
                ErrorHelper.HandleError(this, "Bir hata oluştu: " + ex.Message);
                return RedirectToAction("Error", "Error"); // Genel hata sayfası
            }
            return View("EmailList");
        }
        public async Task<IActionResult> GetEmailDetails(int id)
        {
            var email = _context.EmailList.FirstOrDefault(e => e.Id == id);
            if (email == null)
            {
                return NotFound();
            }
            var jsonData = new
            {
                From = email.From,
                CreatedDate = email.CreatedDate,
                Body = email.Body,
                Cc = email.CC,
                To = email.To,
            };
            return Json(new { Message = jsonData });
        }
        [HttpGet]
        public async Task<IActionResult> EmailListAdd()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

                if (currentUser == null)
                {
                    return RedirectToAction("SignIn", "Home"); // Kullanıcı giriş yapmamışsa giriş sayfasına yönlendir
                }

                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> EmailListAdd(EmailList emailList)
        {
            try
            {
                    var currentUser = await _userManager.GetUserAsync(User);
                    emailList.CreatedDate = DateTime.Now;
                    emailList.IsStar = false;
                    emailList.SendMail = currentUser.Email;
                    emailList.From = currentUser.Email;
                    emailList.AppUserId = currentUser.Id;
                    _context.EmailList.Add(emailList);
                    _context.SaveChanges();
                    return RedirectToAction("EmailList");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Bir hata oluştu: " + ex.Message);
            }

            return RedirectToAction("EmailList");// ModelState geçersiz olduğu için form yeniden yüklenir
        }
        [HttpPost]
        public IActionResult EmailListDelete(int id)
        {
            try
            {
                EmailList emaillist = _context.EmailList.Find(id);

                if (emaillist == null)
                {
                    return NotFound();
                }
                _context.EmailList.Remove(emaillist);
                _context.SaveChanges();

                return RedirectToAction("EmailList");
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }
        [HttpGet]
        public async Task<IActionResult> EmailListSend(string? searchTerm)
        {
            ViewBag.CheckedSend = true;
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                {
                    ErrorHelper.HandleError(this, "Geçerli kullanıcı bilgisi bulunamadı.");
                    return RedirectToAction("NotFound", "Error");
                }

                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");
                List<EmailList> emailsList;

                if (currentUser != null)
                {
                    emailsList = _context.EmailList
                    .Where(e => e.From == currentUser.Email)
                    .ToList();
                    ViewBag.SendCount = emailsList.Count();
                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        emailsList = _context.EmailList
                        .Where(e => e.Subject.Contains(searchTerm) && e.From == currentUser.Email)
                        .ToList(); ;
                    }
                    foreach (var email in emailsList)
                    {
                        email.IsSend = true;
                        break;
                    }
                    var emailsListGet = _context.EmailList
                        .Where(e => e.To == currentUser.Email)
                        .ToList();

                    ViewBag.GetCount = emailsListGet.Count();
                    return View("EmailList", emailsList);
                }
            }
            catch (NullReferenceException ex)
            {
                ErrorHelper.HandleError(this, "Veri bulunamadı: " + ex.Message);
                return RedirectToAction("NotFound", "Error");
            }
            catch (UnauthorizedAccessException ex)
            {
                ErrorHelper.HandleError(this, "Yetkisiz erişim: " + ex.Message);
                return RedirectToAction("Unauthorized", "Error");
            }
            catch (Exception ex)
            {
                ErrorHelper.HandleError(this, "Bir hata oluştu: " + ex.Message);
                return RedirectToAction("Error", "Error"); // Genel hata sayfası
            }
            return View("EmailList");
        }
    }
}

