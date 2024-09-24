using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;
public class PlotsViewModel
{
  public IEnumerable<PlotViewModel> Plots { get; set; }
  public PagingInfo PagingInfo { get; set; }
}