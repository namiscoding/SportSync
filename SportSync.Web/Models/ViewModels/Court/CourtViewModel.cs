namespace SportSync.Web.Models.ViewModels.Court
{
    public class CourtViewModel
    {
        public int CourtId { get; set; }
        public string Name { get; set; }
        public string SportTypeName { get; set; }
        public string StatusByOwner { get; set; }
        public bool IsActiveByOwner { get; set; } // Được tính toán từ StatusByOwner (Available = true, Suspended = false)
    }
}