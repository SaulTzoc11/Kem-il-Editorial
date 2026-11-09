using KemilEditorial.API.Data;
using Microsoft.EntityFrameworkCore;

namespace KemilEditorial.API.Services
{
    public static class PasswordMigrator
    {
        public const string PasswordTemporal = "Kemil2026!";

        public static async Task EjecutarAsync(AppDbContext db)
        {
            var pendientes = await db.Usuarios
                .Where(u => !u.PasswordHash.StartsWith("$2"))
                .ToListAsync();

            foreach (var u in pendientes)
                u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordTemporal, workFactor: 12);

            if (pendientes.Count > 0)
            {
                await db.SaveChangesAsync();
                Console.WriteLine($"[PasswordMigrator] {pendientes.Count} usuario(s) migrados. Contraseña temporal: {PasswordTemporal}");
            }
        }
    }
}