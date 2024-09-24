using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Extensions.Selectors;

public static class SpeciessSort
{
  public static IQueryable<Species> ApplySort(this IQueryable<Species> query, int sort, bool ascending)
  {
    System.Linq.Expressions.Expression<Func<Species, object>> orderSelector = null;
    switch (sort)
    {
      case 1:
        orderSelector = p => p.Id;
        break;
      case 2:
        orderSelector = p => p.Name;
        break;
      case 3:
        orderSelector = p => p.NutritionalValues;
        break;
    }
    if (orderSelector != null)
    {
      query = ascending ?
             query.OrderBy(orderSelector) :
             query.OrderByDescending(orderSelector);
    }

    return query;
  }
}