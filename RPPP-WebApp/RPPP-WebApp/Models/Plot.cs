using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RPPP_WebApp.Models;

public partial class Plot
{
    [Display(Name = "Plot id")] public int Id { get; set; }

    [Display(Name = "Soil type")] public int SoilId { get; set; }

    [Display(Name = "Owner")] public int OwnerId { get; set; }

    [Display(Name = "Size")] public int Size { get; set; }

    [Display(Name = "Light intensity")] public int LightIntensity { get; set; }

    [Display(Name = "Coordinate X")] public double CoordX { get; set; }

    [Display(Name = "Coordinate Y")] public double CoordY { get; set; }

    [JsonIgnore] public virtual ICollection<Infrastructure> Infrastructures { get; set; } = new List<Infrastructure>();

    [JsonIgnore] public virtual ICollection<Leasing> Leasings { get; set; } = new List<Leasing>();

    [JsonIgnore] public virtual Person Owner { get; set; }

    [JsonIgnore] public virtual ICollection<Plant> Plants { get; set; } = new List<Plant>();

    [JsonIgnore] public virtual SoilType Soil { get; set; }
}
