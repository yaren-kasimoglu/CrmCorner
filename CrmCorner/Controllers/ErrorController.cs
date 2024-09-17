using Microsoft.AspNetCore.Mvc;

namespace CrmCorner.Controllers
{
    public class ErrorController : Controller
    {
        // Genel hata sayfası
        public ActionResult Error()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"] ?? "Bilinmeyen bir hata oluştu.";
            return View();
        }

        // 404 - Sayfa Bulunamadı hatası
        public ActionResult NotFound()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"] ?? "Sayfa bulunamadı.";
            return View();
        }

        // 500 - Sunucu hatası
        public ActionResult InternalServerError()
        {
            ViewBag.ErrorMessage = TempData["ErrorMessage"] ?? "Sunucu hatası oluştu.";
            return View();
        }
    }

    public static class ErrorHelper
    {
        public static void HandleError(Controller controller, string errorMessage)
        {
            controller.TempData["ErrorMessage"] = errorMessage;
        }
    }
}
