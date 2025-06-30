using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Data;

namespace ReservaCabanasSite.Pages.Cabanas;

public class CreateModel : PageModel
{
    private readonly AppDbContext _context;
    [BindProperty]
    public Cabana Cabana { get; set; } = new();

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(List<IFormFile> Imagenes)
    {
        if (!ModelState.IsValid)
            return Page();

        _context.Cabanas.Add(Cabana);
        await _context.SaveChangesAsync();

        if (Imagenes != null && Imagenes.Count > 0)
        {
            foreach (var imagen in Imagenes)
            {
                if (imagen.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(imagen.FileName)}";
                    var filePath = Path.Combine("wwwroot/img/cabanas", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imagen.CopyToAsync(stream);
                    }
                    var cabanaImagen = new CabanaImagen
                    {
                        CabanaId = Cabana.Id,
                        ImagenUrl = $"/img/cabanas/{fileName}"
                    };
                    _context.CabanaImagenes.Add(cabanaImagen);
                }
            }
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("Index");
    }
}