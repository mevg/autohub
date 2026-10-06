namespace AutoHub.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler(
    IUserQueries userQueries,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await userQueries.FindByEmailAsync(email, cancellationToken);

        // Mensaje genérico en todos los casos para no revelar si el email existe o si está inactivo.
        if (user is null || !user.IsActive || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Credenciales inválidas.");
        }

        var token = jwtTokenGenerator.GenerateToken(user);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            token
        );
    }
}
