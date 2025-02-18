using System.Text.Json.Serialization;

namespace TechAid.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Department
    {
        SALES,

        MARKETING,

        CUSTOMER_SERVICE,

        OPERATIONS,

        TREASURY,

        HUMAN_RESOURCES
    }

}

