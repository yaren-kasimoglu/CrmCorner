//using CrmCorner.Models;
//using CrmCorner.Models.PasswordHash;
//using Microsoft.AspNetCore.Authentication.Cookies;
//using Microsoft.AspNetCore.Authentication;
//using Microsoft.AspNetCore.Mvc;
//using NuGet.Configuration;
//using System.Security.Claims;

//namespace CrmCorner.Controllers
//{
//    public class LoginController : Controller
//    {
//        private readonly CrmCornerContext _context;
//        public LoginController(CrmCornerContext context)
//        {
//            _context = context;
//        }
//        public IActionResult Index()
//        {
//            return View();
//        }

//        [HttpGet]
//        public IActionResult Register(bool password)
//        {
//            if (password)
//            {
//                ViewBag.Message = "Şifreler Eşleşmiyor";
//            }
//            return View();
//        }

//        [HttpPost]
//        public IActionResult Register(IFormFile UserImage, User userCard)
//        {
//            var fileName = String.Empty;

//            string imagePath = null;
//            if (UserImage != null)
//            {
//                using (var ms = new MemoryStream())
//                {
//                    fileName = Path.GetFileNameWithoutExtension(UserImage.FileName);
//                    var path = Path.Combine(Directory.GetCurrentDirectory(), "www.root/images/", fileName);
//                    UserImage.CopyTo(ms);
//                    var fileBytes = ms.ToArray();
//                    string imageInfo = Convert.ToBase64String(fileBytes);

//                    imagePath = imageInfo;
//                    ViewBag.imageInfo = imageInfo;
//                }
//            }


//            userCard.Username = userCard.Username;
//            userCard.Email = userCard.Email;
//            userCard.FirstName = userCard.FirstName;
//            userCard.LastName = userCard.LastName;
//            userCard.PhoneNumber = userCard.PhoneNumber;
//            DateOnly dob;
//            if (DateOnly.TryParse(Request.Form["DateOfBirth"], out dob))
//            {
//                userCard.DateOfBirth = dob;
//            }
//            userCard.CreatedAt = DateTime.Now;

//            var password = false;


//            if (HttpContext.Request.Form["Password1"] == HttpContext.Request.Form["Password2"])
//            {
//                var value = SecureKey.getKey();

//                userCard.PasswordSalt = CryptographyManager.Encrypt(value, value);
//                userCard.PasswordHash = CryptographyManager.Encrypt(HttpContext.Request.Form["Password1"], value);


//                userCard.UserImage = imagePath;



//                //  userCard.UserAdminCode = HttpContext.Request.Form["UserAdminCode"]; //role ataması

//                //  userCard.Manager = HttpContext.Request.Form["Manager"];//bağlı olduğu yönetici



//                _context.User.Add(userCard);
//                _context.SaveChanges();
//            }
//            else
//            {
//                password = true;

//                return RedirectToAction("Register", new { password = password });
//            }


//            return RedirectToAction("Login");
//        }

//        [HttpGet]
//        public IActionResult Login()
//        {
//            return View();
//        }
//        public IActionResult Login(User userCard)
//        {
//            var username = userCard.Username;
//            var password = HttpContext.Request.Form["Password"];
//            var user = _context.User.FirstOrDefault(u => u.Username == username);

//            if (user != null && VerifyPassword(user, password))
//            {
//                return RedirectToAction("WelcomePage", "Home");
//            }
//            ViewBag.ErrorMessage = "Invalid username or password";
//            return View();

//        }

//        private bool VerifyPassword(User user, string password)
//        {
//            var value = SecureKey.getKey();
//            var encryptedPassword = CryptographyManager.Encrypt(password, value);

//            return encryptedPassword == user.PasswordHash;
//        }
//    }
//}
