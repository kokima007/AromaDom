using AromaDom.Models;
using AromaDom.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AromaDom.Pages;

public class IndexModel(MaterialCatalog catalog, ILogger<IndexModel> logger) : PageModel
{
    public IReadOnlyList<MaterialCard> Materials { get; private set; } = [];
    public string? LoadError { get; private set; }

    public void OnGet()
    {
        try
        {
            Materials = catalog.GetMaterials();
        }
        catch (Exception exception) when (exception is Microsoft.Data.Sqlite.SqliteException
            or IOException or UnauthorizedAccessException)
        {
            logger.LogError(exception, "Не удалось загрузить список материалов.");
            LoadError = "Не удалось загрузить материалы. Проверьте доступность базы данных и обновите страницу. Если ошибка повторяется, обратитесь к администратору.";
            Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        }
    }
}
