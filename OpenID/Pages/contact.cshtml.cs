using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeycloakTest.Pages
{
    [Authorize(Roles = "contact")]
    public class contact : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}