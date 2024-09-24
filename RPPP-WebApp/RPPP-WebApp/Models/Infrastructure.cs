using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Infrastructure
{
    public int Id { get; set; }

    public int PlotId { get; set; }

    public string Name { get; set; }

    public string Condition { get; set; }

    public double CoordX { get; set; }

    public double CoordY { get; set; }

    public virtual Plot Plot { get; set; }
}
