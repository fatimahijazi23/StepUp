public class AuthResponseDto
{
    public int UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}