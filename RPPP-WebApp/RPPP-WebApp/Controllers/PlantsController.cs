using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Extensions.Selectors;
using RPPP_WebApp.Models;
using RPPP_WebApp.ViewModels;
using System.Text.Json;
using System.Linq.Expressions;

namespace RPPP_WebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class PlantsController : ControllerBase, CommonController<int, PlantViewModel>
{
  private readonly Rppp12Context ctx;
 
  public PlantsController(Rppp12Context ctx, IOptionsSnapshot<AppSettings> options)
  {
    this.ctx = ctx;
  }

    [HttpGet(Name = "GetPlants")]
    public async Task<List<PlantViewModel>> GetAll([FromQuery] string? filter, [FromQuery] int? page, [FromQuery] int? itemsPerPage) 
    {
      var query = ctx.Plants.AsQueryable();

      if (!string.IsNullOrWhiteSpace(filter))
      {
        query = query.Where(m => m.Species.Name.Contains(filter));
      }

      IQueryable<PlantViewModel> list = query.OrderBy(m => m.Id)
                          .Select(m => new PlantViewModel
                          {
                              PlantId = m.Id,
                              Quantity = m.Quantity,
                              PurposeId = m.PurposeId,
                              SpeciesId = m.SpeciesId,
                              PlotId = m.PlotId,
                              Name = m.Species.Name,
                              NutritionalValues = m.Species.NutritionalValues,
                              PurposeName = m.Purpose.Name
                          });

      if (page > 0 && itemsPerPage > 0) {
          list = list
              .Skip(((int)page - 1) * (int)itemsPerPage)
              .Take((int)itemsPerPage);
      }
      return list.ToList();
    }

  [HttpPost(Name="addPlant")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Create(PlantViewModel model)
  {
    Plant plant = new Plant();
    plant.Id = await ctx.Plants.MaxAsync(p => p.Id) + 1;
    plant.Quantity = model.Quantity;
    plant.SpeciesId = model.SpeciesId;
    plant.PurposeId = model.PurposeId;
    plant.PlotId = model.PlotId;

    ctx.Add(plant);
    await ctx.SaveChangesAsync();

    var addedPlant = await Get(plant.Id);

    return CreatedAtAction(nameof(Get), new { id = plant.Id }, addedPlant.Value);
  }

    [HttpPut("{id}", Name = "UpdatePlant")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, PlantViewModel model)
    {
      if (model.PlantId != id) //ModelState.IsValid i model != null are automatically evalued because of [ApiController]
      {
        return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.PlantId}");
      }
      else
      {
        var plant = await ctx.Plants.FindAsync(id);
        if (plant == null)
        { 
          return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
        }

        plant.Quantity = model.Quantity;
        plant.PlotId = model.PlotId;
        plant.PurposeId = model.PurposeId;
        plant.SpeciesId = model.SpeciesId;

        await ctx.SaveChangesAsync();
        return NoContent();
      }
    }

    [HttpDelete("{id}", Name = "DeletePlant")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
      var plant = await ctx.Plants.FindAsync(id);
      if (plant == null)
      {
        return NotFound();
      }
      else
      {
        ctx.Remove(plant);
        await ctx.SaveChangesAsync();
        return NoContent();
      };     
    }

    [HttpGet("{id}", Name = "GetPlantById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlantViewModel>> Get(int id)
    {   
      var plant = await ctx.Plants                            
                          .Where(m => m.Id == id)
                          .Select(m => new PlantViewModel {
                            PlantId = m.Id,
                            Quantity = m.Quantity,
                            PurposeId = m.PurposeId,
                            SpeciesId = m.SpeciesId,
                            PlotId = m.PlotId,
                            Name = m.Species.Name,
                            NutritionalValues = m.Species.NutritionalValues,
                            PurposeName = m.Purpose.Name
                          })
                          .FirstOrDefaultAsync();
      if (plant == null)
      {      
        return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
      }
      else
      {
        return plant;
      }
    }
}