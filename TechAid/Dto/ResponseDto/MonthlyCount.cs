namespace TechAid.Dto.ResponseDto
{
    public class MonthlyCount
    {
        public string? Month { get; set; }
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int NotActiveTickets { get; set; }
        public int CompletedTickets { get; set; }
        public string Day { get; internal set; }
        public string Date { get; internal set; }
    }

}
