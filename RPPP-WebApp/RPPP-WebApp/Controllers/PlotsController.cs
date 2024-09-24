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

[ApiController]
[Route("[controller]")]
public class PlotsController : Controller
{
  private readonly CommonController<int, PlantViewModel> _plantsController;
  private readonly Rppp12Context ctx;
  private readonly AppSettings appData;
 
  public PlotsController(Rppp12Context ctx, IOptionsSnapshot<AppSettings> options, CommonController<int, PlantViewModel> plantsController)
  {
    this.ctx = ctx;
    appData = options.Value;
    _plantsController = plantsController;
  }

  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet("view")]
  public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
  {
    int pagesize = appData.PageSize;

    var query = ctx.Plots.AsNoTracking();
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
   
    var plots = await query
        .Include(m => m.Infrastructures)
        .Include(m => m.Leasings)
        .Select(m => new PlotViewModel
        {
          PlotId = m.Id,
          SoilName = m.Soil.Name,
          OwnerName = m.Owner.Name,
          CoordX = m.CoordX,
          CoordY = m.CoordY,
          LightIntensity = m.LightIntensity,
          Size = m.Size,
          OwnerId = m.OwnerId,
          SoilId = m.SoilId,
          Leasings = m.Leasings,
          Infrastructures = m.Infrastructures,
        })
        .Skip((page - 1) * pagesize)
        .Take(pagesize)
        .AsSplitQuery()
        .ToListAsync();

    var model = new PlotsViewModel
    {
      Plots = plots,
      PagingInfo = pagingInfo
    };

    return View(model);
  }   

  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet("create")]
  public async Task<IActionResult> Create()
  {
    int maxId = await ctx.Plots.MaxAsync(p => p.Id) + 1;
    var plot = new PlotViewModel
    {
      PlotId = maxId,
      Size = 1
    };
    await PrepareDropDownLists(true);
    return View(plot);
  }

  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpPost("create")]
  public async Task<IActionResult> Create([FromForm] PlotViewModel model)
  {
    TempData[Constants.ErrorOccurred] = true;
    if (ModelState.IsValid)
    {
      try
      {
        var (actionResult, plotId) = await addPlot(model);

        foreach (var plantToAdd in model.Plants) {
          plantToAdd.PlotId = plotId;
          await _plantsController.Create(plantToAdd);
        }

        TempData[Constants.Message] = $"Plot has been added with id = {plotId}";
        TempData[Constants.ErrorOccurred] = false;
        return RedirectToAction(nameof(Edit), new {id = plotId});

      }
      catch (Exception exc)
      {
        TempData[Constants.Message] = exc.CompleteExceptionMessage();
        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
        await PrepareDropDownLists();
        return View(model);
      }
    }
    else
    {
      TempData[Constants.ErrorOccurred] = true;
      await PrepareDropDownLists();
      return View(model);
    }
  }

  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet("edit/{id}")]
  public Task<IActionResult> Edit(int id, int page = 1, int sort = 1, bool ascending = true)
  {
    return ShowDetail(id, page, sort, ascending, viewName: nameof(Edit));
  }


  [ApiExplorerSettings(IgnoreApi = true)] 
  [HttpPost("edit/{id}")]
  public async Task<IActionResult> Edit([FromForm] PlotViewModel model, int page = 1, int sort = 1, bool ascending = true)
  {
    ViewBag.Page = page;
    ViewBag.Sort = sort;
    ViewBag.Ascending = ascending;

    TempData[Constants.ErrorOccurred] = true;
    if (ModelState.IsValid)
    {
      var plot = await ctx.Plots
                      .Include(p => p.Plants)
                      .Where(p => p.Id == model.PlotId)
                      .AsSplitQuery()
                      .FirstOrDefaultAsync();

      if (plot == null)
      {
        TempData[Constants.Message] = $"Invalid plot id: {plot?.Id}";
        return RedirectToAction(nameof(Index));
      }

      plot.OwnerId = model.OwnerId;
      plot.SoilId = model.SoilId;
      plot.CoordX = model.CoordX;
      plot.CoordY = model.CoordY;
      plot.LightIntensity = model.LightIntensity;
      plot.Size = model.Size;
                        
      try
      {
        ctx.Update(plot);
        await ctx.SaveChangesAsync();

        List<int?> plantIds = model.Plants
                                .Where(p => p.PlantId > 0)
                                .Select(p => p.PlantId)
                                .ToList();

        foreach(var onePlant in plot.Plants.Where(i => !plantIds.Contains(i.Id)).ToList()) {
          await _plantsController.Delete(onePlant.Id);
        }

        foreach (var onePlant in model.Plants) {
          onePlant.PlotId = plot.Id;

          if (onePlant.PlantId > 0) await _plantsController.Update(plot.Plants.First(p => p.Id == onePlant.PlantId).Id, onePlant);
          else await _plantsController.Create(onePlant);
        }  

        TempData[Constants.Message] = $"Plot with id {plot.Id} has been updated.";
        TempData[Constants.ErrorOccurred] = false;

        return RedirectToAction(nameof(Edit), new {
          id = plot.Id,
          page,
          sort,
          ascending
        });
      }
      catch (Exception exc)
      {
        TempData[Constants.Message] = exc.CompleteExceptionMessage();
        ModelState.AddModelError(string.Empty, exc.CompleteExceptionMessage());
        await PrepareDropDownLists();
        return View(model);
      }
    }
    else
    {
      var entry = ModelState.FirstOrDefault();
      string propertyName = entry.Key;
      var error = entry.Value.Errors.First();
      TempData[Constants.Message] = ($"Error for '{propertyName}': {error.ErrorMessage}");
      TempData[Constants.ErrorOccurred] = true;
      
      await PrepareDropDownLists(true);
      return View(model);
    }
  }

  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpPost("delete/{id}")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> Delete(int id, int page = 1, int sort = 1, bool ascending = true)
  {
    ActionResponseMessage responseMessage;;          
    try
    {
      await Delete(id);
      responseMessage = new ActionResponseMessage(MessageType.Success, $"Plot with id {id} has been deleted.");          
    }
    catch (Exception exc)
    {          
    responseMessage = new ActionResponseMessage(MessageType.Error, $"Error deleting plot: {exc.CompleteExceptionMessage()}");
    }

    TempData[Constants.Message] = responseMessage.Message;
    TempData[Constants.ErrorOccurred] = responseMessage.MessageType == MessageType.Error;

    Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
    return RedirectToAction(nameof(Index), new { page, sort, ascending });
  }

  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet("detail/{id}")]
  public async Task<IActionResult> ShowDetail(int id, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(ShowDetail))
  {
    var plot = await ctx.Plots
        .Include(m => m.Infrastructures)
        .Include(m => m.Leasings)
        .Include(m => m.Plants).ThenInclude(p => p.Species)
        .Include(m => m.Plants).ThenInclude(p => p.Purpose)
        .Where(m => m.Id == id)
        .Select(m => new PlotViewModel
        {
          PlotId = m.Id,
          SoilName = m.Soil.Name,
          OwnerName = m.Owner.Name,
          CoordX = m.CoordX,
          CoordY = m.CoordY,
          LightIntensity = m.LightIntensity,
          Size = m.Size,
          OwnerId = m.OwnerId,
          SoilId = m.SoilId,
          Leasings = m.Leasings,
          Infrastructures = m.Infrastructures,
        })
        .AsSplitQuery()
        .SingleOrDefaultAsync();
    
    if (plot == null) {
      return NotFound($"Invalid plot id: {id}");
    }

    var plants = await ctx.Plants
                        .Where(pl => pl.PlotId == plot.PlotId)
                        .Select(pl => new PlantViewModel
                        {
                          PlantId = pl.Id,
                          Name = pl.Species.Name,
                          SpeciesId = pl.SpeciesId,
                          Quantity = pl.Quantity,
                          NutritionalValues = pl.Species.NutritionalValues,
                          PurposeName = pl.Purpose.Name,
                          PurposeId = pl.PurposeId
                        })
                        .ToListAsync();

    plot.Plants = plants;

    ViewBag.Page = page;
    ViewBag.Sort = sort;
    ViewBag.Ascending = ascending;

    await PrepareDropDownLists(true);
    return View(viewName, plot);
  }


  [HttpGet(Name = "GetPlots")]
  public async Task<List<PlotViewModel>> GetAll([FromQuery] int? page, [FromQuery] int? itemsPerPage, [FromQuery] int sort = 1, [FromQuery] bool ascending = true) 
  {
    var query = ctx.Plots.AsQueryable();
    query = query.ApplySort(sort, ascending);

    IQueryable<PlotViewModel> list = query.OrderBy(m => m.Id)
                        .Select(m => new PlotViewModel
                        {
                          SoilId = m.SoilId,
                          OwnerId = m.OwnerId,
                          SoilName = m.Soil.Name,
                          OwnerName = m.Owner.Name,
                          CoordX = m.CoordX,
                          CoordY = m.CoordY,
                          Size = m.Size,
                          LightIntensity = m.LightIntensity,
                          PlotId = m.Id,
                        });

    if (page > 0 && itemsPerPage > 0) {
        list = list
            .Skip(((int)page - 1) * (int)itemsPerPage)
            .Take((int)itemsPerPage);
    }
    return list.ToList();
  }

  [HttpPost(Name="addPlot")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<(IActionResult, int Id)> addPlot(PlotViewModel model)
  {
    Plot plot = new Plot();
    plot.Id = await ctx.Plots.MaxAsync(p => p.Id) + 1;
    plot.LightIntensity = model.LightIntensity;
    plot.CoordX = model.CoordX;
    plot.CoordY = model.CoordY;
    plot.OwnerId = model.OwnerId;
    plot.SoilId = model.SoilId;
    plot.Size = model.Size;

    ctx.Add(plot);
    await ctx.SaveChangesAsync();

    var addedPlant = await Get(plot.Id);

    return (CreatedAtAction(nameof(Get), new { id = plot.Id }, addedPlant.Value), plot.Id);
  }

    [HttpGet("{id}", Name = "GetPlotById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlotViewModel>> Get(int id)
    {   
      var plot = await ctx.Plots                            
                          .Where(m => m.Id == id)
                          .Select(m => new PlotViewModel {
                            SoilId = m.SoilId,
                            OwnerId = m.OwnerId,
                            SoilName = m.Soil.Name,
                            OwnerName = m.Owner.Name,
                            CoordX = m.CoordX,
                            CoordY = m.CoordY,
                            Size = m.Size,
                            LightIntensity = m.LightIntensity,
                            PlotId = m.Id,
                          })
                          .FirstOrDefaultAsync();
      if (plot == null)
      {      
        return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
      }
      else
      {
        return plot;
      }
    }

    [HttpPut("{id}", Name = "UpdatePlot")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, PlotViewModel model)
    {
      if (model.PlotId != id) //ModelState.IsValid i model != null are automatically evalued because of [ApiController]
      {
        return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.PlotId}");
      }
      else
      {
        var plot = await ctx.Plots.FindAsync(id);
        if (plot == null)
        { 
          return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
        }

        plot.SoilId = model.SoilId;
        plot.OwnerId = model.OwnerId;
        plot.LightIntensity = model.LightIntensity;
        plot.CoordX = model.CoordX;
        plot.CoordY = model.CoordY;
        plot.Size = model.Size;

        await ctx.SaveChangesAsync();
        return NoContent();
      }
    }

    [HttpDelete("{id}", Name = "DeletePlot")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
      var plot = await ctx.Plots.FindAsync(id);
      if (plot == null)
      {
        return NotFound();
      }
      else
      {
        ctx.Remove(plot);
        await ctx.SaveChangesAsync();
        return NoContent();
      };     
    }

  private async Task PrepareDropDownLists(bool addDetail = false)
  {
    var soilTypes = await ctx.SoilTypes
                          .OrderBy(d => d.Id)
                          .Select(d => new { d.Id, d.Name })
                          .ToListAsync();

    var owners = await ctx.People
                          .OrderBy(d => d.Id)
                          .Select(d => new { d.Id, d.Name })
                          .ToListAsync();

    ViewBag.SoilTypes = new SelectList(soilTypes, nameof(RPPP_WebApp.Models.SoilType.Id), nameof(RPPP_WebApp.Models.SoilType.Name));
    ViewBag.Owner = new SelectList(owners, nameof(RPPP_WebApp.Models.Person.Id), nameof(RPPP_WebApp.Models.Person.Name));

    if (addDetail) await PrepareDetailDropdownList();
  }
  
  private async Task PrepareDetailDropdownList() {
    var purposes = await ctx.Purposes
                          .OrderBy(d => d.Id)
                          .Select(d => new { d.Id, d.Name })
                          .ToListAsync();

    var species = await ctx.Species
                          .OrderBy(d => d.Id)
                          .Select(d => new { d.Id, d.Name })
                          .ToListAsync();


    ViewBag.Purposes = new SelectList(purposes, nameof(RPPP_WebApp.Models.Purpose.Id), nameof(RPPP_WebApp.Models.Purpose.Name));
    ViewBag.Species = new SelectList(species, nameof(RPPP_WebApp.Models.Species.Id), nameof(RPPP_WebApp.Models.Species.Name));
  }
}