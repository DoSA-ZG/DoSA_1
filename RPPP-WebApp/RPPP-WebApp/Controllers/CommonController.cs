using Microsoft.AspNetCore.Mvc;
using RPPP_WebApp.Models;

namespace RPPP_WebApp.Controllers
{
  public interface CommonController<TKey, TModel>
  {
    public Task<List<TModel>> GetAll([FromQuery] string filter, [FromQuery] int? page, [FromQuery] int? itemsPerPage);
    public Task<ActionResult<TModel>> Get(TKey id);
    public Task<IActionResult> Create(TModel model);
    public Task<IActionResult> Update(TKey id, TModel model);
    public Task<IActionResult> Delete(TKey id);
  }
}