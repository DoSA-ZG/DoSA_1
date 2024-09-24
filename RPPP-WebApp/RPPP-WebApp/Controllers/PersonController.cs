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
  /// Web API service for People CRUD operations
  /// </summary>
[ApiController]
[Route("[controller]")]
public class PersonController : Controller
{
  private readonly CommonController<int, OrderViewModel> _ordersController;
  private readonly Rppp12Context ctx;
  private readonly AppSettings appData;
    /// <summary>
    /// Create an instance of the controller 
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="options"> </param>
    /// <param name="operationsController">controller intializing an instance of detail table viewmodel</param>
 
  public PersonController(Rppp12Context ctx, IOptionsSnapshot<AppSettings> options,CommonController<int, OrderViewModel> ordersController)
  {
    this.ctx = ctx;
    appData = options.Value;
    _ordersController = ordersController;
  }

    /// <summary>
    /// Get Request to view base information about Person table
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

    var query = ctx.People.AsNoTracking();
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
   
    var people = await query
                        .Select(m => new PersonViewModel
                        {
                          PersonId = m.Id,
                          Name = m.Name,
                          Email = m.Email,
                          RoleId = m.RoleId,
                          AddressId = m.AddressId,
                          PhoneNumber = m.PhoneNumber,
                          RoleName = m.Role.Name,
                          Address_City = m.Address.City,
                          Address_Number = m.Address.Number,
                          Address_PostalCode = m.Address.PostalCode,
                          Address_Street = m.Address.Street
                        })
                        .Skip((page - 1) * pagesize)
                        .Take(pagesize)
                        .ToListAsync();
    var model = new PeopleViewModel
    {
      People = people,
      PagingInfo = pagingInfo
    }; 

    return View(model);
  }   
    /// <summary>
    /// Request to create Person without any additional data besides ID 
    /// </summary>
    /// <returns></returns>
    
  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpGet("create")]
  public async Task<IActionResult> Create()
  {
    int maxId = await ctx.People.MaxAsync(p => p.Id) + 1;
    var person = new PersonViewModel
    {
      PersonId = maxId,
      Name = "-"
    };
    await PrepareDropDownLists(true);
    return View(person);
  }
    /// <summary>
    /// Request to create Person
    /// </summary>
    /// <param name="model">instance of PersonViewModel</param>
    /// <returns></returns>

  [ApiExplorerSettings(IgnoreApi = true)]
  [HttpPost("create")]
  public async Task<IActionResult> Create([FromForm] PersonViewModel model)
  {
    TempData[Constants.ErrorOccurred] = true;
    if (ModelState.IsValid)
    {
      try
      {
        var (actionResult, personId) = await addPerson(model);

        foreach (var orderToAdd in model.Orders) {
          orderToAdd.CustomerId = personId;
          await _ordersController.Create(orderToAdd);
        }

        TempData[Constants.Message] = $"Person has been added with id = {personId}";
        TempData[Constants.ErrorOccurred] = false;
        return RedirectToAction(nameof(Edit), new {id = personId});

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
    /// Request responsible for whole logic of editing master Person data and detail informations
    /// </summary>
    /// <param name="model">An instance of PersonViewModel</param>
    /// <param name="page">number of page</param>
    /// <param name="sort">Index of the field to sort by</param>
    /// <param name="ascending">determines whether the sort should be ascending</param>
    /// <returns></returns>

  [ApiExplorerSettings(IgnoreApi = true)] 
  [HttpPost("edit/{id}")]
  public async Task<IActionResult> Edit([FromForm] PersonViewModel model, int page = 1, int sort = 1, bool ascending = true)
  {
    ViewBag.Page = page;
    ViewBag.Sort = sort;
    ViewBag.Ascending = ascending;

    TempData[Constants.ErrorOccurred] = true;
    if (ModelState.IsValid)
    {
      var person = await ctx.People
                      .Include(p => p.Orders)
                      .Where(p => p.Id == model.PersonId)
                      .AsSplitQuery()
                      .FirstOrDefaultAsync();

      if (person == null)
      {
        TempData[Constants.Message] = $"Invalid person id: {person?.Id}";
        return RedirectToAction(nameof(Index));
      }
      
      person.Id = model.PersonId;
      person.AddressId = model.AddressId;
      person.RoleId = Int16.Parse(model.RoleName);
      person.Name = model.Name;
      person.PhoneNumber = model.PhoneNumber;
      person.Email = model.Email;
                        
      try
      {
        ctx.Update(person);
        await ctx.SaveChangesAsync();

        List<int> orderIds = model.Orders
                                .Where(p => p.OrderId > 0)
                                .Select(p => p.OrderId)
                                .ToList();

        foreach(var oneOrder in person.Orders.Where(i => !orderIds.Contains(i.Id)).ToList()) {
          await _ordersController.Delete(oneOrder.Id);
        }

        foreach (var oneOrder in model.Orders) {
          oneOrder.CustomerId = person.Id;
          if (oneOrder.OrderId > 0) await _ordersController.Update(person.Orders.First(p => p.Id == oneOrder.OrderId).Id, oneOrder);
          else await _ordersController.Create(oneOrder);
        }  

        TempData[Constants.Message] = $"Person with id {person.Id} has been updated.";
        TempData[Constants.ErrorOccurred] = false;

        return RedirectToAction(nameof(Edit), new {
          id = person.Id,
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
    /// Request to delete Person of given id
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
    var person = await ctx.People
        .Include(m => m.Plots)
        .Include(m => m.Orders).ThenInclude(p => p.Requests)
        .Where(m => m.Id == id)
        .Select(m => new PersonViewModel
        {
          PersonId = m.Id,
          Name = m.Name,
          PhoneNumber = m.PhoneNumber,
          Email = m.Email,
          RoleId = m.RoleId,
          RoleName = m.Role.Name,
          AddressId = m.AddressId,
          Address_City = m.Address.City,
          Address_Number = m.Address.Number,
          Address_PostalCode = m.Address.PostalCode,
          Address_Street = m.Address.Street
        })
        .AsSplitQuery()
        .SingleOrDefaultAsync();
    
    if (person == null) {
      return NotFound($"Invalid person id: {id}");
    }

    var orders = await ctx.Orders
                        .Where(pl => pl.CustomerId == person.PersonId)
                        .Select(pl => new OrderViewModel
                        {
                          OrderId = pl.Id,
                          CustomerId = pl.CustomerId,
                          Date = pl.Date,
                        })
                        .ToListAsync();

    person.Orders = orders;

    ViewBag.Page = page;
    ViewBag.Sort = sort;
    ViewBag.Ascending = ascending;

    await PrepareDropDownLists(true);
    return View(viewName, person);
  }
    /// <summary>
    /// Get a list of People with pagination
    /// </summary>
    /// <param name="page">number of page</param>
    /// <param name="itemsPerPage">number of items per page</param>
    /// <param name="sort">Index of the field to sort by</param>
    /// <param name="ascending">determines whether the sort should be ascending</param>
    /// <returns></returns>

  [HttpGet(Name = "GetPeople")]
  public async Task<List<PersonViewModel>> GetAll([FromQuery] int? page, [FromQuery] int? itemsPerPage, [FromQuery] int sort = 1, [FromQuery] bool ascending = true) 
  {
    var query = ctx.People.AsQueryable();
    query = query.ApplySort(sort, ascending);

    IQueryable<PersonViewModel> list = query.OrderBy(m => m.Id)
                        .Select(m => new PersonViewModel
                        {
                          PersonId = m.Id,
                          Name = m.Name,
                          PhoneNumber = m.PhoneNumber,
                          Email = m.Email,
                          RoleId = m.RoleId,
                          RoleName = m.Role.Name,
                          AddressId = m.AddressId,
                          Address_City = m.Address.City,
                          Address_Number = m.Address.Number,
                          Address_PostalCode = m.Address.PostalCode,
                          Address_Street = m.Address.Street
                        });

    if (page > 0 && itemsPerPage > 0) {
        list = list
            .Skip(((int)page - 1) * (int)itemsPerPage)
            .Take((int)itemsPerPage);
    }
    return list.ToList();
  }

    /// <summary>
    /// create a ne person
    /// </summary>
    /// <param name="model">an instance of PersonViewModel</param>
    /// <returns></returns>

  [HttpPost(Name="addPerson")]
  [ProducesResponseType(StatusCodes.Status201Created)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<(IActionResult, int Id)> addPerson(PersonViewModel model)
  {
    Person person = new Person();
    person.Id = await ctx.People.MaxAsync(p => p.Id) + 1;
    person.PhoneNumber = model.PhoneNumber;
    person.AddressId = model.AddressId;
    person.Name = model.Name;
    person.Email = model.Email;
    person.RoleId = model.RoleId;

    ctx.Add(person);
    await ctx.SaveChangesAsync();

    var addedPerson = await Get(person.Id);

    return (CreatedAtAction(nameof(Get), new { id = person.Id }, addedPerson.Value), person.Id);
  }
    /// <summary>
    /// select person by their id
    /// </summary>
    /// <param name="id">id of person to be selected </param>
    /// <returns></returns>

  [HttpGet("{id}", Name = "GetPersonById")]
  [ProducesResponseType(StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<ActionResult<PersonViewModel>> Get(int id)
  {   
    var person = await ctx.People                            
                        .Where(m => m.Id == id)
                        .Select(m => new PersonViewModel {
                          PhoneNumber = m.PhoneNumber,
                          Email = m.Email,
                          Name = m.Name,
                          Address_City = m.Address.City,
                          Address_Number = m.Address.Number,
                          Address_PostalCode = m.Address.PostalCode,
                          Address_Street = m.Address.Street,
                          RoleName = m.Role.Name
                        })
                        .FirstOrDefaultAsync();
    if (person == null)
    {      
      return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"No data for id = {id}");
    }
    else
    {
      return person;
    }
  }
    /// <summary>
    /// update existing person
    /// </summary>
    /// <param name="id">id of person to be updated</param>
    /// <param name="model">an instance of PersonViewModel</param>
    /// <returns></returns>
  
  [HttpPut("{id}", Name = "UpdatePerson")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> Update(int id, PersonViewModel model)
  {
    if (model.PersonId != id) //ModelState.IsValid i model != null are automatically evalued because of [ApiController]
    {
      return Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"Different ids {id} vs {model.PersonId}");
    }
    else
    {
      var person = await ctx.People.FindAsync(id);
      if (person == null)
      { 
        return Problem(statusCode: StatusCodes.Status404NotFound, detail: $"Invalid id = {id}");
      }

      person.PhoneNumber = model.PhoneNumber;
      person.AddressId = model.AddressId;
      person.Name = model.Name;
      person.Email = model.Email;
      person.RoleId = model.RoleId;

      await ctx.SaveChangesAsync();
      return NoContent();
    }
  }
    /// <summary>
    /// Delete a person
    /// </summary>
    /// <param name="id">id of person to be deleted</param>
    /// <returns></returns>

  [HttpDelete("{id}", Name = "DeletePerson")]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  public async Task<IActionResult> Delete(int id)
  {
    var person = await ctx.People.FindAsync(id);
    if (person == null)
    {
      return NotFound();
    }
    else
    {
      ctx.Remove(person);
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

    ViewBag.PlantIDs = new SelectList(plantIDs, nameof(RPPP_WebApp.Models.Plant.Id));
    ViewBag.operationTypes = new SelectList(operationTypes, nameof(RPPP_WebApp.Models.OperationType.Id), nameof(RPPP_WebApp.Models.OperationType.Name));
    ViewBag.requestIDs = new SelectList(requestIDs, nameof(RPPP_WebApp.Models.Request.Id));
  }
}