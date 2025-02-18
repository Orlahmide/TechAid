using System.Text.Json.Serialization;

namespace TechAid.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Status
    {
        NOT_ACTIVE,

        ACTIVE,

        COMPLETED
    }
}
