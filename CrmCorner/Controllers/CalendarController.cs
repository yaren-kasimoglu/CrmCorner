using System;
using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using System.Xml.Serialization;
using System.Data.Common;


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

                    Date=c.Date,
                    Id=c.Id,
                    Title=c.Title,
                    Description=c.Description

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
        [HttpPost]
        public IActionResult CalendarUpdate(int? ID,string title,string date)
        {
            var htmlAttributes = ViewBag.Id;
            Calendar calendar = new Calendar { Id = ID.Value, Title = title ,Date=date};
            if (ModelState.IsValid)
            {
                _context.Calendars.Update(calendar);
                _context.SaveChanges();

               return Json(new { Message = "success"});
            }
            return View("Calendar");
        }

        [HttpPost]
        public IActionResult CalendarDelete(int? ID)
        {
            Calendar calendar = _context.Calendars.Find(ID);

            
            if (calendar == null)
            {
                return NotFound();
            }
            _context.Calendars.Remove(calendar);
            _context.SaveChanges();
           return Json(new { Message = "success" });

        }
        [HttpPost]
        public IActionResult GetDescription(int? ID)
        {
            Calendar calendar = _context.Calendars.Find(ID);


            if (calendar == null)
            {
                return Json(new { Message = "error"});
            }
            return Json(new { Message = calendar.Description });

        }

    }
}

