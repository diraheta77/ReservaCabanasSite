using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Filters;
using ReservaCabanasSite.Services;

namespace ReservaCabanasSite.Pages.Cabanas
{
    [ServiceFilter(typeof(AuthFilter))]
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;
        public List<Cabana> Cabanas { get; set; } = new();
        public bool EsAdministrador { get; set; }

        public IndexModel(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public void OnGet()
        {
            Cabanas = _context.Cabanas
            .Include(c => c.Imagenes)
            .ToList();

            EsAdministrador = _authService.GetCurrentUserRole() == "Administrador";
        }
    }
}