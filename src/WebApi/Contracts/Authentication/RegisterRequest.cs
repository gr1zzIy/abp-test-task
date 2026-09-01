namespace WebApi.Contracts.Authentication;

/// <summary>
/// Дані для реєстрації нового клієнта.
/// </summary>
/// <param name="Email">Електронна пошта користувача.</param>
/// <param name="Password">Пароль користувача.</param>
public sealed record RegisterRequest(
    string Email,
    string Password);