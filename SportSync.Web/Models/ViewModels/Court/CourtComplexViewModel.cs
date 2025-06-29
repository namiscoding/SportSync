namespace SportSync.Web.Models.ViewModels.Court
{
    public class CourtComplexViewModel
    {
        public int CourtComplexId { get; set; }
        public string Name { get; set; }
        public string OwnerPhoneNumber { get; set; }
        public string OwnerFullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string SportTypeName { get; set; }
        public bool IsActiveByOwner { get; set; }
    }
}