# Guía de Instalación - Sistema de Gestión de Cabañas Aldea Auriel
## Instalación en Servidor de Producción

---

## ⚠️ Nota Importante sobre la Instalación

Esta guía está diseñada para instalar el sistema en un **servidor de producción con SQL Server local**.

**Punto crítico:** A diferencia del ambiente de desarrollo, en producción debe crear manualmente el usuario administrador ejecutando un script SQL (ver **Paso 3 - Sección B**). Este es un paso obligatorio y debe realizarse antes del primer inicio de sesión.

---

## 📋 Requisitos Previos del Servidor

### Sistema Operativo
- **Windows Server 2019** o superior (recomendado)
- **Windows 10/11 Pro** (alternativa)
- **Linux** (Ubuntu 20.04 LTS o superior) con compatibilidad .NET

### Hardware Mínimo Recomendado
- **Procesador**: 2 núcleos / 2.4 GHz o superior
- **RAM**: 4 GB mínimo (8 GB recomendado)
- **Disco Duro**: 50 GB de espacio libre
- **Conexión a Internet**: Estable para actualizaciones y envío de correos

---

## 🔧 Software Necesario para Instalar

### 1. .NET 8.0 Runtime y Hosting Bundle

**Para Windows:**
1. Descargar el **ASP.NET Core 8.0 Hosting Bundle** desde:
   - URL: https://dotnet.microsoft.com/download/dotnet/8.0
   - Buscar la sección "ASP.NET Core Runtime 8.0.x"
   - Descargar "Hosting Bundle" (incluye runtime y módulos IIS)

2. Ejecutar el instalador descargado
3. Reiniciar el servidor después de la instalación

**Para Linux:**
```bash
# Ubuntu/Debian
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get update
sudo apt-get install -y aspnetcore-runtime-8.0
```

**Verificar instalación:**
```bash
dotnet --version
# Debe mostrar 8.0.x o superior
```

---

### 2. SQL Server Express (Gratis)

1. Descargar SQL Server 2022 Express desde:
   - URL: https://www.microsoft.com/sql-server/sql-server-downloads

2. Durante la instalación:
   - Seleccionar **"Basic"** o **"Custom"**
   - Configurar autenticación en **"Mixed Mode"** (SQL Server + Windows)
   - Crear una contraseña para el usuario **"sa"** (anotarla de forma segura)
   - Anotar el **nombre de la instancia** (por defecto: `SQLEXPRESS`)

3. Instalar SQL Server Management Studio (SSMS) - **OBLIGATORIO**:
   - URL: https://docs.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms
   - Necesario para administrar la base de datos y ejecutar scripts de configuración

**Verificar instalación:**
- Abrir SQL Server Management Studio
- Conectarse con el usuario **sa** y la contraseña configurada
- Nombre del servidor: `localhost\SQLEXPRESS` (o solo `localhost` si es versión completa)

---

### 3. Internet Information Services (IIS) - Solo para Windows

**Habilitar IIS en Windows:**

1. Abrir **Panel de Control** → **Programas** → **Activar o desactivar las características de Windows**

2. Marcar las siguientes opciones:
   - ☑ Internet Information Services
   - ☑ Herramientas de administración web
     - ☑ Consola de administración de IIS
   - ☑ Servicios World Wide Web
     - ☑ Características de desarrollo de aplicaciones
       - ☑ ASP.NET 4.8
       - ☑ Extensibilidad de .NET 4.8
       - ☑ Extensiones ISAPI
       - ☑ Filtros ISAPI
     - ☑ Características HTTP comunes
       - ☑ Documento predeterminado
       - ☑ Examinador de directorios
       - ☑ Errores HTTP
       - ☑ Contenido estático

3. Hacer clic en **Aceptar** y esperar la instalación

4. Verificar:
   - Abrir navegador y visitar `http://localhost`
   - Debe mostrar la página de bienvenida de IIS

**Alternativa Linux: Nginx o Apache**
- Configurar como reverse proxy para la aplicación .NET

---

## 📦 Instalación de la Aplicación

### Paso 1: Obtener los Archivos de la Aplicación

**Opción A: Publicar desde el código fuente**

1. Instalar .NET 8.0 SDK en la computadora de desarrollo:
   - URL: https://dotnet.microsoft.com/download/dotnet/8.0

2. Abrir terminal en la carpeta del proyecto

3. Ejecutar comando de publicación:
```bash
dotnet publish -c Release -o ./publish
```

4. Los archivos publicados estarán en la carpeta `publish/`

**Opción B: Recibir carpeta publicada** (recomiendo)
- Si ya tiene la carpeta publicada, continuar al siguiente paso

-Dejo la carpeta publish en esta ruta: 

---

### Paso 2: Copiar Archivos al Servidor

1. Crear carpeta en el servidor:
   - Ubicación recomendada: `C:\inetpub\wwwroot\SistemaReservasCabanas\`
   - En Linux: `/var/www/reservas-cabanas/`

2. Copiar todos los archivos de la carpeta `publish/` a la ubicación del servidor

---

### Paso 3: Configurar la Base de Datos

#### A. Crear la Base de Datos y Tablas

1. Abrir **SQL Server Management Studio (SSMS)**

2. Conectarse al servidor local:
   - Servidor: `localhost\SQLEXPRESS` (o `localhost` si es versión completa)
   - Autenticación: **SQL Server Authentication**
   - Usuario: `sa`
   - Contraseña: (la que configuró durante la instalación)

3. Crear nueva base de datos:
   - Click derecho en **"Databases"** → **"New Database"**
   - Nombre: `ReservaCabanas`
   - Click en **OK**

4. **Crear las tablas del sistema:**
   - La aplicación está configurada para crear las tablas automáticamente en el primer inicio
   - Esto se hace mediante las migraciones de Entity Framework
   - **No es necesario ejecutar scripts SQL manualmente para las tablas**

#### B. Crear Usuario Administrador (DESPUÉS del primer inicio)

**⚠️ CRÍTICO:** Este paso debe realizarse **DESPUÉS** de iniciar la aplicación por primera vez para que las tablas se creen automáticamente.

**Orden correcto de pasos:**
1. Primero: Configurar todo (connection string, IIS, etc.)
2. Segundo: Iniciar la aplicación por primera vez (se crearán las tablas automáticamente)
3. Tercero: **Ejecutar este script** para crear el usuario administrador
4. Cuarto: Iniciar sesión en el sistema

**Procedimiento:**

1. **Después de iniciar la aplicación por primera vez**, abrir SQL Server Management Studio

2. Conectarse al servidor y expandir la base de datos `ReservaCabanas`

3. Verificar que existan las tablas (especialmente la tabla `Usuarios`)

4. Hacer click en **"New Query"**

5. Asegurarse de tener seleccionada la base de datos `ReservaCabanas` en el dropdown superior

6. Copiar y ejecutar el siguiente script SQL:

```sql
-- Crear usuario administrador principal
-- Usuario: admin
-- Contraseña: admin123
INSERT INTO Usuarios (NombreUsuario, Password, NombreCompleto, Rol, Activo, FechaCreacion)
VALUES (
    'admin',
    'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=', -- Hash SHA256 de 'admin123'
    'Administrador del Sistema',
    'Administrador',
    1,
    GETDATE()
);
```

7. Hacer click en **"Execute"** o presionar **F5**

8. Verificar que aparezca el mensaje: **(1 row affected)**

**Detalles de la cuenta creada:**
- **Usuario:** `admin`
- **Contraseña:** `admin123` (hasheada con SHA256 por seguridad)
- **Rol:** Administrador (acceso completo al sistema)

**⚠️ SEGURIDAD:** Después del primer login, debe cambiar inmediatamente esta contraseña desde el menú Usuarios del sistema.

#### C. Configurar Connection String

1. Abrir el archivo `appsettings.json` en la carpeta de la aplicación

2. Modificar el ConnectionString según su configuración:

**Para SQL Server Express (más común):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ReservaCabanas;User ID=sa;Password=SU_CONTRASEÑA_SA;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

**Para SQL Server versión completa:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ReservaCabanas;User ID=sa;Password=SU_CONTRASEÑA_SA;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

**⚠️ IMPORTANTE:**
- Reemplace `SU_CONTRASEÑA_SA` con la contraseña que configuró durante la instalación de SQL Server
- Si cambió el nombre de la instancia de SQL Server, actualice `localhost\\SQLEXPRESS` según corresponda
- Asegúrese de usar **doble barra invertida** `\\` para SQLEXPRESS

---

### Paso 4: Configurar Envío de Correos Electrónicos

El sistema envía correos de confirmación de reservas. Configure estos parámetros:

1. Abrir el archivo `appsettings.json`

2. Agregar o modificar la sección de Email:

```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUser": "su-correo@gmail.com",
    "SmtpPass": "su-contraseña-de-aplicacion"
  }
}
```

**Para Gmail:**
1. Crear una "Contraseña de aplicación" en su cuenta de Google:
   - Ir a: https://myaccount.google.com/security
   - Buscar "Contraseñas de aplicaciones"
   - Generar contraseña para "Correo"
   - Usar esa contraseña en `SmtpPass`

**Para otros proveedores:**
- Consultar la configuración SMTP de su proveedor de correo
- Actualizar `SmtpHost`, `SmtpPort` según corresponda

---

### Paso 5: Configurar IIS (Windows)

#### A. Crear Application Pool

1. Abrir **Administrador de IIS**
   - Buscar "IIS" en el menú inicio

2. En el panel izquierdo, expandir el servidor y hacer clic en **"Application Pools"**

3. En el panel derecho, hacer clic en **"Add Application Pool"**

4. Configurar:
   - **Name**: `SistemaReservasCabanas`
   - **.NET CLR version**: **"No Managed Code"** (importante para .NET Core/8)
   - **Managed pipeline mode**: `Integrated`
   - Hacer clic en **OK**

5. Click derecho en el pool creado → **"Advanced Settings"**
   - **Start Mode**: `AlwaysRunning` (para mejor rendimiento)
   - **Idle Time-out**: `0` (desactivar timeout)

#### B. Crear el Sitio Web

1. En el panel izquierdo, hacer clic derecho en **"Sites"** → **"Add Website"**

2. Configurar:
   - **Site name**: `SistemaReservasCabanas`
   - **Application pool**: Seleccionar `SistemaReservasCabanas` (el creado anteriormente)
   - **Physical path**: `C:\inetpub\wwwroot\SistemaReservasCabanas\` (la carpeta donde copió los archivos)
   - **Binding**:
     - Type: `http`
     - IP address: `All Unassigned`
     - Port: `80` (o el puerto que desee)
     - Host name: (dejar vacío o poner el dominio si tiene)

3. Hacer clic en **OK**

#### C. Permisos de Carpetas

1. Click derecho en la carpeta `C:\inetpub\wwwroot\SistemaReservasCabanas\`

2. Propiedades → Seguridad → Editar

3. Agregar permisos para:
   - **IIS_IUSRS**: Lectura y ejecución
   - **IUSR**: Lectura y ejecución

4. Especialmente importante: carpeta `wwwroot\uploads\`
   - Debe tener permisos de **Escritura** para IIS_IUSRS
   - Crear la carpeta si no existe: `C:\inetpub\wwwroot\SistemaReservasCabanas\wwwroot\uploads\`

---

### Paso 6: Configurar Nginx (Linux - Alternativa a IIS)

Si usa Linux, configure Nginx como reverse proxy:

1. Instalar Nginx:
```bash
sudo apt-get update
sudo apt-get install nginx
```

2. Crear archivo de configuración:
```bash
sudo nano /etc/nginx/sites-available/reservas-cabanas
```

3. Agregar configuración:
```nginx
server {
    listen 80;
    server_name su-dominio.com;  # o la IP del servidor

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

4. Habilitar el sitio:
```bash
sudo ln -s /etc/nginx/sites-available/reservas-cabanas /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

5. Crear servicio systemd para la aplicación:
```bash
sudo nano /etc/systemd/system/reservas-cabanas.service
```

```ini
[Unit]
Description=Sistema de Reservas de Cabanas

[Service]
WorkingDirectory=/var/www/reservas-cabanas
ExecStart=/usr/bin/dotnet /var/www/reservas-cabanas/ReservaCabanasSite.dll
Restart=always
RestartSec=10
SyslogIdentifier=reservas-cabanas
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

6. Iniciar el servicio:
```bash
sudo systemctl enable reservas-cabanas
sudo systemctl start reservas-cabanas
```

---

### Paso 7: Primera Ejecución y Creación del Usuario Admin

#### A. Iniciar la Aplicación por Primera Vez

1. **Iniciar el sitio en IIS**:
   - En el Administrador de IIS, click derecho en el sitio → **"Manage Website"** → **"Start"**

2. **Abrir navegador** y visitar:
   - `http://localhost` (si configuró puerto 80)
   - O `http://IP-DEL-SERVIDOR`

3. **Lo que sucederá automáticamente:**
   - La aplicación se conectará a la base de datos
   - Ejecutará las migraciones de Entity Framework
   - Creará todas las tablas necesarias (Usuarios, Clientes, Cabanas, Reservas, etc.)
   - Mostrará la página de Login

4. **En este punto NO PODRÁ iniciar sesión** porque aún no existe ningún usuario en la base de datos

#### B. Crear el Usuario Administrador

**⚠️ AHORA ES EL MOMENTO** de ejecutar el script SQL del **Paso 3 - Sección B**.

1. Mantener abierta la página de login en el navegador

2. Abrir SQL Server Management Studio (SSMS)

3. Ejecutar el script SQL para crear el usuario `admin` (ver Paso 3 - Sección B)

4. Verificar que el mensaje **(1 row affected)** aparezca

#### C. Verificar el Login

1. **Volver a la página de Login** en el navegador

2. **Iniciar sesión** con:
   - Usuario: `admin`
   - Contraseña: `admin123`

3. **Si todo está correcto**, verá:
   - Redirección automática al panel principal
   - Menú lateral con todas las opciones
   - Página de "Cabañas" como vista inicial

4. **¡Instalación exitosa!** El sistema está funcionando correctamente

#### D. Solución de Problemas

**Si no puede iniciar sesión con el usuario admin:**

1. Verificar que el script SQL se ejecutó correctamente:
   - Abrir SSMS
   - Ejecutar: `SELECT * FROM Usuarios WHERE NombreUsuario = 'admin'`
   - Debe aparecer un registro con el usuario admin

2. Si no aparece ningún registro:
   - Ejecutar nuevamente el script SQL del Paso 3 - Sección B
   - Verificar que esté seleccionada la base de datos `ReservaCabanas` antes de ejecutar

3. Si aparece "Usuario o contraseña incorrectos":
   - Verificar que está usando `admin` como usuario (en minúsculas)
   - Verificar que está usando `admin123` como contraseña
   - El sistema es sensible a mayúsculas/minúsculas

**Si aparece error 500 o página en blanco:**

1. Habilitar logs detallados:
   - Editar `web.config` en la carpeta de la aplicación
   - Cambiar `stdoutLogEnabled="false"` a `stdoutLogEnabled="true"`
   - Reiniciar el sitio en IIS
   - Revisar logs en la carpeta `logs/` dentro de la aplicación

2. Verificar que el Application Pool está corriendo:
   - En IIS → Application Pools
   - El pool debe estar en estado "Started"

3. Verificar connection string:
   - Asegurarse que SQL Server está corriendo
   - Verificar que el usuario y contraseña son correctos
   - Probar conexión con SSMS usando los mismos datos

**Si no puede conectarse a SQL Server:**

1. Verificar que SQL Server está corriendo:
   - Servicios de Windows → Buscar "SQL Server"
   - Debe estar "En ejecución"

2. Habilitar TCP/IP:
   - Abrir **SQL Server Configuration Manager**
   - SQL Server Network Configuration → Protocols for SQLEXPRESS
   - Habilitar **TCP/IP**
   - Reiniciar servicio SQL Server

3. Verificar firewall:
   - Permitir puerto 1433 para SQL Server

---

## 🔒 Configuración de Seguridad Post-Instalación

### 1. Cambiar Contraseñas por Defecto

**⚠️ CRÍTICO - Realizar inmediatamente después de la primera instalación:**

1. Ingresar al sistema con usuario `admin`
2. Ir a **Usuarios** en el menú
3. Editar el usuario `admin` y cambiar la contraseña
4. Editar el usuario `operador` y cambiar la contraseña
5. O inactivar el usuario `operador` si no lo necesita

### 2. Configurar HTTPS (Recomendado para Producción)

**Para IIS:**

1. Obtener certificado SSL:
   - **Opción gratuita**: Let's Encrypt (https://letsencrypt.org/)
   - **Opción paga**: Comprar certificado SSL

2. Instalar certificado en IIS:
   - Administrador de IIS → Certificados de servidor → Importar
   - Click derecho en el sitio → Edit Bindings
   - Agregar binding HTTPS en puerto 443
   - Seleccionar el certificado instalado

3. Forzar HTTPS:
   - La aplicación ya incluye redirección automática a HTTPS
   - Configurado en `Program.cs` con `app.UseHttpsRedirection()`

**Para Nginx:**
- Usar Certbot para Let's Encrypt:
```bash
sudo apt-get install certbot python3-certbot-nginx
sudo certbot --nginx -d su-dominio.com
```

### 3. Firewall

Permitir solo los puertos necesarios:
- **Puerto 80** (HTTP)
- **Puerto 443** (HTTPS)
- **Puerto 1433** (SQL Server) - SOLO si necesita acceso remoto a la BD

### 4. Backup de Base de Datos

Configurar backup automático:

1. En SQL Server Management Studio:
   - Click derecho en la base de datos `ReservaCabanas`
   - Tasks → Back Up
   - Configurar backup automático diario/semanal

2. Guardar backups en ubicación segura fuera del servidor

---

## 🎯 Configuración Inicial de la Aplicación

### Después del Primer Login

1. **Configurar Datos de la Empresa**:
   - Menú: **Ajustes** → **Datos de la Empresa**
   - Completar:
     - Nombre de la empresa
     - Dirección, teléfono, email
     - Sitio web
     - Términos y condiciones
     - Logo (aparecerá en reportes PDF)

2. **Crear Cabañas**:
   - Menú: **Cabañas** → **Nueva Cabaña**
   - Ingresar:
     - Nombre
     - Capacidad
     - Descripción
     - Precio base
     - Subir imágenes

3. **Configurar Temporadas**:
   - Menú: **Ajustes** → **Temporadas**
   - Crear temporadas (ej: Alta, Baja, Media)
   - Definir:
     - Fechas de inicio/fin
     - Precio por persona

4. **Configurar Medios de Pago**:
   - Menú: **Ajustes** → **Medios de Pago**
   - Agregar métodos aceptados (Efectivo, Transferencia, Tarjeta, etc.)

5. **Crear Usuarios Adicionales** (opcional):
   - Menú: **Usuarios** (solo Admin)
   - Crear usuarios con roles específicos

---

## 📞 Soporte y Mantenimiento

### Logs de la Aplicación

- **Ubicación**: `C:\inetpub\wwwroot\SistemaReservasCabanas\logs\`
- Revisar en caso de errores

### Actualizaciones

Cuando reciba una nueva versión:

1. Hacer backup de la base de datos
2. Hacer backup de `appsettings.json`
3. Detener el sitio en IIS
4. Reemplazar archivos (excepto `appsettings.json`)
5. Restaurar `appsettings.json` con sus configuraciones
6. Iniciar el sitio
7. Las migraciones se aplicarán automáticamente

### Información de Contacto

**Desarrollador**: Diego Iraheta
**Proyecto**: Sistema de Gestión de Cabañas - Aldea Auriel
**Versión**: 1.0.0

---

## ✅ Checklist de Instalación

Use esta lista para verificar que completó todos los pasos:

- [ ] .NET 8.0 Hosting Bundle instalado
- [ ] SQL Server Express instalado y corriendo
- [ ] SQL Server Management Studio (SSMS) instalado
- [ ] IIS instalado y configurado (Windows) o Nginx (Linux)
- [ ] Base de datos `ReservaCabanas` creada en SQL Server
- [ ] **Script SQL del usuario administrador ejecutado**
- [ ] Archivos de la aplicación copiados al servidor
- [ ] Connection String configurado en `appsettings.json`
- [ ] Configuración de Email completada en `appsettings.json`
- [ ] Application Pool creado (IIS)
- [ ] Sitio web creado en IIS o servicio systemd (Linux)
- [ ] Permisos de carpeta configurados
- [ ] Carpeta `wwwroot\uploads` creada con permisos de escritura
- [ ] Primera ejecución exitosa - página de login visible
- [ ] Login con usuario admin funciona correctamente
- [ ] Contraseña del admin cambiada
- [ ] Datos de la empresa configurados
- [ ] Al menos una cabaña creada
- [ ] Temporadas configuradas
- [ ] Medios de pago configurados
- [ ] HTTPS configurado (recomendado)
- [ ] Backup automático de BD configurado

---

**¡Instalación Completa!**

El sistema está listo para usar. Ingrese con el usuario `admin` y comience a gestionar las reservas de cabañas.
