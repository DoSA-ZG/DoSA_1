using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Extensions.Selectors;

public static class RequestsSort
{
  public static IQueryable<Request> ApplySort(this IQueryable<Request> query, int sort, bool ascending)
  {
    System.Linq.Expressions.Expression<Func<Request, object>> orderSelector = null;
    switch (sort)
    {
      case 1:
        orderSelector = p => p.Id;
        break;
      case 2:
        orderSelector = p => p.Amount;
        break;
      case 3:
        orderSelector = p => p.NeededSpeciesId;
        break;
      case 4:
        orderSelector = p => p.OrderId;
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