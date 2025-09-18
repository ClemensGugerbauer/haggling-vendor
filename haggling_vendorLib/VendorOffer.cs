using haggling_interfaces;
using System;

public class VendorOffer : IOffer
{
    public OfferStatus Status { get; set; }
    public IProduct Product { get; set; } = null!;
    public decimal Price { get; set; }
    public PersonType OfferedBy { get; set; }
}
