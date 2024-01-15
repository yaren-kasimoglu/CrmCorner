using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
    public class CompanyController : Controller
    {
        private readonly CrmCornerContext _context;
        public CompanyController(CrmCornerContext context)
        {
            _context = context;
        }
        public IActionResult CompanyList()
        {
            var Companys = _context.Companies
                .Include(e => e.IdEmployeeNavigation)
                .Include(e => e.Status)
                                .ToList();
            return View(Companys);
        }

        [HttpGet]
        public IActionResult CompanyAdd()
        {
            var employees = _context.Employees.ToList();
            var status = _context.Statuses.ToList();

            List<SelectListItem> employeeItems = employees
          .Select(d => new SelectListItem
          {
              Text = d.EmployeeName,
              Value = d.IdEmployee.ToString()
          }).ToList();

            List<SelectListItem> statusItems = status
    .Select(d => new SelectListItem
    {
        Text = d.StatusName,
        Value = d.StatusId.ToString()
    }).ToList();


            ViewBag.Employees = employeeItems;
            ViewBag.Status = statusItems;
            return View();
        }
        [HttpPost]
        public IActionResult CompanyAdd(Company Company)
        {
            if (ModelState.IsValid)
            {
                var Companys = _context.Companies.Include(e => e.IdEmployeeNavigation).ToList();

                _context.Companies.Add(Company);
                _context.SaveChanges();

                return RedirectToAction("CompanyList");
            }
            return View(Company);
        }


        [HttpGet]
        public IActionResult CompanyEdit(int id)
        {
            var employees = _context.Employees.ToList();
            var status = _context.Statuses.ToList();

            List<SelectListItem> employeeItems = employees
          .Select(d => new SelectListItem
          {
              Text = d.EmployeeName,
              Value = d.IdEmployee.ToString()
          }).ToList();

            List<SelectListItem> statusItems = status
    .Select(d => new SelectListItem
    {
        Text = d.StatusName,
        Value = d.StatusId.ToString()
    }).ToList();
            ViewBag.Employees = employeeItems;
            ViewBag.Status = statusItems;

            // id parametresini kullanarak düzenlenecek müşteriyi veritabanından al
            Company company = _context.Companies.Find(id);

            // Eğer müşteri bulunamazsa
            if (company == null)
            {
                return NotFound(); // 404 Not Found dönülebilir
            }

            // Müşteriyi düzenleme sayfasına gönder
            return View(company);
        }

        [HttpPost]
        public IActionResult CompanyEdit(Company editedCompany)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(editedCompany).State = EntityState.Modified;
                _context.SaveChanges();

                return RedirectToAction("CompanyList");
            }
            //ModelState.IsValid false ise
            return View(editedCompany);
        }
        [HttpPost]
        public IActionResult CompanyDelete(int id)
        {
            Company company = _context.Companies.Find(id);

            // Eğer müşteri bulunamazsa
            if (company == null)
            {
                return NotFound();
            }
            _context.Companies.Remove(company);
            _context.SaveChanges();

            return RedirectToAction("CompanyList");
        }
    }
}
