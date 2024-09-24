using System;
using System.Collections.Generic;

namespace RPPP_WebApp.Models;

public partial class Species
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string NutritionalValues { get; set; }

    public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();

    public virtual ICollection<Request> Requests { get; set; } = new List<Request>();
}
