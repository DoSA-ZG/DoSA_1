using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.ViewModels;

public class PlantViewModel
{

  public int? PlantId { get; set; }

  public int SpeciesId { get; set; }

  [Display(Name = "Species name")]
  public string Name { get; set; }

  [Display(Name = "Nutritional values")]
  public string NutritionalValues { get; set; }

  [Display(Name = "Purpose")]
  public string PurposeName { get; set; }

  public int PurposeId { get; set; }

  public int PlotId { get; set; }

  [Display(Name = "Quantity")]
  [Range(0, int.MaxValue, ErrorMessage = "The quantity must be positive")]
  public int Quantity { get; set; }  
}