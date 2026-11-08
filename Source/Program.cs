using AromaDom;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages().AddMvcOptions(options =>
{
    var messages = options.ModelBindingMessageProvider;
    messages.SetMissingBindRequiredValueAccessor(_ => "Заполните обязательное поле.");
    messages.SetAttemptedValueIsInvalidAccessor((_, _) => "Укажите допустимое значение.");
    messages.SetValueMustNotBeNullAccessor(_ => "Заполните обязательное поле.");
    messages.SetValueMustBeANumberAccessor(_ => "Введите число.");
});
builder.Services.AddSingleton<Store>();
builder.Services.AddScoped<AromaDom.Services.MaterialCatalog>();
builder.Services.AddScoped<AromaDom.Services.ProductionCalculator>();
var app = builder.Build();
app.Services.GetRequiredService<Store>().Initialize();
app.UseExceptionHandler("/Error");
app.UseStatusCodePagesWithReExecute("/Status", "?code={0}");
app.UseStaticFiles();
app.UseRouting();
app.MapRazorPages();
app.Run();

