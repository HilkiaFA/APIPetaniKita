namespace APIPetaniKita.DTOs
{
    public class UserProfileDto
    {
        public string Username { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public List<string> Roles { get; set; }
    }
}
