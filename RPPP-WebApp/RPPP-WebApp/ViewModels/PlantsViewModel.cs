using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;
public class PlantsViewModel
{
  public IEnumerable<PlantViewModel> Plants { get; set; }
  public PagingInfo PagingInfo { get; set; }
}