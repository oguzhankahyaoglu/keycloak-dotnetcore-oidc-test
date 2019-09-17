using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeycloakTest.Pages
{
    [Authorize]
    public class TestAuth : PageModel
    {
        public void OnGet()
        {
            
        }
    }
}