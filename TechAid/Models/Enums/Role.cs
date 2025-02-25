using System.Text.Json.Serialization;

namespace TechAid.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Role
    {
        ADMIN,

        BANK_STAFF,

        IT_PERSONNEL
    }
}
