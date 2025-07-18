using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;

var builder = WebApplication.CreateBuilder(args);

// Agregar DbContext con SQLite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        db.Database.Migrate();

        // Opcional: seed de datos de prueba si la tabla Cabanas está vacía
        if (!db.Cabanas.Any())
        {
            db.Cabanas.Add(new Cabana { Nombre = "Demo", Capacidad = 4, PrecioPorNoche = 1000, Activa = true });
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        // Log simple a archivo (en Azure puedes ver esto en Log Stream)
        System.IO.File.AppendAllText("D:\\home\\site\\wwwroot\\migracion_error.log", ex.ToString());
        throw;
    }
}

app.Run();
