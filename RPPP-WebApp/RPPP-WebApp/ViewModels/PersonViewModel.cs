using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace RPPP_WebApp.ViewModels;

public class PersonViewModel
{

  [Key]
  public int PersonId { get; set; }

  [Required]
  [Display(Name = "Name and Surname")]
  public string Name { get; set; }

  [Display(Name = "Phone Number")]
  public string PhoneNumber { get; set; }

  [Display(Name = "Email")]
  public string Email { get; set; }


  [ForeignKey("Address")]
  [Display(Name = "Address")]
  public int AddressId { get; set; }

  public string RoleName { get; set; }
  [Display(Name = "Address City")]
  public string Address_City {get; set;}
  [Display(Name = "Address Postal Code")]
  public int Address_PostalCode {get; set;}
  [Display(Name = "Address Street")]
  public string Address_Street {get; set;}
  [Display(Name = "Address Number")]
  public int Address_Number {get; set;}

  [ForeignKey("Role")]
  [Display(Name = "Role")]
  public int RoleId { get; set; }
  [JsonIgnore] public IEnumerable<RPPP_WebApp.ViewModels.PersonViewModel> People { get; set; }

  [JsonIgnore] public IEnumerable<RPPP_WebApp.ViewModels.PlotViewModel> Plots { get; set; }
  [JsonIgnore] public ICollection<RPPP_WebApp.ViewModels.OrderViewModel> Orders { get; set; }

  public PersonViewModel() {
    this.Orders = new List<RPPP_WebApp.ViewModels.OrderViewModel>();
  }
}