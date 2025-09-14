
namespace haggling_interfaces;

public class Vendor : IVendor
{
    public string Name { get; init; }
    public int Age { get; init; }

    private Percentage _patience;
    public Percentage Patience
    {
        get => _patience;
        set => _patience = value;
    }
    public IProduct[] Products { get; init; }
    public decimal Money { get => _money; }

    private List<IProduct> _inventory = new List<IProduct>();
    private int _maxPatience;
    private const int _patienceDecreaseOnOffer = 5;             // Decrease patience by this amount on every offer made by the customer
    private const int _patienceDecreaseOnUndershoot = 10;       // Undershoot means that the customer offered to few funds in the offer
    private const int _patienceDecreaseOnBigUndershoot = 25;    // BigUndershoot means that the customer offered way to few funds in the offer

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
        new VendorProduct("Apfel Watch 67", ProductType.Electronics, 20),
        new VendorProduct("Mexikaser T-Shirt", ProductType.Clothing, 5)
    ];

    public Vendor(string name, int age)
    {
        Name = name;
        Age = age;

        Products = GenerateProducts();
        _inventory = Products.ToList();

        // "Some vendors are more patient than others" implementation from the pdf
        Random rand = new Random();
        _maxPatience = rand.Next(80, 101);
        _patience = new Percentage(_maxPatience);
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

            int idx = _inventory.FindIndex(p => p.Name == offer.Product.Name);

            if (idx == -1)
            {
                throw new InvalidOperationException("Produkt existiert nicht im Inventar.");
            }
            else
            {
                _inventory.RemoveAt(idx);
                int remainingAmount = _inventory.Count(p => p.Name == offer.Product.Name);
                if (remainingAmount > 1)
                {
                    foreach (var product in _inventory.Where(p => p.Name == offer.Product.Name))
                    {
                        product.Rarity = new Percentage(product.Rarity.Value - 5);
                    }

                }
                else if (remainingAmount == 1)
                {
                    foreach (var product in _inventory.Where(p => p.Name == offer.Product.Name))
                    {
                        product.Rarity = new Percentage(100);
                    }
                }
                else
                {
                    StopTrade();
                }
            }
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
        if (_patience.Value == 0)
        {
            offer.Status = OfferStatus.Stopped;
            StopTrade(); 
            return offer;
        }
        else
        {
            _pastOffers.Add(offer);
            var estPrice = GetEstimatedPrice(offer.Product, customer);

            if (offer.Price > estPrice * 1.3m) { offer.Status = OfferStatus.Accepted; }

            _patience.Value -= _patienceDecreaseOnOffer;
            if (offer.Price < estPrice * 0.5m)
            {
                _patience.Value -= _patienceDecreaseOnBigUndershoot;
            }
            else if (offer.Price < estPrice * 0.8m)
            {
                _patience.Value -= _patienceDecreaseOnUndershoot;
            }

            return offer;
        }
    }

    public void StopTrade()
    {
        this._pastOffers.Clear();
        this._patience.Value = this._maxPatience;
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
