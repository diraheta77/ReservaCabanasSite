using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Services;

var builder = WebApplication.CreateBuilder(args);

// Agregar DbContext con SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddRazorPages();

// Configurar sesiones
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configurar HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Registrar servicios de autenticación
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Usar sesiones
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        Console.WriteLine("[MIGRACION] Ejecutando EnsureCreated...");
        db.Database.EnsureCreated();
        Console.WriteLine("[MIGRACION] Ejecutando Migrate...");
        db.Database.Migrate();
        Console.WriteLine("[MIGRACION] Migraciones aplicadas correctamente.");
        
        // Crear usuarios por defecto si no existen
        if (!db.Usuarios.Any())
        {
            // Usuario Administrador
            var adminUser = new Usuario
            {
                NombreUsuario = "admin",
                Password = "admin123", // Se hasheará en el servicio
                NombreCompleto = "Administrador del Sistema",
                Rol = "Administrador",
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            
            // Usuario Operador
            var operadorUser = new Usuario
            {
                NombreUsuario = "operador",
                Password = "operador123", // Se hasheará en el servicio
                NombreCompleto = "Operador del Sistema",
                Rol = "Operador",
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            
            // Hashear las contraseñas
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var adminHashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(adminUser.Password));
                adminUser.Password = Convert.ToBase64String(adminHashedBytes);
                
                var operadorHashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(operadorUser.Password));
                operadorUser.Password = Convert.ToBase64String(operadorHashedBytes);
            }
            
            db.Usuarios.Add(adminUser);
            db.Usuarios.Add(operadorUser);
            db.SaveChanges();
            Console.WriteLine("[SEED] Usuario administrador creado: admin / admin123");
            Console.WriteLine("[SEED] Usuario operador creado: operador / operador123");
        }
        else if (!db.Usuarios.Any(u => u.NombreUsuario == "operador"))
        {
            // Solo crear el usuario operador si no existe
            var operadorUser = new Usuario
            {
                NombreUsuario = "operador",
                Password = "operador123", // Se hasheará en el servicio
                NombreCompleto = "Operador del Sistema",
                Rol = "Operador",
                Activo = true,
                FechaCreacion = DateTime.Now
            };
            
            // Hashear la contraseña
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var operadorHashedBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(operadorUser.Password));
                operadorUser.Password = Convert.ToBase64String(operadorHashedBytes);
            }
            
            db.Usuarios.Add(operadorUser);
            db.SaveChanges();
            Console.WriteLine("[SEED] Usuario operador creado: operador / operador123");
        }
    }
    catch (Exception ex)
    {
        System.IO.File.AppendAllText("D:\\home\\site\\wwwroot\\migracion_error.log", ex.ToString());
        Console.WriteLine("[MIGRACION] Error: " + ex.ToString());
        throw;
    }
}

app.Run();
