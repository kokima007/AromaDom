using System.Globalization;
using AromaDom.Models;

namespace AromaDom.Services;

public sealed class MaterialCatalog(Store store)
{
    public IReadOnlyList<MaterialCard> GetMaterials()
    {
        using var connection = store.Open();
        using var transaction = connection.BeginTransaction();
        using var materialCommand = connection.CreateCommand();
        materialCommand.Transaction = transaction;
        materialCommand.CommandText = """
            SELECT m.Id, m.Name, t.Name, u.Name, m.PriceCents, m.Stock, m.Minimum
            FROM Materials m
            JOIN MaterialTypes t ON t.Id = m.MaterialTypeId
            JOIN Units u ON u.Id = m.UnitId
            ORDER BY m.Name
            """;
        var materials = new List<MaterialCard>();
        using (var reader = materialCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                materials.Add(new MaterialCard(
                    reader.GetInt64(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetInt64(4) / 100m,
                    Convert.ToDecimal(reader.GetDouble(5), CultureInfo.InvariantCulture),
                    Convert.ToDecimal(reader.GetDouble(6), CultureInfo.InvariantCulture),
                    0m));
            }
        }
        using var recipeCommand = connection.CreateCommand();
        recipeCommand.Transaction = transaction;
        recipeCommand.CommandText = "SELECT MaterialId, Quantity FROM MaterialProducts ORDER BY MaterialId, ProductId";
        var quantities = new Dictionary<long, List<decimal>>();
        using (var reader = recipeCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                var materialId = reader.GetInt64(0);
                if (!quantities.TryGetValue(materialId, out var values))
                {
                    values = [];
                    quantities.Add(materialId, values);
                }
                values.Add(Convert.ToDecimal(reader.GetDouble(1), CultureInfo.InvariantCulture));
            }
        }
        transaction.Commit();
        return materials.Select(material => material with
        {
            RequiredQuantity = MaterialRequirementCalculator.Calculate(
                quantities.GetValueOrDefault(material.Id) ?? [])
        }).ToList();
    }
}
