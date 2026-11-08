namespace AromaDom.Models;

public sealed record MaterialCard(
    long Id,
    string Name,
    string TypeName,
    string UnitName,
    decimal Price,
    decimal Stock,
    decimal Minimum,
    decimal RequiredQuantity);
