using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;

namespace RPPP_WebApp.Controllers;

public class AutoCompleteController : Controller
{
  private readonly Rppp12Context ctx;
  private readonly AppSettings appData;

  public AutoCompleteController(Rppp12Context ctx, IOptionsSnapshot<AppSettings> options)
  {
    this.ctx = ctx;
    appData = options.Value;
  }

  public async Task<List<AutoCompletePlants>> Plants(string term)
  {
    var query = ctx.Species
                   .Where(s => s.Name.Contains(term))
                   .OrderBy(s => s.Name)
                   .Select(s => new AutoCompletePlants
                   {
                       SpeciesId = s.Id,
                       Label = s.Name,
                       NutritionalValues = s.NutritionalValues
                   });

    var list = await query.OrderBy(l => l.Label)
                          .ThenBy(l => l.SpeciesId)
                          .Take(appData.AutoCompleteCount)
                          .ToListAsync();
    return list;
  }

}
