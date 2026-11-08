using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
namespace AromaDom.Pages;
public class ProductsModel(Store store) : PageModel
{
    public Dictionary<string,object> Material { get; set; } = new();
    public List<Dictionary<string,object>> Products { get; set; } = [];
    public IActionResult OnGet(int id)
    {
        var material=store.Query("SELECT m.Name,u.Name AS UnitName FROM Materials m JOIN Units u ON u.Id=m.UnitId WHERE m.Id=$id",("$id",id)).FirstOrDefault();
        if(material is null) return NotFound();
        Material=material;
        Products=store.Query("SELECT p.Article,p.Name,mp.Quantity FROM Products p JOIN MaterialProducts mp ON mp.ProductId=p.Id WHERE mp.MaterialId=$id ORDER BY p.Article",("$id",id));
        return Page();
    }
}
