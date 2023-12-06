using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    public class CustomerController : Controller
    {
        private readonly CrmcornerContext _context;
        public CustomerController(CrmcornerContext context)
        {
            _context = context;
        }
        public IActionResult CustomerList()
        {
            var customers = _context.Customers.Include(e => e.IdEmployeeNavigation)
                                .ToList();
            return View(customers);
        }

        [HttpGet]
        public IActionResult CustomerAdd()
        {
            var employees = _context.Employees.ToList();

            List<SelectListItem> employeeItems = employees
          .Select(d => new SelectListItem
          {
              Text = d.EmployeeName,
              Value = d.IdEmployee.ToString()
          }).ToList();

            ViewBag.StatusList = GetStatusList(); // StatusList, dropdown'ı dolduracak veri
            ViewBag.Employees = employeeItems;
            return View();
        }
        [HttpPost]
        public IActionResult CustomerAdd(Customer customer)
        {
            if (ModelState.IsValid)
            {
                var customers = _context.Customers.Include(e => e.IdEmployeeNavigation).ToList();

                _context.Customers.Add(customer);
                _context.SaveChanges();

                return RedirectToAction("CustomerList");
            }
            ViewBag.StatusList = GetStatusList();
            return View(customer);
        }


        [HttpGet]
        public IActionResult CustomerEdit(int id)
        {
            var employees = _context.Employees.ToList();
            List<SelectListItem> employeeItems = employees
.Select(d => new SelectListItem
{
    Text = d.EmployeeName,
    Value = d.IdEmployee.ToString()
}).ToList();

            ViewBag.Employees = employeeItems;
            // id parametresini kullanarak düzenlenecek müşteriyi veritabanından al
            Customer customer = _context.Customers.Find(id);

            // Eğer müşteri bulunamazsa
            if (customer == null)
            {
                return NotFound(); // 404 Not Found dönülebilir
            }
            // StatusList'i doldur
            ViewBag.StatusList = GetStatusList();

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
