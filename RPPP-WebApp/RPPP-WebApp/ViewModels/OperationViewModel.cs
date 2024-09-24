using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.ViewModels;

public class OperationViewModel
{

  public int OperationId { get; set; }

  public int PlantId { get; set; }
  public int OperationTypeId { get; set; }
  public string OperationTypeName { get; set; }
  public int RequestId { get; set; }

  public int Cost { get; set; }
  public int Amount { get; set; }
  public DateTime Date { get; set; }
  public bool Status { get; set; }
}