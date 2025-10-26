using CrmCorner.Extensions;
using CrmCorner.Models;
using CrmCorner.Models.Enums;
using CrmCorner.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace CrmCorner.Controllers
{

    [Authorize]
    public class MemberController : Controller //Sadece kullanıcı olanların görebileceği bir sayfadır
    {
        private readonly CrmCornerContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IFileProvider _fileProvider;
        private readonly IWebHostEnvironment _environment;

        public MemberController(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IFileProvider fileProvider, IWebHostEnvironment environment, CrmCornerContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _fileProvider = fileProvider;
            _environment = environment;
            _context = context;
        }


        public async Task<IActionResult> MyProfile()
        {
            try
            {
                var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);
                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

                var userViewModel = new UserViewModel
                {
                    Email = currentUser!.Email,
                    UserName = currentUser!.UserName,
                    PhoneNumber = currentUser!.PhoneNumber,
                    PictureUrl = currentUser.Picture
                };

                return View(userViewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> PasswordChange()
        {
            var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);
            ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

            return View();

        }

        [HttpPost]
        public async Task<IActionResult> PasswordChange(PasswordChangeViewModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                var currentUser = await _userManager.FindByNameAsync(User.Identity!.Name!);
                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

                var checkOldPass = await _userManager.CheckPasswordAsync(currentUser!, request.PasswordOld);

                if (!checkOldPass)
                {
                    ModelState.AddModelError(string.Empty, "Eski şifreniz yanlış.");
                    return RedirectToAction("PasswordChange");
                }

                var resultChangePassword = await _userManager.ChangePasswordAsync(currentUser, request.PasswordOld, request.PasswordNew);

                if (!resultChangePassword.Succeeded)
                {
                    TempData["ErrorMessage"] = "Şifre değiştirme işlemi başarısız oldu.";
                    return RedirectToAction("PasswordChange");
                }

                await _userManager.UpdateSecurityStampAsync(currentUser);
                await _signInManager.SignOutAsync();
                await _signInManager.PasswordSignInAsync(currentUser, request.PasswordNew, true, false);

                TempData["SuccessMessage"] = "Şifre başarıyla değiştirildi.";
                return RedirectToAction("PasswordChange");

            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        public async Task<IActionResult> UserEdit()
        {
            try
            {
                ViewBag.GenderList = new SelectList(Enum.GetNames(typeof(Gender)));
                var currentUser = (await _userManager.FindByNameAsync(User.Identity!.Name!))!;
                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

                var userEditViewModel = new UserEditViewModel()
                {
                    UserName = currentUser.UserName!,
                    Email = currentUser.Email!,
                    Phone = currentUser.PhoneNumber!,
                    BirthDate = currentUser.BirthDate,
                    City = currentUser.City,
                    Gender = currentUser.Gender,
                    EmployeeCount = currentUser.EmployeeCount,
                    Sector = currentUser.Sector,
                };
                return View(userEditViewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UserEdit(UserEditViewModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(request);
                }

                var currentUser = await _userManager.FindByNameAsync(User.Identity.Name);
                ViewBag.PictureUrl = "/userprofilepicture/" + (currentUser.Picture ?? "defaultpp.png");

                if (currentUser == null)
                {
                    ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                    return View(request);
                }

                // Kullanıcı bilgilerini güncelle
                currentUser.UserName = request.UserName;
                currentUser.Email = request.Email;
                currentUser.BirthDate = request.BirthDate;
                currentUser.City = request.City;
                currentUser.Gender = request.Gender;
                currentUser.PhoneNumber = request.Phone;
                currentUser.Sector = request.Sector;
                currentUser.EmployeeCount = request.EmployeeCount;

                // Profil resmi için yol oluşturma ve kaydetme bloğu
                if (request.Picture != null && request.Picture.Length > 0)
                {
                    try
                    {
                        // 'wwwroot' klasörünün yolunu alın
                        var wwwrootPath = _environment.WebRootPath;
                        var userprofilepicturePath = Path.Combine(wwwrootPath, "userprofilepicture");

                        // Directory.Exists metodu ile klasörün var olup olmadığını kontrol et
                        if (!Directory.Exists(userprofilepicturePath))
                        {
                            // Klasör yoksa, Directory.CreateDirectory metodu ile klasörü oluştur
                            Directory.CreateDirectory(userprofilepicturePath);
                        }

                        // Sadece belirli uzantılara izin ver
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                        var fileExt = Path.GetExtension(request.Picture.FileName).ToLower();

                        if (!allowedExtensions.Contains(fileExt))
                        {
                            TempData["ErrorMessage"] = "Yalnızca .jpg, .jpeg, .png veya .gif formatındaki dosyaları yükleyebilirsiniz.";
                            return RedirectToAction("UserEdit");
                        }

                        var randomFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.Picture.FileName)}";
                        var newPicturePath = Path.Combine(userprofilepicturePath, randomFileName);

                        using (var stream = new FileStream(newPicturePath, FileMode.Create))
                        {
                            await request.Picture.CopyToAsync(stream);
                        }

                        currentUser.Picture = randomFileName;
                    }
                    catch (Exception ex)
                    {
                        return RedirectToAction("NotFound", "Error");
                    }
                }

                var profilePicturePath = currentUser?.Picture ?? "/userprofilepicture/defaultpp.png";
                ViewBag.UserProfilePicture = profilePicturePath;

                // Kullanıcıyı güncelle
                var updateToUserResult = await _userManager.UpdateAsync(currentUser);
                if (!updateToUserResult.Succeeded)
                {
                    // Hata mesajlarını ModelState'e ekle
                    foreach (var error in updateToUserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(request);
                }

                // Güvenlik damgasını güncelle ve kullanıcıyı yeniden giriş yaptır
                await _userManager.UpdateSecurityStampAsync(currentUser);
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(currentUser, isPersistent: true);

                // Başarı mesajını TempData'ya ekle
                TempData["SuccessMessage"] = "Bilgiler başarıyla güncellendi.";

                // Güncellenmiş kullanıcı bilgileri ile ViewModel'i doldur
                var userEditViewModel = new UserEditViewModel
                {
                    UserName = currentUser.UserName,
                    Email = currentUser.Email,
                    Phone = currentUser.PhoneNumber,
                    BirthDate = currentUser.BirthDate,
                    City = currentUser.City,
                    Gender = currentUser.Gender,
                    Sector = request.Sector,
                    EmployeeCount = request.EmployeeCount,
                };

                return RedirectToAction("UserEdit");
            }
            catch (Exception ex)
            {
                return RedirectToAction("NotFound", "Error");
            }
        }

        public  IActionResult AccessDenied(string ReturnUrl)
        {
            string message=string.Empty;
            message = "Bu sayfayı görmeye yetkiniz yoktur. Yetki almak için yöneticiniz ile görüşebilirsiniz.";

            ViewBag.message = message;
            return View();
        }

    }

}
