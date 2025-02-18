using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Text.Json.Serialization;

namespace TechAid.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Category
    {
        NETWORK,

        TRANSACTION,

        SOFTWARE,

        HARDWARE
    }
}
