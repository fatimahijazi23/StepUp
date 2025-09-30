
using System.ComponentModel.DataAnnotations;

public class RegisterCompanyDto
{
    public string CompanyName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    [Required]
    public int CompanySize { get; set; }   // number of employees or category

    [Required]
    public Industry Industry { get; set; }  // dropdown (enum)

}