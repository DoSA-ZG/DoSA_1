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
    /// Web API service for Orders CRUD operations
    /// </summary>
[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase, CommonController<int, OrderViewModel>
{
  private readonly Rppp12Context ctx;
    /// <summary>
    /// Create an instance of the controller 
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="options"> </param>
  public OrderController(Rppp12Context ctx, IOptionsSnapshot<AppSettings> options)
  {
    this.ctx = ctx;
  }
    /// <summary>
    /// Get a list of Orders with pagination
    /// </summary>
    /// <param name="filter"></param>
    /// <param name="page">Page number</param>
    /// <param name="itemsPerPage">number of items per page</param>
    /// <returns>A paginated list of operations</returns>
    
    [HttpGet(Name = "GetOrders")]
    public async Task<List<OrderViewModel>> GetAll([FromQuery] string filter, [FromQuery] int? page, [FromQuery] int? itemsPerPage) 
    {
      var query = ctx.Orders.AsQueryable();

      if (!string.IsNullOrWhiteSpace(filter))
      {
        query = query.Where(m => m.Customer.Name.Contains(filter));
      }

      IQueryable<OrderViewModel> list = query.OrderBy(m => m.Id)
                          .Select(m => new OrderViewModel
                          {
                              OrderId = m.Id,
                              CustomerId = m.CustomerId,
                              Date = m.Date,
                          });

      if (page > 0 && itemsPerPage > 0) {
          list = list
              .Skip(((int)page - 1) * (int)itemsPerPage)
              .Take((int)itemsPerPage);
      }
      return list.ToList();
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    /// <param name="model"> an istance of OrderViewModel</param>

  [HttpPost(Name="addOrder")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Create(OrderViewModel model)
  {
    Order order = new Order();
    order.Id = await ctx.Orders.MaxAsync(p => p.Id) + 1;
    order.CustomerId = model.CustomerId;
    order.Date = model.Date;

    ctx.Add(order);
    await ctx.SaveChangesAsync();

    var addedOrder = await Get(order.Id);

    return CreatedAtAction(nameof(Get), new { id = order.Id }, addedOrder.Value);
  }
    /// <summary>
    /// update existing order
    /// </summary>
    /// <param name="id"> id of order to be updated </param>
    /// <param name="model"> an istance of OrderViewModel</param>

    [HttpPut("{id}", Name = "UpdateOrder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, OrderViewModel model)
    {
      if (model.OrderId != id) //ModelState.IsValid i model != null are automatically evalued because of [ApiController]
      {
        return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.OrderId}");
      }
      else
      {
        var order = await ctx.Orders.FindAsync(id);
        if (order == null)
        { 
          return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
        }

        order.CustomerId = model.CustomerId;
        order.Date = model.Date;

        await ctx.SaveChangesAsync();
        return NoContent();
      }
    }
    /// <summary>
    /// Delete an order
    /// </summary>
    /// <param name="id"> id of order to be deleted </param>

    [HttpDelete("{id}", Name = "DeleteOrder")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
      var order = await ctx.Orders.FindAsync(id);
      if (order == null)
      {
        return NotFound();
      }
      else
      {
        ctx.Remove(order);
        await ctx.SaveChangesAsync();
        return NoContent();
      };     
    }

    /// <summary>
    /// select a single order based on it's id
    /// </summary>
    /// <param name="id"> id of order to be selected </param> 

    [HttpGet("{id}", Name = "GetOrderById")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderViewModel>> Get(int id)
    {   
      var order = await ctx.Orders                            
                          .Where(m => m.Id == id)
                          .Select(m => new OrderViewModel {
                            OrderId = m.Id,
                            CustomerId = m.CustomerId,
                            Date = m.Date
                          })
                          .FirstOrDefaultAsync();
      if (order == null)
      {      
        return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
      }
      else
      {
        return order;
      }
    }
}