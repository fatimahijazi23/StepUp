
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace teamProject.WEB.Pages 
{
    public class HomeModel : PageModel
    {
        // Add this property to fix CS1061
        [BindProperty]
        public string SelectedAccountType { get; set; }

        public void OnGet()
        {
        }
    }
}
