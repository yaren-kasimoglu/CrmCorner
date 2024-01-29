using System;
using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
	public class ChatAppController : Controller
    {
        private readonly CrmCornerContext _context;
        public ChatAppController(CrmCornerContext context)
        {
            _context = context;
        }
        public IActionResult ChatApp()
        {
            var employees = _context.Employees
                                .Include(e => e.IdDepartmentNavigation)
                                .Include(e => e.IdPositionsNavigation)
                                .ToList();
            return View(employees);
        }
        [HttpGet]
        public IActionResult ChatApp(string search)
        {
            if (search != null)
            {
                var employees = _context.Employees.Where(emp => emp.EmployeeName.StartsWith(search) || search == null).ToList();
                return View(employees);
            }
            else
            {
                var employees = _context.Employees
                                .ToList();
                return View(employees);
            }
        }
        
    }
}

