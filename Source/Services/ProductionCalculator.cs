namespace AromaDom.Services;

public sealed class ProductionCalculator(Store store)
{
    public int Calculate(int productTypeId, int materialTypeId, int rawQuantity, double parameter1, double parameter2)
    {
        if (rawQuantity < 0 || !double.IsFinite(parameter1) || !double.IsFinite(parameter2)
            || parameter1 <= 0 || parameter2 <= 0)
        {
            return -1;
        }

        var types = store.Query("""
            SELECT p.Coefficient, m.LossPercent
            FROM ProductTypes p CROSS JOIN MaterialTypes m
            WHERE p.Id = $productTypeId AND m.Id = $materialTypeId
            """, ("$productTypeId", productTypeId), ("$materialTypeId", materialTypeId)).SingleOrDefault();

        if (types is null)
        {
            return -1;
        }

        try
        {
            var coefficient = Convert.ToDecimal(types["Coefficient"]);
            var lossPercent = Convert.ToDecimal(types["LossPercent"]);
            var perUnit = (decimal)parameter1 * (decimal)parameter2 * coefficient;
            if (perUnit <= 0 || lossPercent < 0 || lossPercent >= 100)
            {
                return -1;
            }

            // В справочнике 0,12 означает 0,12 % потерь.
            var usableQuantity = rawQuantity * (1m - lossPercent / 100m);
            var output = decimal.Floor(usableQuantity / perUnit);
            return output <= int.MaxValue ? decimal.ToInt32(output) : -1;
        }
        catch (OverflowException)
        {
            return -1;
        }
    }
}
