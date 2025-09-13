using haggling_interfaces;
using System;

public class VendorProduct : IProduct
{
    public string Name { get; init; }
    public ProductType Type { get; init; }
    public Percentage Rarity { get; set; }

    public VendorProduct(string name, ProductType type, Percentage rarity)
    {
        Name = name;
        Type = type;
        Rarity = rarity;
    }
}
