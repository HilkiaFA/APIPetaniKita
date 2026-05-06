namespace APIPetaniKita.DTOs
{
    public class RegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int RoleId { get; set; }
    }
}