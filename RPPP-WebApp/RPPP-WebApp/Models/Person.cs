using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Person
{
    public int Id { get; set; }

    public int AddressId { get; set; }

    public int RoleId { get; set; }

    public string Name { get; set; }

    public string PhoneNumber { get; set; }

    public string Email { get; set; }

    public virtual Address Address { get; set; }

    public virtual ICollection<Leasing> LeasingOwners { get; set; } = new List<Leasing>();

    public virtual ICollection<Leasing> LeasingRentiers { get; set; } = new List<Leasing>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Plot> Plots { get; set; } = new List<Plot>();

    public virtual Role Role { get; set; }
}
