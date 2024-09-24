using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Purpose
{
    public int Id { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();
}
