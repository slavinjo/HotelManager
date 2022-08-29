using System;
using System.ComponentModel.DataAnnotations.Schema;
using Hotel.Server.Data;
using Hotel.Server.Helpers;

namespace Hotel.Server.Hotels;

public class Hotel : UserChangeTracked
{
    public Guid Id { get; set; } = IdProvider.NewId();
    [QuickSearchable] public string Name { get; set; }
    public decimal Price { get; set; }
    public double GeoLat { get; set; }
    public double GeoLng { get; set; }

    [NotMapped] public double Distance { get; set; }
}
