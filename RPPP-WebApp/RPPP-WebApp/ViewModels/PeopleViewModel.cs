using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;

public class PeopleViewModel
{
  public IEnumerable<PersonViewModel> People { get; set; }
  public PagingInfo PagingInfo { get; set; }
}