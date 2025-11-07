using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ReservaCabanasSite.Data;
using ReservaCabanasSite.Models;
using System.Security.Cryptography;
using System.Text;

namespace ReservaCabanasSite.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string nombreUsuario, string password);
        void Logout();
        bool IsAuthenticated();
        string GetCurrentUserRole();
        string GetCurrentUserName();
        int GetCurrentUserId();
        string HashPassword(string password);
    }

    public class AuthService : IAuthService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;

        public AuthService(IHttpContextAccessor httpContextAccessor, AppDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<bool> LoginAsync(string nombreUsuario, string password)
        {
            var hashedPassword = HashPassword(password);
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == nombreUsuario && 
                                        u.Password == hashedPassword && 
                                        u.Activo);

            if (usuario != null)
            {
                var session = _httpContextAccessor.HttpContext.Session;
                session.SetString("UserId", usuario.Id.ToString());
                session.SetString("UserName", usuario.NombreUsuario);
                session.SetString("UserFullName", usuario.NombreCompleto);
                session.SetString("UserRole", usuario.Rol);
                return true;
            }

            return false;
        }

        public void Logout()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            session.Clear();
        }

        public bool IsAuthenticated()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            return !string.IsNullOrEmpty(session.GetString("UserId"));
        }

        public string GetCurrentUserRole()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            return session.GetString("UserRole") ?? "";
        }

        public string GetCurrentUserName()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            return session.GetString("UserName") ?? "";
        }

        public int GetCurrentUserId()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            var userIdStr = session.GetString("UserId");
            return int.TryParse(userIdStr, out int userId) ? userId : 0;
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
} 