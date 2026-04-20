using Iyzipay;
using Iyzipay.Model;
using Iyzipay.Request;
using CrmCorner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CrmCorner.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IyzicoSettings _iyzicoSettings;

        public PaymentController(IOptions<IyzicoSettings> iyzicoOptions)
        {
            _iyzicoSettings = iyzicoOptions.Value;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> StartCheckout(CheckoutStartRequest model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Checkout", new
                {
                    planName = model.PlanName,
                    userCount = model.UserCount
                });
            }

            var totalPrice = CalculatePrice(model.PlanName, model.UserCount);

            var options = new Iyzipay.Options
            {
                ApiKey = _iyzicoSettings.ApiKey,
                SecretKey = _iyzicoSettings.SecretKey,
                BaseUrl = _iyzicoSettings.BaseUrl
            };

            var request = new CreateCheckoutFormInitializeRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = Guid.NewGuid().ToString("N"),
                Price = totalPrice.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                PaidPrice = totalPrice.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
                Currency = Currency.TRY.ToString(),
                BasketId = $"BASKET-{Guid.NewGuid():N}",
                PaymentGroup = PaymentGroup.PRODUCT.ToString(),

                // localhost callback bazen sandbox testlerinde sorun çıkarabilir.
                // Mümkünse ngrok/https bir adres kullan.
                CallbackUrl = Url.Action(
                    "Callback",
                    "Payment",
                    null,
                    Request.Scheme)
            };

            request.Buyer = new Buyer
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = GetFirstName(model.FullName),
                Surname = GetLastName(model.FullName),
                GsmNumber = NormalizePhone(model.Phone),
                Email = model.Email,
                IdentityNumber = "11111111111",
                LastLoginDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                RegistrationDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                RegistrationAddress = model.Address,
                Ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
                City = model.City,
                Country = model.Country,
                ZipCode = "41000"
            };

            request.BillingAddress = new Address
            {
                ContactName = model.FullName,
                City = model.City,
                Country = model.Country,
                Description = model.Address,
                ZipCode = "41000"
            };

            // SaaS sattığın için VIRTUAL uygun
            request.BasketItems = new List<BasketItem>
    {
        new BasketItem
        {
            Id = $"ITEM-{Guid.NewGuid():N}",
            Name = $"{model.PlanName} Paketi",
            Category1 = "SaaS",
            Category2 = "CRM",
            ItemType = BasketItemType.VIRTUAL.ToString(),
            Price = totalPrice.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
        }
    };

            var checkoutFormInitialize = await CheckoutFormInitialize.Create(request, options);

            if (checkoutFormInitialize.Status != Iyzipay.Model.Status.SUCCESS.ToString() ||
      string.IsNullOrWhiteSpace(checkoutFormInitialize.PaymentPageUrl))
            {
                TempData["PaymentError"] = checkoutFormInitialize.ErrorMessage ?? "Ödeme formu başlatılamadı.";
                return RedirectToAction("Failure");
            }

            // paymentPageUrl ile yönlendirme
            return Redirect(checkoutFormInitialize.PaymentPageUrl);
        }



        private string GetFirstName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "Ad";

            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return parts.FirstOrDefault() ?? "Ad";
        }

        private string GetLastName(string fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return "Soyad";

            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
                return "Soyad";

            return string.Join(" ", parts.Skip(1));
        }

        private string NormalizePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return "+905000000000";

            var digits = new string(phone.Where(char.IsDigit).ToArray());

            if (digits.StartsWith("90"))
                return "+" + digits;

            if (digits.StartsWith("0"))
                return "+9" + digits;

            return "+90" + digits;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Callback(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return RedirectToAction("Failure");
            }

            var options = new Iyzipay.Options
            {
                ApiKey = _iyzicoSettings.ApiKey,
                SecretKey = _iyzicoSettings.SecretKey,
                BaseUrl = _iyzicoSettings.BaseUrl
            };

            var request = new RetrieveCheckoutFormRequest
            {
                Locale = Locale.TR.ToString(),
                ConversationId = Guid.NewGuid().ToString("N"),
                Token = token
            };

            var result = await CheckoutForm.Retrieve(request, options);

            if (result.Status == Iyzipay.Model.Status.SUCCESS.ToString() &&
                result.PaymentStatus == "SUCCESS")
            {
                return RedirectToAction("Success");
            }

            TempData["PaymentError"] = result.ErrorMessage ?? "Ödeme doğrulanamadı.";
            return RedirectToAction("Failure");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Success()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Failure()
        {
            return View();
        }

        private decimal CalculatePrice(string planName, int userCount)
        {
            switch (planName)
            {
                case "Gold":
                    return 1000 * userCount;
                case "Platinum":
                    return 5000 * userCount;
                default:
                    return 0;
            }
        }



        [HttpGet]
        [AllowAnonymous]
        public IActionResult Checkout(string planName, int userCount)
        {
            var totalPrice = CalculatePrice(planName, userCount);

            ViewBag.PlanName = planName;
            ViewBag.UserCount = userCount;
            ViewBag.TotalPrice = totalPrice;

            return View();
        }


    }
}