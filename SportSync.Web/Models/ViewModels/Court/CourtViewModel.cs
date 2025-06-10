namespace SportSync.Web.Models.ViewModels.Court
{
    public class CourtViewModel
    {
        public int CourtId { get; set; }
        public string Name { get; set; }
        public string SportTypeName { get; set; }
        public string StatusByOwner { get; set; } // Thêm trường này để hiển thị trạng thái
        public bool IsActiveByAdmin { get; set; }
    }
}