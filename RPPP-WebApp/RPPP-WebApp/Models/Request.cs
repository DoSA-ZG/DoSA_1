using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Request
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int NeededSpeciesId { get; set; }

    public int Amount { get; set; }

    public virtual Species NeededSpecies { get; set; }

    public virtual ICollection<Operation> Operations { get; set; } = new List<Operation>();

    public virtual Order Order { get; set; }
}
