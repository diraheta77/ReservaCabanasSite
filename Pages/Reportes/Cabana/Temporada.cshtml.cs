using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Filters;

namespace ReservaCabanasSite.Pages.Reportes.Cabana
{
    [ServiceFilter(typeof(AdminAuthFilter))]
    public class TemporadaModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}