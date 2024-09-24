using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;

namespace RPPP_WebApp.Controllers;
public class SpeciesController : Controller
{
  private readonly Rppp12Context ctx;
  private readonly AppSettings appData;
 
  public SpeciesController(Rppp12Context ctx, IOptionsSnapshot<AppSettings> options)
  {
    this.ctx = ctx;
    appData = options.Value;
  }

  public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
  {
    int pagesize = appData.PageSize;

    var query = ctx.Species.AsNoTracking();
    int count = await query.CountAsync();

    var pagingInfo = new PagingInfo
    {
      CurrentPage = page,
      Sort = sort,
      Ascending = ascending,
      ItemsPerPage = pagesize,
      TotalItems = count
    };
    if (page < 1 || page > pagingInfo.TotalPages)
    {
      return RedirectToAction(nameof(Index), new { page = 1, sort, ascending });
    }

    query = query.ApplySort(sort, ascending);
   
    var speciesPlural = await query
                        .Select(m => new SpeciesViewModel
                        {
                          SpeciesId = m.Id,
                          Name = m.Name,
                          NutritionalValues = m.NutritionalValues
                        })
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToListAsync();
    var model = new SpeciesPluralViewModel
    {
      Species = speciesPlural,
      PagingInfo = pagingInfo
    };

    return View(model);
  }   

  [HttpGet]
  public async Task<IActionResult> Create()
  {
    await PrepareDropDownLists();
    return View();
  }

  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(Species species)
  {
    TempData[Constants.ErrorOccurred] = true;
    if (ModelState.IsValid)
    {
      try
      {
        ctx.Add(species);
        await ctx.SaveChangesAsync();

        TempData[Constants.Message] = $"Species {species.Name} has been added with id = {species.Id}";
        TempData[Constants.ErrorOccurred] = false;
        return RedirectToAction(nameof(Index));

      }
      catch (Exception exc)
      {
        TempData[Constants.Message] = exc.CompleteExceptionMessage();
        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
        await PrepareDropDownLists();
        return View(species);
      }
    }
    else
    {
      await PrepareDropDownLists();
      return View(species);
    }
  }

  private async Task PrepareDropDownLists()
  {
    var hr = await ctx.Plants                  
                      .Where(d => d.PurposeId == 1)
                      .Select(d => new { d.Id, d.SpeciesId })
                      .FirstOrDefaultAsync();
    var countries = await ctx.Plants
                          .Where(d => d.PurposeId != 1)
                          .OrderBy(d => d.Id)
                          .Select(d => new { d.Id, d.SpeciesId })
                          .ToListAsync();
    if (hr != null)
    {
      countries.Insert(0, hr);
    }      
    ViewBag.Countries = new SelectList(countries, nameof(hr.Id), nameof(hr.SpeciesId));
  }
  

  #region Methods for dynamic update and delete
  [HttpPost]
  public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true)
  {
    ActionResponseMessage responseMessage;
    var species = await ctx.Species.FindAsync(id);          
    if (species != null)
    {
      try
      {
        string name = species.Name;
        ctx.Remove(species);
        await ctx.SaveChangesAsync();
        responseMessage = new ActionResponseMessage(MessageType.Success, $"Species {name} with id {id} has been deleted.");          
      }
      catch (Exception exc)
      {          
        responseMessage = new ActionResponseMessage(MessageType.Error, $"Error deleting species: {exc.CompleteExceptionMessage()}");
      }
    }
    else
    {
      responseMessage = new ActionResponseMessage(MessageType.Error, $"Species with id {id} does not exist.");
    }

    TempData[Constants.Message] = responseMessage.Message;
    TempData[Constants.ErrorOccurred] = responseMessage.MessageType == MessageType.Error;

    Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
    return RedirectToAction(nameof(Index), new { page, sort, ascending });
  }

  [HttpGet]
  public async Task<IActionResult> Edit(int id)
  {
    var species = await ctx.Species
                          .AsNoTracking()
                          .Where(m => m.Id == id)
                          .SingleOrDefaultAsync();
    if (species != null)
    {        
      await PrepareDropDownLists();
      return View(species);
    }
    else
    {
      return NotFound($"Invalid species id: {id}");
    }
  }

  [HttpPost]    
  public async Task<IActionResult> Edit(Species species)
  {
    TempData[Constants.ErrorOccurred] = true;

    if (species == null)
    {
      TempData[Constants.Message] = "No data submitted!?";
      return RedirectToAction(nameof(Index));
    }

    bool checkId = await ctx.Species.AnyAsync(m => m.Id == species.Id);
    if (!checkId)
    {
      TempData[Constants.Message] = $"Invalid species id: {species?.Id}";
      return RedirectToAction(nameof(Index));
    }

    if (ModelState.IsValid)
    {
      try
      {
        ctx.Update(species);
        await ctx.SaveChangesAsync();
        TempData[Constants.Message] = $"Species {species.Name} with id {species.Id} has been updated.";
        TempData[Constants.ErrorOccurred] = false;
        return RedirectToAction(nameof(Index), new { id = species.Id });
      }
      catch (Exception exc)
      {
        TempData[Constants.Message] = exc.CompleteExceptionMessage();
        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
        await PrepareDropDownLists();
        return View(species);
      }
    }
    else
    {
      TempData[Constants.ErrorOccurred] = false;
      await PrepareDropDownLists();
      return View(species);
    }
  }

  [HttpGet]
  public async Task<IActionResult> Get(int id)
  {
    var species = await ctx.Species
                        .Where(m => m.Id == id)
                        .Select(m => new SpeciesViewModel
                        {
                          SpeciesId = m.Id,
                          Name = m.Name,
                          NutritionalValues = m.NutritionalValues
                        })
                        .SingleOrDefaultAsync();
    if (species != null)
    {
      return PartialView(species);
    }
    else
    {
      return NotFound($"Invalid species id: {id}");
    }
  }
  #endregion
}