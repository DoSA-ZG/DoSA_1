using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RPPP_WebApp.ViewModels;

public class PlotViewModel
{
  public int PlotId { get; set; }
  [Display(Name = "Size")] public int Size { get; set; }
  [Display(Name = "Light intensity")] public int LightIntensity { get; set; }
  [Display(Name = "Coordinate X")] public double CoordX {get; set; }
  [Display(Name = "Coordinate Y")]public double CoordY {get; set; }
  public string SoilName { get; set; }
  public string OwnerName { get; set; }

  [Display(Name = "Soil type")] public int SoilId { get; set; }

  [Display(Name = "Owner")] public int OwnerId { get; set; }

  [JsonIgnore] public ICollection<RPPP_WebApp.Models.Infrastructure> Infrastructures { get; set; }
  [JsonIgnore] public ICollection<RPPP_WebApp.Models.Leasing> Leasings { get; set; }
  [JsonIgnore] public IEnumerable<RPPP_WebApp.ViewModels.PlantViewModel> Plants { get; set; }

  public PlotViewModel() {
    this.Plants = new List<RPPP_WebApp.ViewModels.PlantViewModel>();
  }
}