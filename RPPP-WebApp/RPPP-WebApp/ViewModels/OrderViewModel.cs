using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.ViewModels;

public class OrderViewModel
{

  public int OrderId { get; set; }

  public int CustomerId { get; set; }
  public DateTime Date { get; set; }
}