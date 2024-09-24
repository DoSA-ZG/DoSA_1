using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Plant
{
    public int Id { get; set; }

    public int SpeciesId { get; set; }

    public int PurposeId { get; set; }

    public int PlotId { get; set; }

    public int Quantity { get; set; }

    public virtual ICollection<Operation> Operations { get; set; } = new List<Operation>();

    public virtual Plot Plot { get; set; }

    public virtual Purpose Purpose { get; set; }

    public virtual Species Species { get; set; }
}
