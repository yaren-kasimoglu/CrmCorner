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
        private readonly CrmcornerContext _context;
        public CalendarController(CrmcornerContext context)
        {
            _context = context;
        }
        public IActionResult Calendar()
        {
            var calendars = _context.Calendars.ToList();
            List<Calendar> calendarItems = calendars
               .Select(c => new Calendar
               {
                    Date=c.Date,
                    Id=c.Id,
                    Title=c.Title
               }).ToList();
            
            ViewBag.Calendar = calendars;
            return View();
        }
    }
}

