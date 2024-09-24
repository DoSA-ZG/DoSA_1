using RPPP_WebApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;

namespace RPPP_WebApp.TagHelpers;

[HtmlTargetElement(Attributes="page-info")]
public class PagerTagHelper : TagHelper
{

  private readonly IUrlHelperFactory urlHelperFactory;  
  private readonly AppSettings appData;   
  public PagerTagHelper(IUrlHelperFactory helperFactory, IOptionsSnapshot<AppSettings> options)
  {      
    urlHelperFactory = helperFactory;
    appData = options.Value;
  }

  [ViewContext]
  [HtmlAttributeNotBound]
  public ViewContext ViewContext { get; set; }

  /// <summary>
  /// Serialized string containing information about current page, and total number of pages
  /// </summary>
  public PagingInfo PageInfo { get; set; }

  /// <summary>
  /// Action for which link should be created
  /// </summary>
  public string PageAction { get; set; }

  /// <summary>
  /// Tooltip for the textbox that is used to enter desired page
  /// </summary>
  public string PageTitle { get; set; }

  public override void Process(TagHelperContext context, TagHelperOutput output)
  {
    output.TagName = "nav";      
    int offset = appData.PageOffset;      
    TagBuilder paginationList = new TagBuilder("ul");
    paginationList.AddCssClass("pagination");      

    if (PageInfo.CurrentPage - offset > 1) //create list item for the first page
    {
      var tag = BuildListItemForPage(1, false, "1..");
      paginationList.InnerHtml.AppendHtml(tag);
    }

    for (int i = Math.Max(1, PageInfo.CurrentPage - offset);
             i <= Math.Min(PageInfo.TotalPages, PageInfo.CurrentPage + offset);
             i++)
    {
      var tag = BuildListItemForPage(i, i == PageInfo.CurrentPage);        
      paginationList.InnerHtml.AppendHtml(tag);
    }

    if (PageInfo.CurrentPage + offset < PageInfo.TotalPages) //create list item for the last page
    {
      var tag = BuildListItemForPage(PageInfo.TotalPages, false, ".. " + PageInfo.TotalPages);
      paginationList.InnerHtml.AppendHtml(tag);
    }

    output.Content.AppendHtml(paginationList);     
  }


  private TagBuilder BuildListItemForPage(int i, bool isActive)
  {
    return BuildListItemForPage(i, isActive, i.ToString());
  }

  /// <summary>
  ///  Create tag for the i-th page with *text* as text
  /// </summary>
  /// <param name="i">page number</param>
  /// <param name="text">text to display</param>
  /// <returns>TagBuilder with the link</returns>
  private TagBuilder BuildListItemForPage(int i, bool isActive, string text)
  {
    IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);      

    TagBuilder a = new TagBuilder("a");
    a.InnerHtml.Append(text);
    a.Attributes["href"] = urlHelper.Action(PageAction, new
    {
      page = i,
      sort = PageInfo.Sort,
      ascending = PageInfo.Ascending,
    });
    a.AddCssClass("page-link");

    TagBuilder li = new TagBuilder("li");
    li.AddCssClass("page-item " + (isActive ? "active" : ""));
    li.InnerHtml.AppendHtml(a);
    return li;
  }

}