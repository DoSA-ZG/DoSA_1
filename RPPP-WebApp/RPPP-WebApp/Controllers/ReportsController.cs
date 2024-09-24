
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfRpt.ColumnsItemsTemplates;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using RPPP_WebApp.ViewModels;

namespace MVC_EN.Controllers;

public class ReportsController : Controller
{
  private readonly Rppp12Context ctx;
  private readonly IWebHostEnvironment environment;
  private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

  public ReportsController(Rppp12Context ctx, IWebHostEnvironment environment)
  {
    this.ctx = ctx;
    this.environment = environment;
  }

  public async Task<IActionResult> People()
  {
    string title = "People";
    var people = await ctx.People
                             .Include(m => m.Role)
                             .Include(m => m.Address)
                             .AsNoTracking()
                             .OrderBy(d => d.Id)
                             .ToListAsync();
    PdfReport report = CreateReport(title);
    #region Header and footer
    report.PagesFooter(footer =>
    {
      footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
    })
    .PagesHeader(header =>
    {
      header.CacheHeader(cache: true); // It's a default setting to improve the performance.
      header.DefaultHeader(defaultHeader =>
      {
        defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
        defaultHeader.Message(title);
      });
    });
    #endregion
    #region Set datasource and define columns
    report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(people));

    report.MainTableColumns(columns =>
    {
      columns.AddColumn(column =>
      {
        column.IsRowNumber(true);
        column.CellsHorizontalAlignment(HorizontalAlignment.Right);
        column.IsVisible(true);
        column.Order(0);
        column.Width(1);
        column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName(nameof(Person.Id));
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(1);
        column.Width(1);
        column.HeaderCell("Id");
      });

      columns.AddColumn(column =>
      {
        column.PropertyName(nameof(Person.Name));
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(2);
        column.Width(1);
        column.HeaderCell("Name");
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<Person>(x => x.Role.Name);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(3);
        column.Width(1);
        column.HeaderCell("Role name", horizontalAlignment: HorizontalAlignment.Center);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<Person>(x => x.PhoneNumber);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(4);
        column.Width(1);
        column.HeaderCell("Phone number", horizontalAlignment: HorizontalAlignment.Center);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<Person>(x => x.Email);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(5);
        column.Width(1);
        column.HeaderCell("Email", horizontalAlignment: HorizontalAlignment.Center);
      });

       columns.AddColumn(column =>
      {
        column.PropertyName<Person>(x => x.Address.City);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(6);
        column.Width(1);
        column.HeaderCell("Address city", horizontalAlignment: HorizontalAlignment.Center);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<Person>(x => x.Address.PostalCode);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(7);
        column.Width(1);
        column.HeaderCell("Address postal code", horizontalAlignment: HorizontalAlignment.Center);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<Person>(x => x.Address.Street);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(8);
        column.Width(1);
        column.HeaderCell("Address street", horizontalAlignment: HorizontalAlignment.Center);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<Person>(x => x.Address.Number);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(9);
        column.Width(1);
        column.HeaderCell("Address number", horizontalAlignment: HorizontalAlignment.Center);
      });
      
    });
    #endregion
    byte[] pdf = report.GenerateAsByteArray();

    if (pdf != null)
    {
      Response.Headers.Add("content-disposition", "inline; filename=plots.pdf");
      return File(pdf, "application/pdf");
    }
    else
    {
      return NotFound();
    }
  }

  public async Task<IActionResult> Plots()
  {
    string title = "Plots";
    var plots = await ctx.Plots
                             .Include(m => m.Owner)
                             .Include(m => m.Soil)
                             .AsNoTracking()
                             .OrderBy(d => d.Size)
                             .ToListAsync();
    PdfReport report = CreateReport(title);
    #region Header and footer
    report.PagesFooter(footer =>
    {
      footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
    })
    .PagesHeader(header =>
    {
      header.CacheHeader(cache: true); // It's a default setting to improve the performance.
      header.DefaultHeader(defaultHeader =>
      {
        defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
        defaultHeader.Message(title);
      });
    });
    #endregion
    #region Set datasource and define columns
    report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(plots));

    report.MainTableColumns(columns =>
    {
      columns.AddColumn(column =>
      {
        column.IsRowNumber(true);
        column.CellsHorizontalAlignment(HorizontalAlignment.Right);
        column.IsVisible(true);
        column.Order(0);
        column.Width(1);
        column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName(nameof(Plot.Id));
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(1);
        column.Width(1);
        column.HeaderCell("Id");
      });

      columns.AddColumn(column =>
      {
        column.PropertyName(nameof(Plot.CoordX));
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(1);
        column.Width(1);
        column.HeaderCell("Coordinate X");
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<Plot>(x => x.CoordY);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(2);
        column.Width(1);
        column.HeaderCell("Coordinate Y", horizontalAlignment: HorizontalAlignment.Center);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<Plot>(x => x.Size);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(3);
        column.Width(1);
        column.HeaderCell("Size", horizontalAlignment: HorizontalAlignment.Center);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<Plot>(x => x.LightIntensity);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(3);
        column.Width(1);
        column.HeaderCell("Light intensity", horizontalAlignment: HorizontalAlignment.Center);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<Plot>(x => x.Owner.Name);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(3);
        column.Width(1);
        column.HeaderCell("Owner", horizontalAlignment: HorizontalAlignment.Center);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<Plot>(x => x.Soil.Name);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(3);
        column.Width(1);
        column.HeaderCell("Soil type", horizontalAlignment: HorizontalAlignment.Center);
      });
    });

    #endregion
    byte[] pdf = report.GenerateAsByteArray();

    if (pdf != null)
    {
      Response.Headers.Add("content-disposition", "inline; filename=plots.pdf");
      return File(pdf, "application/pdf");
    }
    else
    {
      return NotFound();
    }
  }

  public async Task<IActionResult> Requests()
  {
    string title = "Requests";
    var people = await ctx.Requests
                             .Include(m => m.Order)
                             .Include(m => m.Operations)
                             .Include(m => m.NeededSpecies)
                             .AsNoTracking()
                             .OrderBy(d => d.Id)
                             .ToListAsync();
    PdfReport report = CreateReport(title);
    #region Header and footer
    report.PagesFooter(footer =>
    {
      footer.DefaultFooter(DateTime.Now.ToString("dd.MM.yyyy."));
    })
    .PagesHeader(header =>
    {
      header.CacheHeader(cache: true); // It's a default setting to improve the performance.
      header.DefaultHeader(defaultHeader =>
      {
        defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
        defaultHeader.Message(title);
      });
    });
    #endregion
    #region Set datasource and define columns
    report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(people));

    report.MainTableColumns(columns =>
    {
      columns.AddColumn(column =>
      {
        column.IsRowNumber(true);
        column.CellsHorizontalAlignment(HorizontalAlignment.Right);
        column.IsVisible(true);
        column.Order(0);
        column.Width(1);
        column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName(nameof(RPPP_WebApp.Models.Request.Id));
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(1);
        column.Width(1);
        column.HeaderCell("Id");
      });

      columns.AddColumn(column =>
      {
        column.PropertyName(nameof(RPPP_WebApp.Models.Request.Amount));
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(2);
        column.Width(1);
        column.HeaderCell("Amount");
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<RPPP_WebApp.Models.Request>(x => x.NeededSpecies.Name);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(3);
        column.Width(1);
        column.HeaderCell("Needed species name", horizontalAlignment: HorizontalAlignment.Center);
      });

      columns.AddColumn(column =>
      {
        column.PropertyName<RPPP_WebApp.Models.Request>(x => x.NeededSpecies.NutritionalValues);
        column.CellsHorizontalAlignment(HorizontalAlignment.Center);
        column.IsVisible(true);
        column.Order(4);
        column.Width(1);
        column.HeaderCell("Nutritional values", horizontalAlignment: HorizontalAlignment.Center);
      });

      

      
    });
    #endregion
    byte[] pdf = report.GenerateAsByteArray();

    if (pdf != null)
    {
      Response.Headers.Add("content-disposition", "inline; filename=plots.pdf");
      return File(pdf, "application/pdf");
    }
    else
    {
      return NotFound();
    }
  }

  #region Private methods

  PdfReport CreateReport(string title)
  {
    var pdf = new PdfReport();

    pdf.DocumentPreferences(doc =>
    {
      doc.Orientation(PageOrientation.Portrait);
      doc.PageSize(PdfPageSize.A4);
      doc.DocumentMetadata(new DocumentMetadata
      {
        Author = "Egor Shevtsov",
        Application = "RPPP12",
        Title = title
      });
      doc.Compression(new CompressionSettings
      {
        EnableCompression = true,
        EnableFullCompression = true
      });
    })
    .DefaultFonts(fonts => {
      fonts.Path(Path.Combine(environment.WebRootPath, "fonts", "verdana.ttf"),
                       Path.Combine(environment.WebRootPath, "fonts", "tahoma.ttf"));
      fonts.Size(9);
      fonts.Color(System.Drawing.Color.Black);
    })
    //
    .MainTableTemplate(template =>
    {
      template.BasicTemplate(BasicTemplate.ProfessionalTemplate);
    })
    .MainTablePreferences(table =>
    {
      table.ColumnsWidthsType(TableColumnWidthType.Relative);
            //table.NumberOfDataRowsPerPage(20);
            table.GroupsPreferences(new GroupsPreferences
      {
        GroupType = GroupType.HideGroupingColumns,
        RepeatHeaderRowPerGroup = true,
        ShowOneGroupPerPage = true,
        SpacingBeforeAllGroupsSummary = 5f,
        NewGroupAvailableSpacingThreshold = 150,
        SpacingAfterAllGroupsSummary = 5f
      });
      table.SpacingAfter(4f);
    });

    return pdf;
    #endregion
  }
}