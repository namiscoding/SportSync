
namespace SportSync.Web.Models.ViewModels.Admin
{
    public class CourtOwnerViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string AccountStatus { get; set; }
        public DateTime RegisteredDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public string Role { get; set; } // Thêm thuộc tính Role

    }
}
