using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ReservaCabanasSite.Pages.Reservas
{
    public class CreateModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Redirigir al primer paso del wizard
            return RedirectToPage("Step1");
        }
    }
}
