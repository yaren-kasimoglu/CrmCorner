
using CrmCorner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly CrmCornerContext _context;
        public CustomerController(CrmCornerContext context)
        {
            _context = context;
        }
        public IActionResult CustomerList()
        {
            var customers = _context.Customers.Include(e => e.Company)
                                .ToList();
            return View(customers);
        }

        [HttpGet]
        public IActionResult CustomerAdd()
        {
            var company = _context.Companies.ToList();

            List<SelectListItem> companyItems = company
          .Select(d => new SelectListItem
          {
              Text = d.CompanyName,
              Value = d.Id.ToString()
          }).ToList();

            ViewBag.CompanyList = companyItems;
            return View();
        }
        [HttpPost]
        public IActionResult CustomerAdd(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var customers = _context.Customers.Include(e => e.Company).ToList();

                _context.Customers.Add(customer);
                _context.SaveChanges();

                return RedirectToAction("CustomerList");
            }

            var company = _context.Companies.ToList();

            List<SelectListItem> companyItems = company
          .Select(d => new SelectListItem
          {
              Text = d.CompanyName,
              Value = d.Id.ToString()
          }).ToList();

            ViewBag.CompanyList = companyItems;

            return View(customer);
        }


        [HttpGet]
        public IActionResult CustomerEdit(int id)
        {
            var company = _context.Companies.ToList();

            List<SelectListItem> companyItems = company
          .Select(d => new SelectListItem
          {
              Text = d.CompanyName,
              Value = d.Id.ToString()
          }).ToList();

            ViewBag.CompanyList = companyItems;
            // id parametresini kullanarak düzenlenecek müşteriyi veritabanından al
            Customer customer = _context.Customers.Find(id);

            // Eğer müşteri bulunamazsa
            if (customer == null)
            {
                return NotFound(); // 404 Not Found dönülebilir
            }

            // Müşteriyi düzenleme sayfasına gönder
            return View(customer);
        }

        [HttpPost]
        public IActionResult CustomerEdit(Customer editedCustomer)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(editedCustomer).State = EntityState.Modified;
                _context.SaveChanges();

                return RedirectToAction("CustomerList");
            }
            //ModelState.IsValid false ise
            return View(editedCustomer);
        }
        [HttpPost]
        public IActionResult CustomerDelete(int id)
        {
            Customer customer = _context.Customers.Find(id);

            // Eğer müşteri bulunamazsa
            if (customer == null)
            {
                return NotFound();
            }
            _context.Customers.Remove(customer);
            _context.SaveChanges();

            return RedirectToAction("CustomerList");
        }

        private List<string> GetStatusList()
        {
            // Dropdown'ı dolduracak veriler burada alınır
            return new List<string> { "Active", "Inactive", "Pending" };
        }
    }
}
