using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Hotel.Server.Helpers;

namespace Hotel.Server.Users;

public class User : IHasPassword
{
    public static readonly User SYSTEM_USER = new User
    {
        Id = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        Email = "admin@hoteltest.hr",
        Password = "AQAAAAEAACcQAAAAENLAaKQ9RTKMDD6R6isiYgPSzuk/urB/co49UGgZ4RfDTMYXZncKNBNEry2wwNM/wQ==",
        FirstName = "Hotel",
        LastName = "Admin",
        Role = "admin"
    };

    public Guid Id { get; set; } = IdProvider.NewId();

    [Required, EmailAddress, QuickSearchable]
    public string Email { get; set; }

    [Required] public string Password { get; set; }
    [Required, QuickSearchable] public string FirstName { get; set; }
    [Required, QuickSearchable] public string LastName { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public string Role { get; set; }
    public string ActivationCode { get; set; }
    public string PasswordResetCode { get; set; }
}
