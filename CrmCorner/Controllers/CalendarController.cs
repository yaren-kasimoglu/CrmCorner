using System;
using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;


namespace CrmCorner.Controllers
{
    public class CalendarController:Controller
    {
        private readonly CrmCornerContext _context;
        public CalendarController(CrmCornerContext context)
        {
            _context = context;
        }
        public IActionResult Calendar()
        {
            var calendars = _context.Calendars.ToList();
            List<Calendar> calendarItems = calendars
               .Select(c => new Calendar
               {
                   Date = c.Date,
                   Id = c.Id,
                   Title = c.Title
               }).ToList();
            List<Calendar> calendarItemsFilter = calendars
         .Select(c => new Calendar
         {
             Date = c.Date,
             Id = c.Id,
             Title = c.Title,
             Description = c.Description
         }).ToList();
            ViewBag.Calendar = calendars;
            ViewBag.CalendarFilter = calendarItemsFilter.Take(5);
            return View();
        }
        [HttpPost]
        public IActionResult CalendarAdd(Calendar Calendar)
        {
            if (ModelState.IsValid)
            {
                _context.Calendars.Add(Calendar);
                _context.SaveChanges();
                return RedirectToAction("Calendar");
            }
            return View(Calendar);
        }
    }
}

