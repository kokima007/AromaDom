namespace AromaDom.Services;

public static class MaterialRequirementCalculator
{
    public static decimal Calculate(IEnumerable<decimal> recipeQuantities)
    {
        ArgumentNullException.ThrowIfNull(recipeQuantities);
        decimal total = 0;
        foreach (var quantity in recipeQuantities)
        {
            if (quantity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(recipeQuantities), "Расход материала не может быть отрицательным.");
            }
            total += quantity;
        }
        // Округляем сумму, а не отдельные рецептуры: иначе мелкие расходы теряются.
        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }
}
