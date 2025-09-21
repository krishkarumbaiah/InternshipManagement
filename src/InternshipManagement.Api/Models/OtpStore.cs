public class OtpStore
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime Expiry { get; set; }
    public bool IsVerified { get; set; } = false;
}
