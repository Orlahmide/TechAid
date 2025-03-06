using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TechAid.Models.Enums;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace TechAid.Models.Entity
{
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public required string Subject { get; set; }
        public required string Description { get; set; }
        public string? Attachment { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required Category Category { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required Department Department { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required Priority Priority { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public required Status Status { get; set; }

        public required DateTime CreatedAt { get; set; }

        public required DateTime UpdatedAt { get; set; }

        public Guid EmployeeId { get; set; }

        public Guid? It_PersonnelId { get; set; }

        [ForeignKey("EmployeeId")]
        public Employee? Employee { get; set; }

        [ForeignKey("It_PersonnelId")]
        public Employee? ItPersonnel { get; set; }



        public String? Comment { get; set; }

    
    }
}
