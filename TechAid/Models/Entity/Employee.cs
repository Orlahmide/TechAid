﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text.Json.Serialization;
using TechAid.Validation;

namespace TechAid.Models.Entity
{
    public class Employee
    {
        public Guid Id { get; set; }

        [EmailDomain("@optimusbank.com", ErrorMessage = "Email must be from @optimusbank.com domain.")]

        public required string Email { get; set; }

        public required string Password { get; set; }

        public required string Phone_number { get; set; }

        public required string First_name { get; set; }

        public required string Last_name { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required DateTime UpdatedAt { get; set; }

        [JsonIgnore]
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();

   
    }


}
