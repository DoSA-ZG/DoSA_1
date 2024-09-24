using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Address
{
    public int Id { get; set; }

    public string City { get; set; }

    public int PostalCode { get; set; }

    public string Street { get; set; }

    public int Number { get; set; }

    public virtual ICollection<Person> People { get; set; } = new List<Person>();
}
