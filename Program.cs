using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using ReservaCabanasSite.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using ReservaCabanasSite.Filters;

var builder = WebApplication.CreateBuilder(args);

// Agregar DbContext con SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

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

// Registrar filtros de autenticación
builder.Services.AddScoped<AuthFilter>();
builder.Services.AddScoped<AdminAuthFilter>();

// Registrar servicio de exportación
builder.Services.AddScoped<IExportacionService, ExportacionService>();

// Configurar autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Configurar autorización
builder.Services.AddAuthorization();

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

// Usar autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();

// Inicialización de base de datos y datos de prueba
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Solo aplicar migraciones (comentado para producción - ejecutar manualmente)
        // db.Database.Migrate();
        
        // Verificar si la base de datos puede conectarse
        if (db.Database.CanConnect())
        {
            logger.LogInformation("Conexión a base de datos exitosa");
            
            // Crear usuarios por defecto si no existen
            if (!db.Usuarios.Any())
            {
                logger.LogInformation("Creando usuarios por defecto...");
                
                // Usuario Administrador
                var adminUser = new Usuario
                {
                    NombreUsuario = "admin",
                    Password = "admin123",
                    NombreCompleto = "Administrador del Sistema",
                    Rol = "Administrador",
                    Activo = true,
                    FechaCreacion = DateTime.Now
                };
                
                // Usuario Operador
                var operadorUser = new Usuario
                {
                    NombreUsuario = "operador",
                    Password = "operador123",
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
                
                logger.LogInformation("Usuarios por defecto creados exitosamente");
            }
            else if (!db.Usuarios.Any(u => u.NombreUsuario == "operador"))
            {
                logger.LogInformation("Creando usuario operador...");
                
                // Solo crear el usuario operador si no existe
                var operadorUser = new Usuario
                {
                    NombreUsuario = "operador",
                    Password = "operador123",
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
                
                logger.LogInformation("Usuario operador creado exitosamente");
            }
            else
            {
                logger.LogInformation("Los usuarios ya existen en la base de datos");
            }
        }
        else
        {
            logger.LogError("No se pudo conectar a la base de datos");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error durante la inicialización de la base de datos");
        
        // Guardar log en archivo si hay error
        try
        {
            //var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "startup_error.log");
            //Directory.CreateDirectory(Path.GetDirectoryName(logPath));
            //File.WriteAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}\n{ex}\n");
        }
        catch
        {
            // Si falla el log en archivo, al menos se registró en el logger
        }
        
        // En producción, podrías querer que continue sin usuarios por defecto
        // throw; // Descomenta esta línea si quieres que la app no inicie si falla esto
    }
}

app.Run();