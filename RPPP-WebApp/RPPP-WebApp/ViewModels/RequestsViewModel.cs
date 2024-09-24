using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;
public class RequestsViewModel
{
  public IEnumerable<RequestViewModel> Requests { get; set; }
  public PagingInfo PagingInfo { get; set; }
}