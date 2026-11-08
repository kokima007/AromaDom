using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace AromaDom.Pages;
public class MaterialModel(Store store) : PageModel
{
    [TempData] public string? SuccessMessage { get; set; }
    [BindProperty] public Input Data { get; set; } = new();
    public List<Dictionary<string, object>> Types { get; set; } = [];
    public List<Dictionary<string, object>> Units { get; set; } = [];
    public class Input
    {
        [BindRequired, Range(0, int.MaxValue, ErrorMessage = "Некорректный идентификатор материала.")]
        public int Id { get; set; }
        [Required(ErrorMessage="Введите наименование.")][StringLength(200, ErrorMessage = "Введите не более 200 символов.")] public string Name { get; set; } = "";
        [BindRequired, Range(1, int.MaxValue, ErrorMessage = "Выберите тип материала.")] public int MaterialTypeId { get; set; }
        [BindRequired, Range(1, int.MaxValue, ErrorMessage = "Выберите единицу измерения.")] public int UnitId { get; set; }
        [BindRequired, Range(typeof(decimal),"0","1000000000",ErrorMessage="Цена должна быть от 0 до 1 млрд.")] public decimal Price { get; set; }
        [BindRequired, Range(0, 1000000000, ErrorMessage = "Укажите остаток от 0 до 1 млрд.")] public double Stock { get; set; }
        [BindRequired, Range(0, 1000000000, ErrorMessage = "Укажите минимальное количество от 0 до 1 млрд.")] public double Minimum { get; set; }
        [BindRequired, Range(0.000001, 1000000000, ErrorMessage = "Укажите положительное количество в упаковке от 0,000001 до 1 млрд.")] public double PackageQuantity { get; set; } = 1;
    }
    private void Load() { Types = store.Query("SELECT * FROM MaterialTypes ORDER BY Name"); Units = store.Query("SELECT * FROM Units ORDER BY Name"); }
    public IActionResult OnGet(int? id)
    {
        if (!ModelState.IsValid) return BadRequest();
        Load();
        if (id is null) return Page();
        var m = store.Query("SELECT * FROM Materials WHERE Id=$id", ("$id",id.Value)).FirstOrDefault();
        if (m is null) return NotFound();
        Data = new Input { Id=Convert.ToInt32(m["Id"]),Name=(string)m["Name"],MaterialTypeId=Convert.ToInt32(m["MaterialTypeId"]),UnitId=Convert.ToInt32(m["UnitId"]),Price=Convert.ToDecimal(m["PriceCents"])/100,Stock=Convert.ToDouble(m["Stock"]),Minimum=Convert.ToDouble(m["Minimum"]),PackageQuantity=Convert.ToDouble(m["PackageQuantity"]) };
        return Page();
    }
    public IActionResult OnPost()
    {
        Load();
        if (Data.Price != decimal.Round(Data.Price, 2))
        {
            ModelState.AddModelError("Data.Price", "Укажите цену с точностью до копеек.");
        }
        if (Data.MaterialTypeId > 0 && !Types.Any(type => Convert.ToInt32(type["Id"]) == Data.MaterialTypeId))
        {
            ModelState.AddModelError("Data.MaterialTypeId", "Выберите действующий тип материала из списка.");
        }
        if (Data.UnitId > 0 && !Units.Any(unit => Convert.ToInt32(unit["Id"]) == Data.UnitId))
        {
            ModelState.AddModelError("Data.UnitId", "Выберите действующую единицу измерения из списка.");
        }
        if (!ModelState.IsValid)
        {
            return Page();
        }
        using var connection = store.Open();
        using var command = connection.CreateCommand();
        command.CommandText = Data.Id == 0 ? "INSERT INTO Materials(Name,MaterialTypeId,UnitId,PriceCents,Stock,Minimum,PackageQuantity) VALUES($name,$type,$unit,$price,$stock,$min,$pack)" : "UPDATE Materials SET Name=$name,MaterialTypeId=$type,UnitId=$unit,PriceCents=$price,Stock=$stock,Minimum=$min,PackageQuantity=$pack WHERE Id=$id";
        command.Parameters.AddWithValue("$id",Data.Id);
        command.Parameters.AddWithValue("$name",Data.Name.Trim());
        command.Parameters.AddWithValue("$type",Data.MaterialTypeId);
        command.Parameters.AddWithValue("$unit",Data.UnitId);
        command.Parameters.AddWithValue("$price",decimal.ToInt64(Data.Price*100));
        command.Parameters.AddWithValue("$stock",Data.Stock);
        command.Parameters.AddWithValue("$min",Data.Minimum);
        command.Parameters.AddWithValue("$pack",Data.PackageQuantity);
        try
        {
            if (command.ExecuteNonQuery() == 0)
            {
                return NotFound();
            }
        }
        catch (SqliteException exception) when (exception.SqliteExtendedErrorCode == 2067)
        {
            ModelState.AddModelError("Data.Name", "Материал с таким названием уже существует. Укажите другое название.");
            return Page();
        }
        catch (SqliteException exception) when (exception.SqliteErrorCode == 19)
        {
            ModelState.AddModelError("", "Данные не прошли проверку базы. Проверьте количества, тип материала и единицу измерения.");
            return Page();
        }
        SuccessMessage = Data.Id == 0 ? "Материал добавлен." : "Изменения сохранены.";
        return RedirectToPage("Index");
    }
}
