﻿namespace TechAid.Dto.ResponseDto
{
    public class DailyCount
    {
        public required string Day { get; set; } // e.g., Monday
        public required string Date { get; set; } // e.g., 2025-03-04
        public int TotalTickets { get; set; }
        public int ActiveTickets { get; set; }
        public int NotActiveTickets { get; set; }
        public int CompletedTickets { get; set; }
    }
}

