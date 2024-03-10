using System;
using CrmCorner.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmCorner.Controllers
{
	public class ToDoListController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly CrmCornerContext _context;

        public IActionResult ToDoList()
            {
            //Seçili olanların listesini getirceksin.tablo bağlandıktan sonra
            //USERA BAKARAK YAPMAN LAZIM.-----
            return View();
            }

        [HttpPost]
        public async Task<IActionResult> ToDoListdd(ToDo todo)
        {
            //seçili olanları seçili olanalra , seçili olmayanları seçili olmayanlara
            //USERA BAKARAK YAPMAN LAZIM.------
            //İki tarafıda virgülle ayırcaksınn.
            //Update işlemi burda yapılacak, eğer o tarihte data varsa tüm seçilenler
            //ve seçilmeyenleri güncelliceksin.Yoksa ekleme işlemi yaptırcksın.
            if (ModelState.IsValid)
            {
                

            }
            return RedirectToAction("ToDoList");
        }
       
    }
}

