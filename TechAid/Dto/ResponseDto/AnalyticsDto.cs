namespace TechAid.Dto.ResponseDto
{
    public class AnalyticsDto
    {
        public int TotalNumber { get; set; }
        public int ActiveNumber { get; set; }
        public int NotActiveNumber { get; set; }
        public int CompletedNumber { get; set; }
        public List<WeeklyCount>? WeeklyCounts { get; set; }
        public List<MonthlyCount>? MonthlyCounts { get; set; }

    }
}
