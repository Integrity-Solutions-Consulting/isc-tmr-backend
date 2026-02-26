namespace isc.time.report.be.domain.Models.Request.Leaders
{
    public class UpdateLeaderWithPersonIDRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool LeadershipType { get; set; }
    }
}
