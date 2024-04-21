using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;

namespace CrmCorner.Controllers;

public class BigecSayfaDeneme : Controller
{
    private readonly CrmCornerContext _Context; 
    public BigecSayfaDeneme(CrmCornerContext context)
    {
        _Context=context;
    }
    public IActionResult Index()
    {
        var BigecSayfaDenemes = _Context.BigeçSayfaDenemes.ToList();
        return View(BigecSayfaDenemes);
    }

    [HttpGet]
    public IActionResult ContentAdd()
    {
        return View();
    }

    [HttpPost]
    public IActionResult ContentAdd(BigecSayfaDeneme bigecSayfaDeneme)
    {
        return View();
    }


}
