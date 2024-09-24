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
  /// <summary>
  /// Web API service for requests CRUD operations
  /// </summary>
[ApiController]
[Route("[controller]")]
public class RequestController : Controller
{
  private readonly CommonController<int, OperationViewModel> _operationsController;
  private readonly Rppp12Context ctx;
  private readonly AppSettings appData;
    /// <summary>
    /// Create an instance of the controller 
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="options"> </param>
    /// <param name="operationsController">controller intializing an instance of detail table viewmodel</param>
  public RequestController(Rppp12Context ctx, IOptionsSnapshot<AppSettings> options,CommonController<int, OperationViewModel> operationsController)
  {
    this.ctx = ctx;
    appData = options.Value;
    _operationsController = operationsController;
  }
    /// <summary>
    /// Get Request to view base information about Request table
    /// </summary>
    /// <param name="page">number of page</param>
    /// <param name="sort">Index of the field to sort by</param>
    /// <param name="ascending">determines whether the sort should be ascending</param>
    /// <returns></returns>
  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet("view")]
  public async Task<IActionResult> Index(int page = 1, int sort = 1, bool ascending = true)
  {
    int pagesize = appData.PageSize;

    var query = ctx.Requests.AsNoTracking();
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
   
    var requests = await query
                        .Select(m => new RequestViewModel
                        {
                          RequestId = m.Id,
                          OrderId = m.OrderId,
                          Amount = m.Amount,
                          NeededSpecies_Name = m.NeededSpecies.Name,
                          NeededSpeciesId = m.NeededSpeciesId,
                          NeededSpecies_NutritionalValues = m.NeededSpecies.NutritionalValues,
                        })
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToListAsync();
    var model = new RequestsViewModel
    {
      Requests = requests,
      PagingInfo = pagingInfo
    }; 

    return View(model);
  }   
  
    /// <summary>
    /// Request to create Request without any additional data besides ID 
    /// </summary>
    /// <returns></returns>
    
  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet("create")]
  public async Task<IActionResult> Create()
  {
    int maxId = await ctx.Requests.MaxAsync(p => p.Id) + 1;
    var request = new RequestViewModel
    {
      RequestId = maxId,
      Amount = 0
    };
    await PrepareDropDownLists(true);
    return View(request);
  }
    /// <summary>
    /// Request to create object Request 
    /// </summary>
    /// <param name="model">instance of RequestViewModel</param>
    /// <returns></returns>

  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpPost("create")]
  public async Task<IActionResult> Create([FromForm] RequestViewModel model)
  {
    TempData[Constants.ErrorOccurred] = true;
    if (ModelState.IsValid)
    {
      try
      {
        var (actionResult, requestId) = await addRequest(model);

        foreach (var operationToAdd in model.Operations) {
          operationToAdd.RequestId = requestId;
          await _operationsController.Create(operationToAdd);
        }

        TempData[Constants.Message] = $"Request has been added with id = {requestId}";
        TempData[Constants.ErrorOccurred] = false;
        return RedirectToAction(nameof(Edit), new {id = requestId});

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
    /// <summary>
    /// Request responsible for whole logic of editing master Request data and detail informations
    /// </summary>
    /// <param name="model">An instance of RequestViewModel</param>
    /// <param name="page">number of page</param>
    /// <param name="sort">Index of the field to sort by</param>
    /// <param name="ascending">determines whether the sort should be ascending</param>
    /// <returns></returns>

  [ApiExplorerSettings(IgnoreApi = true)] 
  [HttpPost("edit/{id}")]
  public async Task<IActionResult> Edit([FromForm] RequestViewModel model, int page = 1, int sort = 1, bool ascending = true)
  {
    ViewBag.Page = page;
    ViewBag.Sort = sort;
    ViewBag.Ascending = ascending;

    TempData[Constants.ErrorOccurred] = true;
    if (ModelState.IsValid)
    {
      var request = await ctx.Requests
                      .Include(p => p.Operations)
                      .Where(p => p.Id == model.RequestId)
                      .AsSplitQuery()
                      .FirstOrDefaultAsync();

      if (request == null)
      {
        TempData[Constants.Message] = $"Invalid request id: {request?.Id}";
        return RedirectToAction(nameof(Index));
      }

      request.Amount = model.Amount;
      request.Id = model.RequestId;
      request.NeededSpeciesId = model.NeededSpeciesId;
                        
      try
      {
        ctx.Update(request);
        await ctx.SaveChangesAsync();

        List<int> operationIds = model.Operations
                                .Where(p => p.OperationId > 0)
                                .Select(p => p.OperationId)
                                .ToList();

        foreach(var oneOperation in request.Operations.Where(i => !operationIds.Contains(i.Id)).ToList()) {
          await _operationsController.Delete(oneOperation.Id);
        }

        foreach (var oneOperation in model.Operations) {
          oneOperation.RequestId = request.Id;

          if (oneOperation.RequestId > 0) await _operationsController.Update(request.Operations.First(p => p.Id == oneOperation.RequestId).Id, oneOperation);
          else await _operationsController.Create(oneOperation);
        }  

        TempData[Constants.Message] = $"Request with id {request.Id} has been updated.";
        TempData[Constants.ErrorOccurred] = false;

        return RedirectToAction(nameof(Edit), new {
          id = request.Id,
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

    /// <summary>
    /// Request to delete Request from database of given id
    /// </summary>
    /// <param name="id"></param>
    /// <param name="page">number of page</param>
    /// <param name="sort">Index of the field to sort by</param>
    /// <param name="ascending">determines whether the sort should be ascending</param>
    /// <returns></returns>

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
      responseMessage = new ActionResponseMessage(MessageType.Success, $"Person with id {id} has been deleted.");          
    }
    catch (Exception exc)
    {          
    responseMessage = new ActionResponseMessage(MessageType.Error, $"Error deleting person: {exc.CompleteExceptionMessage()}");
    }

    TempData[Constants.Message] = responseMessage.Message;
    TempData[Constants.ErrorOccurred] = responseMessage.MessageType == MessageType.Error;

    Response.Headers["HX-Trigger"] = JsonSerializer.Serialize(new { showMessage = responseMessage });
    return RedirectToAction(nameof(Index), new { page, sort, ascending });
  }
    /// <summary>
    /// Request showing additional details about secondary form (detail)
    /// </summary>
    /// <param name="id"></param>
    /// <param name="page"></param>
    /// <param name="sort"></param>
    /// <param name="ascending"></param>
    /// <param name="viewName"></param>
    /// <returns></returns> 

  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet("detail/{id}")]
  public async Task<IActionResult> ShowDetail(int id, int page = 1, int sort = 1, bool ascending = true, string viewName = nameof(ShowDetail))
  {
    var request = await ctx.Requests
        .Include(m => m.Operations)
        .Where(m => m.Id == id)
        .Select(m => new RequestViewModel
        {
          RequestId = m.Id,
          OrderId = m.OrderId,
          NeededSpeciesId = m.NeededSpeciesId,
          Amount = m.Amount,
          NeededSpecies_Name = m.NeededSpecies.Name,
          NeededSpecies_NutritionalValues = m.NeededSpecies.NutritionalValues,
        })
        .AsSplitQuery()
        .SingleOrDefaultAsync();
    
    if (request == null) {
      return NotFound($"Invalid request id: {id}");
    }

    var operations = await ctx.Operations
                        .Where(pl => pl.RequestId == request.RequestId)
                        .Select(pl => new OperationViewModel
                        {
                          OperationId = pl.Id,
                          OperationTypeName = pl.OperationType.Name,
                          PlantId = pl.PlantId,
                          Date = pl.Date,
                          Cost = pl.Cost,
                          Status = pl.Status,
                          RequestId = pl.RequestId,
                          Amount = pl.Amount
                        })
                        .ToListAsync();

    request.Operations = operations;

    ViewBag.Page = page;
    ViewBag.Sort = sort;
    ViewBag.Ascending = ascending;

    await PrepareDropDownLists(true);
    return View(viewName, request);
  }
    /// <summary>
    /// Get a list of Requests with pagination
    /// </summary>
    /// <param name="page">number of page</param>
    /// <param name="itemsPerPage">number of items per page</param>
    /// <param name="sort">Index of the field to sort by</param>
    /// <param name="ascending">determines whether the sort should be ascending</param>
    /// <returns></returns>
    
  [HttpGet(Name = "GetRequest")]
  public async Task<List<RequestViewModel>> GetAll([FromQuery] int? page, [FromQuery] int? itemsPerPage, [FromQuery] int sort = 1, [FromQuery] bool ascending = true) 
  {
    var query = ctx.Requests.AsQueryable();
    query = query.ApplySort(sort, ascending);

    IQueryable<RequestViewModel> list = query.OrderBy(m => m.Id)
                        .Select(m => new RequestViewModel
                        {
                          RequestId = m.Id,
                          NeededSpeciesId = m.NeededSpeciesId,
                          NeededSpecies_NutritionalValues = m.NeededSpecies.NutritionalValues,
                          NeededSpecies_Name = m.NeededSpecies.Name,
                          OrderId = m.OrderId,
                          Amount = m.Amount
                        });

    if (page > 0 && itemsPerPage > 0) {
        list = list
            .Skip(((int)page - 1) * (int)itemsPerPage)
            .Take((int)itemsPerPage);
    }
    return list.ToList();
  }

    /// <summary>
    /// create a new request
    /// </summary>
    /// <param name="model">an instance of RequestViewModel</param>
    /// <returns></returns>
 
  [HttpPost(Name="addRequest")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<(IActionResult, int Id)> addRequest(RequestViewModel model)
  {
    Request request = new Request();
    request.Id = await ctx.Requests.MaxAsync(p => p.Id) + 1;
    request.Amount = model.Amount;
    request.NeededSpeciesId = model.NeededSpeciesId;
    request.OrderId = model.OrderId;

    ctx.Add(request);
    await ctx.SaveChangesAsync();

    var addedRequest = await Get(request.Id);

    return (CreatedAtAction(nameof(Get), new { id = request.Id }, addedRequest.Value), request.Id);
  }
    /// <summary>
    /// select request by it's id
    /// </summary>
    /// <param name="id">id of request to be selected </param>
    /// <returns></returns>

  [HttpGet("{id}", Name = "GetRequestById")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<RequestViewModel>> Get(int id)
  {   
    var request = await ctx.Requests                            
                        .Where(m => m.Id == id)
                        .Select(m => new RequestViewModel {
                          Amount = m.Amount,
                          NeededSpeciesId = m.NeededSpeciesId,
                          NeededSpecies_NutritionalValues = m.NeededSpecies.NutritionalValues,
                          OrderId = m.OrderId,
                          NeededSpecies_Name = m.NeededSpecies.Name,
                        })
                        .FirstOrDefaultAsync();
    if (request == null)
    {      
      return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
    }
    else
    {
      return request;
    }
  }
    /// <summary>
    /// update existing request
    /// </summary>
    /// <param name="id">id of request to be updated</param>
    /// <param name="model">an instance of RefnquestViewModel</param>
    /// <returns></returns>
    
  [HttpPut("{id}", Name = "UpdateRequest")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Update(int id, RequestViewModel model)
  {
    if (model.RequestId != id) //ModelState.IsValid i model != null are automatically evalued because of [ApiController]
    {
      return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.RequestId}");
    }
    else
    {
      var request = await ctx.Requests.FindAsync(id);
      if (request == null)
      { 
        return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
      }

      request.OrderId = model.OrderId;
      request.NeededSpeciesId = model.NeededSpeciesId;
      request.Amount = model.Amount;

      await ctx.SaveChangesAsync();
      return NoContent();
    }
  }
    /// <summary>
    /// Delete a request
    /// </summary>
    /// <param name="id">id of request to be deleted</param>
    /// <returns></returns>

  [HttpDelete("{id}", Name = "DeleteRequest")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> Delete(int id)
  {
    var request = await ctx.Requests.FindAsync(id);
    if (request == null)
    {
      return NotFound();
    }
    else
    {
      ctx.Remove(request);
      await ctx.SaveChangesAsync();
      return NoContent();
    };     
  }
  
  private async Task PrepareDropDownLists(bool addDetail = false)
  {
    var species = await ctx.Species
                          .OrderBy(d => d.Id)
                          .Select(d => new { d.Id, d.Name })
                          .ToListAsync();

    ViewBag.Species = new SelectList(species, nameof(RPPP_WebApp.Models.Species.Id), nameof(RPPP_WebApp.Models.Species.Name));
    
    if (addDetail) await PrepareDetailDropdownList();
  }
  
  private async Task PrepareDetailDropdownList() {
     var plantIDs = await ctx.Plants
                          .OrderBy(d => d.Id)
                          .Select(d => new { d.Id})
                          .ToListAsync();

    var operationTypes = await ctx.OperationTypes
                          .OrderBy(d => d.Id)
                          .Select(d => new { d.Id, d.Name })
                          .ToListAsync();

    var requestIDs = await ctx.Requests
                          .OrderBy(d => d.Id)
                          .Select(d => new { d.Id })
                          .ToListAsync();

    ViewBag.PlantIDs = new SelectList(plantIDs,nameof(RPPP_WebApp.Models.Plant.Id),nameof(RPPP_WebApp.Models.Plant.Id));
    ViewBag.operationTypes = new SelectList(operationTypes, nameof(RPPP_WebApp.Models.OperationType.Id), nameof(RPPP_WebApp.Models.OperationType.Name));
    ViewBag.requestIDs = new SelectList(requestIDs, nameof(RPPP_WebApp.Models.Request.Id), nameof(RPPP_WebApp.Models.Request.Id));
  }
}