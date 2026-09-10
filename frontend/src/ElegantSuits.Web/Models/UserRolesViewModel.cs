namespace ElegantSuits.Web.Models
{
    public class UserRolesViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> CurrentRoles { get; set; }
        public List<string> AvailableRoles { get; set; }
        public List<string> SelectedRoles { get; set; }
    }
}
