using System;

namespace Hotel.Server.Hotels;

public class HotelAddRequest
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public double GeoLat { get; set; }
    public double GeoLng { get; set; }
}
