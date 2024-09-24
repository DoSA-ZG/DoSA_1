using System.Text.Json.Serialization;

namespace RPPP_WebApp.ViewModels;

public class RequestViewModel
{
  public int RequestId { get; set; }
  public int NeededSpeciesId { get; set; }
  public int OrderId { get; set; }
  public string NeededSpecies_Name {get; set; }
  public string NeededSpecies_NutritionalValues {get; set; }
  public int Amount { get; set; }
  [JsonIgnore] public ICollection<RPPP_WebApp.ViewModels.OperationViewModel> Operations { get; set; }

  public RequestViewModel() {
    this.Operations = new List<RPPP_WebApp.ViewModels.OperationViewModel>();
  }
}