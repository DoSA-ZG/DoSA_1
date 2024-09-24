using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace RPPP_WebApp.ViewModels;

public class AutoCompletePlants
{

  [JsonPropertyName("id")]
  public int SpeciesId { get; set; }

  [JsonPropertyName("label")]
  public string Label { get; set; }

  [JsonPropertyName("nutritional_values")]
  public string NutritionalValues { get; set; }

  public AutoCompletePlants() { }
  public AutoCompletePlants(int id, string label, string nutritionalValues)
  {
    SpeciesId = id;
    Label = label;
    NutritionalValues = nutritionalValues;
  }

}