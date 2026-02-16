using System.Security.Cryptography;
using AshproApp.Data;
using AshproApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AshproApp.Services;

public sealed class AuthService
{
    private const int Iterations = 100_000;
    private const int KeySize = 32;

    private readonly AppDbContextFactory _dbContextFactory;

    public AuthService(AppDbContextFactory dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<(bool Success, string? Error, AppUser? User)> RegisterAsync(
        string email,
        string password,
        string confirmPassword,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(normalizedEmail) || !normalizedEmail.Contains('@'))
        {
            return (false, "Valid email is required.", null);
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            return (false, "Password must be at least 6 characters.", null);
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            return (false, "Passwords do not match.", null);
        }

        await using var db = _dbContextFactory.CreateDbContext();
        var exists = await db.AppUsers.AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
        if (exists)
        {
            return (false, "Email is already registered.", null);
        }

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = HashPassword(password, salt);

        var user = new AppUser
        {
            Email = normalizedEmail,
            PasswordSalt = Convert.ToBase64String(salt),
            PasswordHash = Convert.ToBase64String(hash),
            CreatedAt = DateTime.UtcNow
        };

        db.AppUsers.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return (true, null, user);
    }

    public async Task<(bool Success, string? Error, AppUser? User)> SignInAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(email);
        if (string.IsNullOrWhiteSpace(normalizedEmail) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Email and password are required.", null);
        }

        await using var db = _dbContextFactory.CreateDbContext();
        var user = await db.AppUsers.FirstOrDefaultAsync(item => item.Email == normalizedEmail, cancellationToken);
        if (user is null)
        {
            return (false, "Invalid email or password.", null);
        }

        if (!VerifyPassword(password, user.PasswordSalt, user.PasswordHash))
        {
            return (false, "Invalid email or password.", null);
        }

        return (true, null, user);
    }

    public async Task<AppUser?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return null;
        }

        await using var db = _dbContextFactory.CreateDbContext();
        return await db.AppUsers
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(
        int userId,
        string currentPassword,
        string newPassword,
        string confirmPassword,
        CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return (false, "Invalid user.");
        }

        if (string.IsNullOrWhiteSpace(currentPassword))
        {
            return (false, "Current password is required.");
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            return (false, "New password must be at least 6 characters.");
        }

        if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
        {
            return (false, "New password and confirmation do not match.");
        }

        await using var db = _dbContextFactory.CreateDbContext();
        var user = await db.AppUsers.FirstOrDefaultAsync(item => item.Id == userId, cancellationToken);
        if (user is null)
        {
            return (false, "User not found.");
        }

        if (!VerifyPassword(currentPassword, user.PasswordSalt, user.PasswordHash))
        {
            return (false, "Current password is incorrect.");
        }

        if (VerifyPassword(newPassword, user.PasswordSalt, user.PasswordHash))
        {
            return (false, "New password must be different from current password.");
        }

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = HashPassword(newPassword, salt);

        user.PasswordSalt = Convert.ToBase64String(salt);
        user.PasswordHash = Convert.ToBase64String(hash);

        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static byte[] HashPassword(string password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);
    }

    private static bool VerifyPassword(string password, string saltBase64, string hashBase64)
    {
        byte[] salt;
        byte[] storedHash;

        try
        {
            salt = Convert.FromBase64String(saltBase64);
            storedHash = Convert.FromBase64String(hashBase64);
        }
        catch
        {
            return false;
        }

        var computed = HashPassword(password, salt);
        return CryptographicOperations.FixedTimeEquals(storedHash, computed);
    }
}
