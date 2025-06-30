using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;
using Microsoft.EntityFrameworkCore;

namespace ReservaCabanasSite.Pages.Cabanas;

public class IndexModel : PageModel
{
    private readonly AppDbContext _context;
    public List<Cabana> Cabanas { get; set; } = new();

    public IndexModel(AppDbContext context)
    {
        _context = context;
    }

    public void OnGet()
    {
        Cabanas = _context.Cabanas
        .Include(c => c.Imagenes)
        .ToList();
    }
}