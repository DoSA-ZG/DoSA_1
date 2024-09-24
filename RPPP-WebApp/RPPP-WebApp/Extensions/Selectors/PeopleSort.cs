using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Extensions.Selectors;

public static class PeopleSort
{
  public static IQueryable<Person> ApplySort(this IQueryable<Person> query, int sort, bool ascending)
  {
    System.Linq.Expressions.Expression<Func<Person, object>> orderSelector = null;
    switch (sort)
    {
      case 1:
        orderSelector = p => p.Id;
        break;
      case 2:
        orderSelector = p => p.Name;
        break;
      case 3:
        orderSelector = p => p.PhoneNumber;
        break;
      case 4:
        orderSelector = p => p.Email;
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