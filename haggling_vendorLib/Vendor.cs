
namespace haggling_interfaces;

public class Vendor : IVendor
{
    public string Name { get; init; }
    public int Age { get; init; }
    public Percentage Patience { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IProduct[] Products { get; init; }
    public decimal Money { get => _money; }

    private int _offers = 0;
    private int _maxOffers = 10;
    private List<IOffer> _pastOffers = new List<IOffer>();
    private decimal _money = 0;
    private static readonly IProduct[] _allProducts = [
        new VendorProduct("Apfel", ProductType.Food, 2),
        new VendorProduct("Mango", ProductType.Food, 3),
        new VendorProduct("Haslinger's Laptop", ProductType.Electronics, 100),
        new VendorProduct("Henry's Unterhose 67", ProductType.Clothing, 1),
        new VendorProduct("Couch", ProductType.Furniture, 23),
        new VendorProduct("Haslinger's 1:1 Memorial", ProductType.Furniture, 100),
        new VendorProduct("David's Lichtschwert", ProductType.Toys, 45),
        new VendorProduct("\"Wie man fliegt\" by Horvath Jr.", ProductType.Books, 100),
        new VendorProduct("Hammer", ProductType.Tools, 10),
        new VendorProduct("Werkzeugkasten", ProductType.Tools, 11),
        new VendorProduct("Henry's Sportleiberl", ProductType.SportsEquipment, 1),
        new VendorProduct("Andre's 67 cm Dildo", ProductType.SportsEquipment, 5),
        new VendorProduct("Haslingers IPod Pro 2005 Edition ", ProductType.Electronics, 35),
        new VendorProduct("Free Benko T-Shirt", ProductType.Clothing, 2),
        new VendorProduct("Tisch", ProductType.Furniture, 40),
        new VendorProduct("Spiderman Life Action Figure", ProductType.Toys, 15),
        new VendorProduct("GTA 6", ProductType.Electronics, 12),
        new VendorProduct("Hammer", ProductType.Tools, 25),
        new VendorProduct("Fußball mit dem JFK erschossen wurde", ProductType.SportsEquipment, 8),
        new VendorProduct("Apfel Watch 67", ProductType.Electronics, 20)
    ];

    public Vendor(string name, int age)
    {
        Name = name;
        Age = age;

        Products = GenerateProducts();

    }

    private static IProduct[] GenerateProducts()
    {
        Random rand = new Random();
        int productCount = rand.Next(3, _allProducts.Length - 1);
        IProduct[] products = new IProduct[productCount];

        for (int i = 0; i < productCount; i++)
        {
            IProduct product;
            do
            {
                product = _allProducts[rand.Next(_allProducts.Length)];
            } while (Array.Exists(products, p => p != null && p.Name == product.Name));

            products[i] = product;
        }

        return products;
    }

    public void AcceptTrade(IOffer offer)
    {
        if (offer.Status == OfferStatus.Accepted)
        {
            _money += offer.Price;
            // Remove the sold product from the vendor's inventory
            var productList = Products.ToList();
            productList.Remove(offer.Product);
            // Products = productList.ToArray();
            StopTrade();
        }


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
        if (_offers >= _maxOffers)
        {
            offer.Status = OfferStatus.Stopped;
            // StopTrade(); //TODO ka ob wir das machen müssen oder ob das von außen aufgerufen wird 
            return offer;
        }
        else
        {
            _offers++;
            _pastOffers.Add(offer);
            var estPrice = GetEstimatedPrice(offer.Product, customer);

            if (offer.Price > estPrice * 2m) { offer.Status = OfferStatus.Accepted; }
            if (offer.Price < estPrice * 0.5m) { offer.Status = OfferStatus.Stopped; }
            
            
        return offer;
        }
    }

    public void StopTrade()
    {
        this._pastOffers.Clear();
        this._offers = 0;
        this._maxOffers = 10;
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
