namespace isc.time.report.be.domain.Models.Response.Projects
{
    public class GetAllProjectsResponse
    {
        public int Id { get; set; }
        public int ClientID { get; set; }
        public int ProjectStatusID { get; set; }
        public int? ProjectTypeID { get; set; }
        public int? LeaderID { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public DateTime? ActualEndDate { get; set; }
        public decimal? Budget { get; set; }
        public decimal Hours { get; set; }
        public bool? Status { get; set; }
        public DateTime? WaitingStartDate { get; set; }
        public DateTime? WaitingEndDate { get; set; }
        public string? Observation { get; set; }
        public Lider? Leader { get; set; }
    }

    public class Lider
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Phone { get; set; }
        public string Email { get; set; }
        public bool LeadershipType { get; set; }
    }
}