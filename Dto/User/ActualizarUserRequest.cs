public class ActualizarUserRequest
{
    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? PhoneNumber { get; set; }
}