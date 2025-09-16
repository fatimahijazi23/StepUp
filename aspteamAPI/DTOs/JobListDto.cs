namespace aspteamAPI.DTOs
{
    public class JobListDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal? MinSalaryRange { get; set; }
        public decimal? MaxSalaryRange { get; set; }
    }

}
