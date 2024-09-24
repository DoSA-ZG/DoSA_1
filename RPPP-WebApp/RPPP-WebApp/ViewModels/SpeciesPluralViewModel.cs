using RPPP_WebApp.ViewModels;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.ViewModels;

public class SpeciesPluralViewModel
  {
      public IEnumerable<SpeciesViewModel> Species { get; set; }
      public PagingInfo PagingInfo { get; set; }
  }