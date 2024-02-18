using CrmCorner.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CrmCorner.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        private readonly CrmCornerContext _context;
        public EmployeeController(CrmCornerContext context)
        {
            _context = context;
        }
        #region EMPLOYEE
        public IActionResult EmployeeList()
        {
            var employees = _context.Employees
                                .Include(e => e.IdDepartmentNavigation)
                                .Include(e => e.IdPositionsNavigation)
                                .ToList();
            return View(employees);
        }

        [HttpGet]
        public IActionResult EmployeeAdd()
        {
            // Veritabanından tüm departmanları al
            var departments = _context.Departments.ToList();
            var positions = _context.Positions.ToList();

            List<SelectListItem> departmentItems = departments
            .Select(d => new SelectListItem
            {
                Text = d.DepartmentName,
                Value = d.IdDepartment.ToString()
            }).ToList();
            List<SelectListItem> positionItems = positions
            .Select(d => new SelectListItem
            {
                Text = d.PositionName,
                Value = d.IdPositions.ToString()
            }).ToList();

            // ViewBag'de SelectListItem'ları sakla
            ViewBag.Departments = departmentItems;
            ViewBag.Positions = positionItems;
            return View();
        }
        [HttpPost]
        public IActionResult EmployeeAdd(Employee employee)
        {
            if (ModelState.IsValid)
            {
                if (employee.IdDepartment != 0 || employee.IdPositions != 0)
                {
                    // (Eager Loading) kullanarak Department'ı yükle
                    var employees = _context.Employees
                              .Include(e => e.IdDepartmentNavigation)
                              .Include(e => e.IdPositionsNavigation)
                              .ToList();

                    _context.Employees.Add(employee);
                    _context.SaveChanges();
                    return RedirectToAction("EmployeeList");
                }
                else
                {
                    // Hata mesajları ile birlikte hata sayfasını göster
                    var departments = _context.Departments.ToList();
                    var positions = _context.Positions.ToList();
                    ViewBag.Departments = new SelectList(departments, "IdDepartment", "DepartmentName", employee.IdDepartment);
                    ViewBag.Positions = new SelectList(positions, "IdPositions", "PositionName", employee.IdPositions);

                    return View("ErrorView", employee);

                }
            }
            else
            {
                var departments = _context.Departments.ToList();
                ViewBag.Departments = new SelectList(departments, "IdDepartment", "DepartmentName", employee.IdDepartment);
                var positions = _context.Positions.ToList();
                ViewBag.Positions = new SelectList(positions, "IdPositions", "PositionName", employee.IdPositions);

                return View();
            }
        }
        [HttpGet]
        public IActionResult EmployeeEdit(int id)
        {
            var departments = _context.Departments.ToList();
            var positions = _context.Positions.ToList();

            List<SelectListItem> departmentItems = departments
                .Select(d => new SelectListItem
                {
                    Text = d.DepartmentName,
                    Value = d.IdDepartment.ToString()
                }).ToList();
            List<SelectListItem> positionItems = positions
           .Select(d => new SelectListItem
           {
               Text = d.PositionName,
               Value = d.IdPositions.ToString()
           }).ToList();

            ViewBag.Positions = positionItems;
            ViewBag.Departments = departmentItems;

            Employee employee = _context.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View("EmployeeEdit", employee);
        }
        [HttpPost]
        public IActionResult EmployeeEdit(Employee editedEmployee)
        {
            if (ModelState.IsValid)
            {
                // Çalışanı güncelle
                _context.Employees.Update(editedEmployee);
                _context.SaveChanges();

                return RedirectToAction("EmployeeList");
            }
            else
            {
                return View("ErrorView", editedEmployee);
            }
        }

        public IActionResult EmployeeDetail(int id)
        {
            var departments = _context.Departments.ToList();
            var positions = _context.Positions.ToList();

            List<SelectListItem> departmentItems = departments
                .Select(d => new SelectListItem
                {
                    Text = d.DepartmentName,
                    Value = d.IdDepartment.ToString()
                }).ToList();
            List<SelectListItem> positionItems = positions
           .Select(d => new SelectListItem
           {
               Text = d.PositionName,
               Value = d.IdPositions.ToString()
           }).ToList();

            ViewBag.Positions = positionItems;
            ViewBag.Departments = departmentItems;

            Employee employee = _context.Employees.Find(id);
            if (employee == null)
            {
                return NotFound();
            }
            return View("EmployeeDetail", employee);
        }

        public IActionResult EmployeeTaskStatusChart(int employeeId)
        {
            var employee = _context.Employees
                .Include(e => e.TaskComps)
                .ThenInclude(tc => tc.Status)
                .FirstOrDefault(e => e.IdEmployee == employeeId);

            if (employee == null)
            {
                return NotFound();
            }

            var chartData = new
            {
                labels = employee.TaskComps.Select(tc => tc.Status.StatusName).Distinct(),
                data = employee.TaskComps.GroupBy(tc => tc.Status.StatusName).Select(group => group.Count())
            };

            return Json(chartData);
        }
        public IActionResult EmployeeChart(int employeeId)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.IdEmployee == employeeId);

            if (employee == null)
            {
                return NotFound(); // Employee bulunamadıysa hata döndür
            }

            var taskComps = _context.TaskComps
                .Where(tc => tc.EmployeeId == employeeId)
                .GroupBy(tc => tc.StatusId)
                .Select(group => new
                {
                    StatusId = group.Key,
                    StatusName = group.FirstOrDefault().Status != null ? group.FirstOrDefault().Status.StatusName : null,
                    Count = group.Count() // Duruma göre görev sayısını say
                })
                .ToList();

            // JSON formatına çevirerek geri döndür
            return Content(JsonConvert.SerializeObject(taskComps), "application/json");
        }

        public IActionResult EmployeeDelete(int id)
        {
            Employee employee = _context.Employees.Find(id);

            if (employee == null)
            {
                return NotFound();
            }

            _context.Employees.Remove(employee);
            _context.SaveChanges();
            return RedirectToAction("EmployeeList");
        }
        #endregion

        #region DEPARTMENT
        public IActionResult DepartmentList()
        {
            var department = _context.Departments.ToList();
            return View(department);
        }

        [HttpGet]
        public IActionResult DepartmentAdd()
        {
            return View();
        }
        [HttpPost]
        public IActionResult DepartmentAdd(Department department)
        {
            if (ModelState.IsValid)
            {
                _context.Departments.Add(department);
                _context.SaveChanges();

                return RedirectToAction("DepartmentList");
            }
            return View(department);
        }
        [HttpGet]
        public IActionResult DepartmentEdit(int id)
        {
            // id parametresini kullanarak düzenlenecek müşteriyi veritabanından al
            Department department = _context.Departments.Find(id);

            if (department == null)
            {
                return NotFound();
            }

            return View(department);
        }
        [HttpPost]
        public IActionResult DepartmentEdit(Department editedDepartmen)
        {

            if (ModelState.IsValid)
            {
                _context.Entry(editedDepartmen).State = EntityState.Modified;
                _context.SaveChanges();

                return RedirectToAction("DepartmentList");
            }

            return View(editedDepartmen);
        }
        [HttpPost]
        public IActionResult DepartmentDelete(int id)
        {
            Department department = _context.Departments.Find(id);

            if (department == null)
            {
                return NotFound();
            }

            _context.Departments.Remove(department);
            _context.SaveChanges();
            return RedirectToAction("DepartmentList");
        }

        #endregion

        #region POSITION
        public IActionResult PositionList()
        {
            var position = _context.Positions.ToList();
            return View(position);
        }

        [HttpGet]
        public IActionResult PositionAdd()
        {
            return View();
        }
        [HttpPost]
        public IActionResult PositionAdd(Position position)
        {
            if (ModelState.IsValid)
            {
                _context.Positions.Add(position);
                _context.SaveChanges();

                return RedirectToAction("PositionList");
            }
            return View(position);
        }
        [HttpGet]
        public IActionResult PositionEdit(int id)
        {
            Position position = _context.Positions.Find(id);

            if (position == null)
            {
                return NotFound();
            }

            return View(position);
        }
        [HttpPost]
        public IActionResult PositionEdit(Position editedPosition)
        {
            if (ModelState.IsValid)
            {
                _context.Entry(editedPosition).State = EntityState.Modified;
                _context.SaveChanges();

                return RedirectToAction("PositionList");
            }

            return View("ErrorView", editedPosition);
        }

        public IActionResult PositionDelete(int id)
        {
            Position position = _context.Positions.Find(id);

            if (position == null)
            {
                return NotFound();
            }

            _context.Positions.Remove(position);
            _context.SaveChanges();
            return RedirectToAction("PositionList");
        }

        #endregion
    }
}
