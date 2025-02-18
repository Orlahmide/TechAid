using System.Text.Json.Serialization;

namespace TechAid.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Priority
    {
        LOW, MEDIUM, HIGH
    }
}
