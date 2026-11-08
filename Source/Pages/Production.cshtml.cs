using System.ComponentModel.DataAnnotations;
using AromaDom.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AromaDom.Pages;

public class ProductionModel(Store store, ProductionCalculator calculator) : PageModel
{
    public List<Dictionary<string, object>> ProductTypes { get; private set; } = [];
    public List<Dictionary<string, object>> MaterialTypes { get; private set; } = [];
    public int? Result { get; private set; }

    [BindProperty]
    public Input Data { get; set; } = new();

    public class Input
    {
        [Range(1, int.MaxValue, ErrorMessage = "Выберите тип продукции.")]
        public int ProductTypeId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Выберите тип материала.")]
        public int MaterialTypeId { get; set; }

        [Required(ErrorMessage = "Введите количество сырья.")]
        [Range(0, int.MaxValue, ErrorMessage = "Количество сырья должно быть целым и неотрицательным.")]
        public int? RawQuantity { get; set; }

        [Required(ErrorMessage = "Введите первый параметр.")]
        public double? Parameter1 { get; set; }

        [Required(ErrorMessage = "Введите второй параметр.")]
        public double? Parameter2 { get; set; }
    }

    public void OnGet() => LoadTypes();

    public void OnPost()
    {
        LoadTypes();
        if (!ModelState.IsValid)
        {
            return;
        }

        var result = calculator.Calculate(Data.ProductTypeId, Data.MaterialTypeId,
            Data.RawQuantity!.Value, Data.Parameter1!.Value, Data.Parameter2!.Value);
        if (result < 0)
        {
            ModelState.AddModelError("", "Выберите действующие типы и введите положительные параметры в допустимом диапазоне.");
            return;
        }

        Result = result;
    }

    private void LoadTypes()
    {
        ProductTypes = store.Query("SELECT Id, Name FROM ProductTypes ORDER BY Name");
        MaterialTypes = store.Query("SELECT Id, Name FROM MaterialTypes ORDER BY Name");
    }
}
