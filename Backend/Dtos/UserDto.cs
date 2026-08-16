using WebApi.Dtos;

namespace WebApi.Dtos
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime LastLogin { get; set; }
    }
}

