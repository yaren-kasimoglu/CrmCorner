using CrmCorner.Models;
using Microsoft.AspNetCore.Mvc;

namespace CrmCorner.Controllers;

public class BigeçSayfaDeneme : Controller
{
    private readonly CrmCornerContext _Context; 
    public BigeçSayfaDeneme(CrmCornerContext context)
    {
        _Context=context;
    }
    public IActionResult Index()
    {
        var BigecSayfaDenemes = _Context.BigeçSayfaDenemes.ToList();
        return View(BigecSayfaDenemes);
    }
}
