using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime Date { get; set; }

    public virtual Person Customer { get; set; }

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
