using System;

namespace haggling_interfaces;

public class Vendor : IVendor
{
    public string Name { get; init; }
    public int Age { get; init; }
    public Percentage Patience { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IProduct[] Products { get; init; }

    public Vendor(string name, int age)
    {
        Name = name;
        Age = age;

        Products =
        [
            new VendorProduct("Apfel", ProductType.Food, 2),
            new VendorProduct("Mango", ProductType.Food, 3),
            new VendorProduct("Haslinger's Laptop", ProductType.Electronics, 100),
            new VendorProduct("Henry's Unterhose", ProductType.Clothing, 1),
            new VendorProduct("Couch", ProductType.Furniture, 23),
            new VendorProduct("Haslinger's 1:1 Memorial", ProductType.Furniture, 100),
            new VendorProduct("David's Lichtschwert", ProductType.Toys, 45),
            new VendorProduct("\"Wie man fliegt\" by Horvath Jr.", ProductType.Books, 100),
            new VendorProduct("Hammer", ProductType.Tools, 10),
            new VendorProduct("Werkzeugkasten", ProductType.Tools, 11),
            new VendorProduct("Henry's Sportleiberl", ProductType.SportsEquipment, 1)
        ];
    }

    public void AcceptTrade(IOffer offer)
    {
        throw new NotImplementedException(); //TODO: Maybe dann die Rarity von dem Product erhöhen wenns verkauft worden is?
    }

    public IOffer GetStartingOffer(IProduct product, ICustomer customer)
    {
        return new VendorOffer()
        {
            Status = OfferStatus.Ongoing,
            Product = product,
            Price = GetEstimatedPrice(product, customer),
            OfferedBy = PersonType.Vendor
        };
    }

    public IOffer RespondToOffer(IOffer offer, ICustomer customer)
    {
        var estPrice = GetEstimatedPrice(offer.Product, customer);

        if (offer.Price < estPrice * 0.1m) // Das einfach bodenlos frech.
        {
            offer.Status = OfferStatus.Stopped;
        }

        if (offer.Price == estPrice)
        {
            offer.Status = OfferStatus.Accepted;
        }

        // TODO: logik zum bisherige preise speichern fehlt und dann vergleichen wv wer runter gegangen ist.


        return offer;
    }

    public void StopTrade()
    {
        throw new NotImplementedException(); //TODO: idk was da resetted werden muss? und wieso tf ist des in am interface und iwie relevant für public? 
    }

    static private decimal GetEstimatedPrice(IProduct product, ICustomer customer)
    {
        decimal estPrice = product.Rarity.Value * 100 + 1;

        if (customer.Age < 18 && product.Type == ProductType.Food)
        {
            estPrice *= 0.8m; // under 18-year olds buying food get a 20% discount.
        }
        else if (customer.Age < 18)
        {
            estPrice *= 0.9m; // under 18-year olds NOT buying food get 10% discount.
        }

        return estPrice;
    }
}
