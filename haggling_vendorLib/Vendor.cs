
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

    public decimal Money { get => _money; }
    public IProduct[] Products
    {
        get => _inventory.Where(p => p != null).ToArray();
        init => _inventory = [.. value];
    }

    // for determining if the Customer increased their price (compared to the last trade)
    private decimal _lastCustomerPrice = 0;

    private List<IProduct> _inventory = [];
    private int _maxPatience;
    private const int _patienceDecreaseOnOffer = 5;             // Decrease patience by this amount on every offer made by the customer
    private const int _patienceDecreaseOnUndershoot = 10;       // Undershoot means that the customer offered to few funds in the offer
    private const int _patienceDecreaseOnBigUndershoot = 25;    // BigUndershoot means that the customer offered way to few funds in the offer

    private List<IOffer> _pastOffers = [];
    private decimal _money = 0;
    private static readonly IProduct[] _allProducts = [
        new VendorProduct("Apfel", ProductType.Food, 2),
        new VendorProduct("Mango", ProductType.Food, 3),
        new VendorProduct("Lenovo Laptop", ProductType.Electronics, 100),
        new VendorProduct("Nike Socken", ProductType.Clothing, 1),
        new VendorProduct("Couch", ProductType.Furniture, 23),
        new VendorProduct("Kommode", ProductType.Furniture, 100),
        new VendorProduct("David's Lichtschwert", ProductType.Toys, 45),
        new VendorProduct("c# buch", ProductType.Books, 100),
        new VendorProduct("Werkzeugkasten", ProductType.Tools, 11),
        new VendorProduct("Sportshirt", ProductType.SportsEquipment, 1),
        new VendorProduct("Hantel", ProductType.SportsEquipment, 5),
        new VendorProduct("IPhone 17", ProductType.Electronics, 35),
        new VendorProduct("Gucci T-Shirt", ProductType.Clothing, 2),
        new VendorProduct("Tisch", ProductType.Furniture, 40),
        new VendorProduct("Spiderman Life Action Figure", ProductType.Toys, 15),
        new VendorProduct("GTA 6", ProductType.Electronics, 12),
        new VendorProduct("Hammer", ProductType.Tools, 25),
        new VendorProduct("Fußball ", ProductType.SportsEquipment, 8),
        new VendorProduct("Apple Watch 10", ProductType.Electronics, 20),
        new VendorProduct("Louis Vuitton T-Shirt", ProductType.Clothing, 5)
    ];

    public Vendor(string name, int age)
    {
        Name = name;
        Age = age;

        Products = GenerateProducts();
        //_inventory = Products.ToList(); THEOREDICLY not neccesary
        // "Some vendors are more patient than others" implementation from the pdf
        Random rand = new();
        _maxPatience = rand.Next(80, 101);
        _patience = new Percentage(_maxPatience);
    }

    private static IProduct[] GenerateProducts()
    {
        Random rand = new();
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
        if (!_inventory.Any(p => p.Name == product.Name))
        {
            return new VendorOffer()
            {
                Status = OfferStatus.Stopped,
                Product = product,
                Price = 0,
                OfferedBy = PersonType.Vendor
            };
        }

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
        // Check if the customer increased their offer compared to the last one
        // Capture the previous customer price before updating so percentage
        // calculations use the customer's change, not the vendor's last price.
        decimal prevCustomerPrice = _lastCustomerPrice;
        bool customerIncreased = offer.Price > prevCustomerPrice;
        _lastCustomerPrice = offer.Price;

        decimal customerPrice = offer.Price;
        decimal estPrice = GetEstimatedPrice(offer.Product, customer);
        int idx = _inventory.FindIndex(p => p.Name == offer.Product.Name);

        offer.OfferedBy = PersonType.Vendor;

        if (_patience.Value == 0 || idx == -1)
        {
            offer.Status = OfferStatus.Stopped;
            _pastOffers.Add(offer);
            StopTrade();
            return offer;
        }
        if (customerPrice > estPrice * 1.3m)
        {
            offer.Status = OfferStatus.Accepted;
            _patience.Value -= _patienceDecreaseOnOffer;
            _pastOffers.Add(offer);
            StopTrade();
            return offer;
        }

        decimal counterPrice;

        if (_pastOffers.Count == 0)
        {
            counterPrice = estPrice * 0.8m + customerPrice * 0.2m;
        }
        else
        {
            var lastVendorOffer = _pastOffers[^1];
            decimal lastVendorPrice = lastVendorOffer.Price;

            if (customerIncreased)
            {
                // Compute percent increase of the customer's offer relative to
                // their previous offer. Using the vendor's last price here caused
                // inverted behavior when the customer increased but was still
                // below the vendor's last asking price.
                decimal pIncrease = prevCustomerPrice == 0 ? 0 : (customerPrice - prevCustomerPrice) / prevCustomerPrice; // 0.10 = 10%
                counterPrice = lastVendorPrice * (1m - pIncrease);
            }
            else
            {   //if the customer is not ready to go any higher, we dont need to go any lower
                offer.Status = OfferStatus.Stopped;
                _pastOffers.Add(offer);
                StopTrade();
                return offer;
            }
        }

        offer.Price = decimal.Round(counterPrice, 2, MidpointRounding.AwayFromZero);
        // Accept if counter-offer matches customer offer
        if (offer.Price == customerPrice)
        {
            offer.Status = OfferStatus.Accepted;
            _patience.Value -= _patienceDecreaseOnOffer;
            _pastOffers.Add(offer);
            StopTrade();
            return offer;
        }
        offer.Status = OfferStatus.Ongoing;

        _patience.Value -= _patienceDecreaseOnOffer;
        if (customerPrice < estPrice * 0.5m)
        {
            _patience.Value -= _patienceDecreaseOnBigUndershoot;
        }
        else if (customerPrice < estPrice * 0.8m)
        {
            _patience.Value -= _patienceDecreaseOnUndershoot;
        }
        _pastOffers.Add(offer);

        return offer;
    }

    public void StopTrade()
    {
        this._pastOffers.Clear();
        this._patience.Value = this._maxPatience;
        this._lastCustomerPrice = 0;
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

public class VendorFactory : IVendorFactory
{
    public static IVendor CreateVendor(string name, int age) => new Vendor(name, age);
}
