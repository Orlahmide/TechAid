﻿namespace TechAid.Dto.ResponseDto
{
    public class WeeklyCount
    {
        
        public int WeekNumber { get; set; }
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int NotActiveTickets { get; set; }
        public int CompletedTickets { get; set; }
       
    }
}
