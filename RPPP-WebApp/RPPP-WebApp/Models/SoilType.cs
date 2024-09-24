using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class SoilType
{
    public int Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Plot> Plots { get; set; } = new List<Plot>();
}
