using System.ComponentModel.DataAnnotations;

namespace Stockroom.Web.Components.Pages.Admin;

public class NewUserInput
{
    [Required, StringLength(50)]
    public string UserName { get; set; } = "";

    [Required, StringLength(100)]
    public string DisplayName { get; set; } = "";
}
