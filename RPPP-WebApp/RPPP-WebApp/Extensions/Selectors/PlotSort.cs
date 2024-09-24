using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Extensions.Selectors;

public static class PlotSort
{
  public static IQueryable<Plot> ApplySort(this IQueryable<Plot> query, int sort, bool ascending)
  {
    System.Linq.Expressions.Expression<Func<Plot, object>> orderSelector = sort switch {
      1 => p => p.Id,
      2 => p => p.Size,
      3 => p => p.LightIntensity,
      4 => p => p.CoordX,
      5 => p => p.CoordY,
      6 => p => p.Soil.Name,
      7 => p => p.Owner.Name,
      _ => null
    };
    
    if (orderSelector != null)
    {
      query = ascending ?
             query.OrderBy(orderSelector) :
             query.OrderByDescending(orderSelector);
    }

    return query;
  }
}