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
    /// <summary>
    /// Web API service for Operations CRUD operations
    /// </summary>
[ApiController]
[Route("[controller]")]
public class OperationController : ControllerBase, CommonController<int, OperationViewModel>
{
  private readonly Rppp12Context ctx;
    /// <summary>
    /// Create an instance of the controller 
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="options"> </param>
  public OperationController(Rppp12Context ctx, IOptionsSnapshot<AppSettings> options)
  {
    this.ctx = ctx;
  }
    /// <summary>
    /// Get a list of Operations with pagination 
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="page">Page number</param>
    /// <param name="itemsPerPage">number of elements per page</param>
    /// <returns>A paginated list of operations</returns>
    
    [HttpGet(Name = "GetOperations")]
    public async Task<List<OperationViewModel>> GetAll([FromQuery] string? filter, [FromQuery] int? page, [FromQuery] int? itemsPerPage) 
    {
      var query = ctx.Operations.AsQueryable();

      if (!string.IsNullOrWhiteSpace(filter))
      {
        query = query.Where(m => m.Plant.Species.Name.Contains(filter));
      }

      IQueryable<OperationViewModel> list = query.OrderBy(m => m.Id)
                          .Select(m => new OperationViewModel
                          {
                              OperationId = m.Id,
                              Cost = m.Cost,
                              Date = m.Date,
                              OperationTypeId = m.OperationTypeId,
                              OperationTypeName = m.OperationType.Name,
                              Amount = m.Amount,
                              PlantId = m.PlantId,
                              RequestId = m.RequestId,
                              Status = m.Status,
                          });

      if (page > 0 && itemsPerPage > 0) {
          list = list
              .Skip(((int)page - 1) * (int)itemsPerPage)
              .Take((int)itemsPerPage);
      }
      return list.ToList();
    }
    /// <summary>
    /// Create a new operation
    /// </summary>
    /// <param name="model"> an istance of OperationViewModel</param>
    
  [HttpPost(Name="addOperation")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Create(OperationViewModel model)
  {
    Operation operation = new Operation();
    operation.Id = await ctx.Operations.MaxAsync(p => p.Id) + 1;
    operation.Status = model.Status;
    operation.Date = model.Date;
    operation.OperationTypeId = model.OperationTypeId;
    operation.PlantId = model.PlantId;
    operation.RequestId = model.RequestId;

    ctx.Add(operation);
    await ctx.SaveChangesAsync();

    var addedOperation = await Get(operation.Id);

    return CreatedAtAction(nameof(Get), new { id = operation.Id }, addedOperation.Value);
  }

    /// <summary>
    /// update existing operation
    /// </summary>
    /// <param name="id"> id of operation to be updated </param>
    /// <param name="model"> an istance of OperationViewModel</param>

    [HttpPut("{id}", Name = "UpdateOperation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, OperationViewModel model)
    {
      if (model.OperationId != id) //ModelState.IsValid i model != null are automatically evalued because of [ApiController]
      {
        return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.OperationId}");
      }
      else
      {
        var operation = await ctx.Operations.FindAsync(id);
        if (operation == null)
        { 
          return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
        }

        operation.Status = model.Status;
        operation.Date = model.Date;
        operation.OperationTypeId = model.OperationTypeId;
        operation.Amount = model.Amount;
        operation.Cost = model.Cost;

        await ctx.SaveChangesAsync();
        return NoContent();
      }
    }
    /// <summary>
    /// Delete an operation
    /// </summary>
    /// <param name="id"> id of operation to be deleted </param>

    [HttpDelete("{id}", Name = "DeleteOperation")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
      var operation = await ctx.Operations.FindAsync(id);
      if (operation == null)
      {
        return NotFound();
      }
      else
      {
        ctx.Remove(operation);
        await ctx.SaveChangesAsync();
        return NoContent();
      };     
    }
    /// <summary>
    /// select a single operation based on it's id
    /// </summary>
    /// <param name="id"> id of operation to be selected </param> 

    [HttpGet("{id}", Name = "GetOperationById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OperationViewModel>> Get(int id)
    {   
      var operation = await ctx.Operations                            
                          .Where(m => m.Id == id)
                          .Select(m => new OperationViewModel {
                            OperationId = m.Id,
                            Cost = m.Cost,
                            Date = m.Date,
                            OperationTypeId = m.OperationTypeId,
                            OperationTypeName = m.OperationType.Name,
                            Amount = m.Amount,
                            PlantId = m.PlantId,
                            RequestId = m.RequestId,
                            Status = m.Status,
                          })
                          .FirstOrDefaultAsync();
      if (operation == null)
      {      
        return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
      }
      else
      {
        return operation;
      }
    }
}