using Microsoft.AspNetCore.Mvc;

namespace CrmCorner.Controllers
{
    public class ErrorController : Controller
    {
        // Genel hata sayfası
        public ActionResult Error()
        {
            return View();
        }

        // 404 - Sayfa Bulunamadı hatası //KULLANILAN METHOD BUDUR
        public ActionResult NotFound()
        {
            return View();
        }

        // 500 - Sunucu hatası
        public ActionResult InternalServerError()
        {
            return View();
        }
    }
}
