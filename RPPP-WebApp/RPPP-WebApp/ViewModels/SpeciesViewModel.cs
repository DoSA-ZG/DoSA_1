using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.ViewModels;

public class SpeciesViewModel
{
  public int SpeciesId { get; set; }
  public string Name { get; set; }
  public string NutritionalValues { get; set; }
}