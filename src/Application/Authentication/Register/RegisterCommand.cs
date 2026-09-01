namespace Application.Authentication.Register;

public sealed record RegisterCommand(
    string Email,
    string Password);