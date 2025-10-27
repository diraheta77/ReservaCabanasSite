# Contexto del Proyecto - Sistema de Administración de Cabañas

## 📋 Descripción General
Sistema web de gestión para Aldea Auriel, un complejo de cabañas. Permite administrar reservas, clientes, cabañas, temporadas, medios de pago y generar reportes financieros y operativos.

**Versión**: 1.0.0
**Fecha de creación**: Junio 2025
**Desarrollador**: Diego Iraheta
**Cliente**: Aldea Auriel

---

## 🛠️ Stack Tecnológico

### Backend
- **Framework**: ASP.NET Core 8.0 (Razor Pages)
- **Base de datos**: SQL Server
- **ORM**: Entity Framework Core
- **Autenticación**: Cookie-based Authentication con SHA256

### Frontend
- **Template**: Paper Dashboard 2 (Bootstrap 4)
- **Estilos**: CSS personalizado + Bootstrap
- **Iconos**: Font Awesome
- **JavaScript**: jQuery, Chart.js

### Librerías Principales
- **ClosedXML**: Exportación a Excel
- **iTextSharp**: Generación de PDFs
- **Toastr.js**: Notificaciones

---

## 🏗️ Arquitectura del Proyecto

### Patrón de Arquitectura
**Razor Pages** con patrón MVC implícito:
- Cada página `.cshtml` tiene su code-behind `.cshtml.cs` (PageModel)
- Servicios inyectados vía Dependency Injection
- Filtros de autorización personalizados

### Estructura de Carpetas

```
ReservaCabanasSite/
├── Data/
│   └── AppDbContext.cs          # Contexto de Entity Framework
├── Models/                      # Modelos de dominio
│   ├── Cabana.cs
│   ├── Cliente.cs
│   ├── Reserva.cs
│   ├── Usuario.cs
│   ├── Temporada.cs
│   ├── MedioPago.cs
│   ├── DatosEmpresa.cs
│   ├── Vehiculo.cs
│   ├── ReporteViewModel.cs      # ViewModels para reportes
│   └── ExportacionModels.cs     # Modelos para exportación
├── Services/                    # Servicios de negocio
│   ├── AuthService.cs           # Autenticación y sesiones
│   └── ExportacionService.cs    # Exportación Excel/PDF
├── Filters/                     # Filtros de autorización
│   ├── AuthFilter.cs            # Requiere usuario autenticado
│   └── AdminAuthFilter.cs       # Requiere rol Administrador
├── Pages/                       # Razor Pages (vistas + lógica)
│   ├── Cabanas/                 # CRUD de cabañas
│   ├── Clientes/                # CRUD de clientes
│   ├── Reservas/                # Gestión de reservas (wizard)
│   ├── Temporadas/              # CRUD de temporadas
│   ├── MediosPago/              # CRUD de medios de pago
│   ├── DatosEmpresa/            # Configuración empresa
│   ├── Usuarios/                # Gestión de usuarios (Admin)
│   ├── Reportes/                # Reportes financieros/operativos
│   │   ├── Cliente/             # Reportes por cliente
│   │   ├── Cabana/              # Reportes por cabaña
│   │   └── Otros/               # Medios de pago/contacto
│   ├── Shared/                  # Layout y componentes compartidos
│   ├── Login.cshtml             # Página de inicio de sesión
│   └── Logout.cshtml            # Cierre de sesión
├── Migrations/                  # Migraciones de EF Core
├── wwwroot/                     # Archivos estáticos
│   ├── assets/                  # Template Paper Dashboard
│   ├── uploads/                 # Imágenes subidas
│   └── js/                      # JavaScript personalizado
├── appsettings.json             # Configuración
└── Program.cs                   # Punto de entrada
```

---

## 📊 Modelos de Datos Principales

### Usuario
- **Roles**: Administrador, Operador
- **Campos**: Id, NombreUsuario, Password (SHA256), NombreCompleto, Rol, Activo, FechaCreacion
- **Autenticación**: Password hasheado con SHA256

### Cliente
- **Campos**: Id, Dni, Nombre, Apellido, FechaNacimiento, Nacionalidad, Dirección, Ciudad, Provincia, País, Teléfono, Email, Observaciones, Activo
- **Relaciones**: 1:N con Vehiculo

### Cabana
- **Campos**: Id, Nombre, Capacidad, Descripción, PrecioBase, Activa
- **Relaciones**: 1:N con CabanaImagen

### Reserva
- **Campos**: Id, CabanaId, ClienteId, FechaDesde, FechaHasta, Temporada, CantidadPersonas, MedioContacto, MetodoPago, EstadoPago, EstadoReserva, MontoTotal, PrecioPorPersona, TemporadaId, Observaciones, FechaCreacion, Activa
- **Relaciones**: N:1 con Cabana, N:1 con Cliente, N:1 con Temporada

### Temporada
- **Campos**: Id, Nombre, FechaInicio, FechaFin, PrecioPorPersona, Activa
- **Propósito**: Gestionar precios variables por temporada (alta/baja)

### MedioPago
- **Campos**: Id, Nombre, Descripcion, Activo, FechaCreacion
- **Ejemplos**: Efectivo, Transferencia, Tarjeta de Crédito

### DatosEmpresa
- **Campos**: Id, Nombre, Direccion, Telefono, Email, Sitio Web, Términos y Condiciones, RutaLogo
- **Uso**: Información mostrada en reportes e impresiones

---

## 🔐 Sistema de Autenticación

### Flujo de Autenticación
1. Usuario ingresa credenciales en `/Login`
2. `AuthService.Login()` valida y hashea con SHA256
3. Si es válido, se crea sesión con datos del usuario
4. Cookie de autenticación válida por 8 horas (sliding)
5. Filtros `AuthFilter` y `AdminAuthFilter` protegen páginas

### Usuarios por Defecto (creados en primera ejecución)
- **Administrador**:
  - Usuario: `admin`
  - Password: `admin123`
  - Rol: Administrador

- **Operador**:
  - Usuario: `operador`
  - Password: `operador123`
  - Rol: Operador

### Filtros de Autorización
- **AuthFilter**: Requiere usuario autenticado (cualquier rol)
- **AdminAuthFilter**: Requiere rol "Administrador"

### Sesiones
- Configuración: 8 horas de timeout
- Datos guardados: UserId, UserName, UserRole, UserFullName
- Acceso: Vía `IAuthService` (inyectado)

---

## 🎯 Características Principales

### 1. Gestión de Reservas (Wizard de 3 pasos)
**Flujo**:
- **Step 1**: Selección de cabaña, fechas, temporada, cantidad de personas
- **Step 2**: Datos del cliente (búsqueda por DNI o creación nuevo)
- **Step 3**: Método de pago, estado de pago/reserva, observaciones, confirmación

**Características**:
- Validación de disponibilidad de cabaña
- Cálculo automático de precio según temporada
- Envío de email de confirmación
- Impresión de comprobante PDF
- Edición de reservas existentes

### 2. Gestión de Clientes
- CRUD completo (Create, Read, Update, Inactivate)
- Búsqueda por DNI
- Información de vehículo (opcional)
- Validación de DNI único
- **No se eliminan físicamente**, solo se inactivan

### 3. Gestión de Usuarios (Solo Administradores)
- CRUD completo
- Dos roles: Administrador y Operador
- Inactivación en lugar de eliminación
- No se puede auto-inactivar
- Cambio de contraseña

### 4. Sistema de Reportes
Todos los reportes incluyen:
- Filtros por fecha y/o cabaña
- Estadísticas resumidas
- Exportación a Excel y PDF
- Diseño consistente

**Reportes Disponibles**:

**Por Cliente**:
- Reservas por Cliente (agrupado)
- Reservas por Provincia
- Distribución por Edades

**Por Cabaña**:
- Reservas por Cabaña
- Reservas por Temporada
- Reservas por Meses

**Otros**:
- Ingresos por Medio de Pago
- Ingresos por Medio de Contacto

### 5. Exportación de Datos
**Servicio**: `ExportacionService`
- **Excel**: ClosedXML con formato profesional
- **PDF**: iTextSharp con logo de empresa
- **Formatos soportados**: Tablas con estilos, estadísticas, totales

---

## 🎨 Diseño y UX

### Template Base
**Paper Dashboard 2**
- Sidebar colapsable con menús jerárquicos
- Diseño responsivo
- Tema de colores: Turquesa (#0097b2) y Marrón (#5c4a45)

### Estructura del Menú
```
├── Cabañas
├── Reservas
│   ├── Calendario
│   └── Listado
├── Clientes
├── Ajustes (Solo Admin)
│   ├── Datos de la Empresa
│   ├── Temporadas
│   └── Medios de Pago
├── Usuarios (Solo Admin)
├── Reportes (Solo Admin)
│   ├── Reservas por Cliente
│   │   ├── Clientes
│   │   ├── Provincia
│   │   └── Edades
│   ├── Reservas por Cabañas
│   │   ├── Cabaña
│   │   ├── Temporada
│   │   └── Meses
│   └── Otros
│       ├── Medios de Pago
│       └── Medios de Contacto
└── Cerrar Sesión
```

### Convenciones de Estilo
- **Botones principales**: `btn-primary-custom` (turquesa)
- **Botones secundarios**: `btn-secondary-custom` (marrón)
- **Botones de peligro**: `btn-danger-custom` (rojo oscuro)
- **Tarjetas**: Bordes redondeados, sombras sutiles
- **Tablas**: Header turquesa, hover en filas

---

## ⚙️ Configuración

### Connection String (appsettings.json)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=ReservaCabanas;..."
  }
}
```

### Configuración de Email (para confirmaciones)
Se debe configurar en `Reservas/Confirmacion.cshtml.cs`:
- SMTP Server
- Puerto
- Credenciales

---

## 🔧 Convenciones de Código

### Nombres
- **Modelos**: PascalCase singular (ej: `Cliente`, `Reserva`)
- **Propiedades**: PascalCase (ej: `NombreCompleto`)
- **Métodos**: PascalCase con verbo (ej: `OnGetAsync`, `CargarReporte`)
- **Variables locales**: camelCase (ej: `totalIngresos`)

### Patrones
- **Soft Delete**: Usar campo `Activo` en lugar de eliminar registros
- **Validaciones**: En PageModel antes de guardar
- **Servicios**: Inyectados vía DI, interfaz + implementación
- **Async/Await**: Todos los métodos de acceso a BD son async

### Estructura de PageModel
```csharp
public class ExampleModel : PageModel
{
    private readonly AppDbContext _context;
    private readonly IAuthService _authService;

    [BindProperty]
    public MyModel MyModel { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Verificar permisos
        // Cargar datos
        // Retornar Page()
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Validar ModelState
        // Procesar datos
        // Guardar
        // Redirigir
    }
}
```

---

## 📝 Flujos Importantes

### Flujo de Reserva Completa
1. Usuario navega a `/Reservas/Create` (Step1)
2. Selecciona cabaña, fechas, temporada → Valida disponibilidad
3. Guarda en Session y redirige a Step2
4. Busca cliente por DNI o crea uno nuevo
5. Guarda en Session y redirige a Step3
6. Confirma datos, método de pago, estado
7. Crea registro en BD (Reserva + Cliente si es nuevo)
8. Envía email de confirmación
9. Muestra página de confirmación con opción de imprimir

### Flujo de Autenticación
1. GET `/Login` → Muestra formulario
2. POST `/Login` → `AuthService.Login()`
3. Si válido: Crear sesión, cookie → Redirige a `/Cabanas/Index`
4. Si inválido: Muestra error en formulario
5. En cada request: Filtros validan sesión/rol
6. `/Logout` → Limpia sesión → Redirige a Login

### Flujo de Exportación
1. Usuario genera reporte con filtros
2. Click en botón "Exportar Excel/PDF"
3. `OnGetExportarExcelAsync()` o `OnGetExportarPdfAsync()`
4. Cargar datos según filtros
5. Preparar `DatosExportacion` con columnas y filas
6. `ExportacionService.ExportarAExcel/Pdf()`
7. Retornar archivo para descarga

---

## 🗄️ Base de Datos

### Estrategia de Migraciones
- **Code First**: Modelos definen el esquema
- Comando para nueva migración: `dotnet ef migrations add NombreMigracion`
- Comando para aplicar: `dotnet ef database update`
- **Auto-Migration**: `Program.cs` ejecuta `db.Database.Migrate()` al inicio

### Tablas Principales
```
Usuarios
Clientes
Vehiculos (1:N con Clientes)
Cabanas
CabanaImagenes (1:N con Cabanas)
Temporadas
Reservas (N:1 Cabana, N:1 Cliente, N:1 Temporada)
MediosPago
DatosEmpresa
```

### Campos Comunes
- `Id`: int, auto-incremental
- `Activo/Activa`: bool (soft delete)
- `FechaCreacion`: DateTime

---

## 🚀 Despliegue y Ejecución

### Requisitos
- .NET 8.0 SDK
- SQL Server (LocalDB para desarrollo)
- Visual Studio 2022 o VS Code

### Comandos Útiles
```bash
# Restaurar paquetes
dotnet restore

# Compilar proyecto
dotnet build

# Ejecutar en desarrollo
dotnet run

# Aplicar migraciones
dotnet ef database update

# Crear nueva migración
dotnet ef migrations add NombreMigracion
```

### Variables de Entorno
- `ASPNETCORE_ENVIRONMENT`: Development / Production
- Connection String en appsettings.json

---

## 📌 Notas Importantes

### Seguridad
⚠️ **IMPORTANTE**: Las contraseñas se hashean con SHA256. En producción se recomienda migrar a bcrypt o similar.

### Soft Delete
Todos los registros se inactivan en lugar de eliminarse:
- Clientes: `Activo = false`
- Usuarios: `Activo = false`
- Cabañas: `Activa = false`
- Reservas: `Activa = false`

### Sesiones
- Timeout: 8 horas
- Sliding expiration habilitado
- Datos en sesión: UserId, UserName, UserRole, UserFullName

### Reportes
- Todos requieren rol Administrador
- Filtros persisten en formulario después de generar
- Exportaciones incluyen período y estadísticas

### Reservas
- No se permite solapamiento de fechas por cabaña
- El precio se calcula según la temporada vigente
- Estado de pago: "Pagado", "Pendiente", "Parcial"
- Estado de reserva: "Confirmada", "Pendiente", "Cancelada"

---

## 🔄 Historial de Cambios Recientes

### Octubre 2025
- ✅ Agregado gestión completa de usuarios (CRUD + inactivación)
- ✅ Creados reportes de Medios de Pago y Medios de Contacto
- ✅ Agregado submenú "Otros" en Reportes
- ✅ Cambiada eliminación por inactivación en usuarios

### Septiembre 2025
- ✅ Sistema base de reservas
- ✅ Gestión de clientes y cabañas
- ✅ Sistema de reportes por cliente y cabaña

---

## 📚 Recursos Adicionales

### Documentación Externa
- [ASP.NET Core Razor Pages](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Paper Dashboard 2](https://www.creative-tim.com/product/paper-dashboard-2)

### Archivos Clave para Entender el Proyecto
1. `Program.cs` - Configuración y startup
2. `Data/AppDbContext.cs` - Contexto de base de datos
3. `Services/AuthService.cs` - Autenticación
4. `Services/ExportacionService.cs` - Exportaciones
5. `Pages/Reservas/Step*.cshtml.cs` - Flujo de reservas
6. `Pages/Shared/_Layout.cshtml` - Menú principal

---

## 🤝 Contribuciones y Mantenimiento

### Para Desarrolladores Nuevos
1. Revisar este documento completo
2. Explorar `Program.cs` para entender configuración
3. Revisar modelos en `Models/`
4. Seguir flujo de una reserva completa (Step1-3)
5. Probar generación de reportes

### Buenas Prácticas
- Mantener convenciones de nombres
- Usar soft delete (Activo/Activa)
- Validar permisos con filtros
- Documentar cambios importantes
- Probar exportaciones después de cambios en modelos

---

**Última actualización**: Octubre 2025
**Mantenido por**: Diego Iraheta
