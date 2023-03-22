using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D2Traderie.Project.Models
{
    public enum PropertyType
    {
        numberType,
        booleanType,
        stringType
    }
    public class PropertyEntity
    {
        [JsonIgnore]
        public static Dictionary<string, PropertyType> propertyTypes = new Dictionary<string, PropertyType>()
        {
            {"number", PropertyType.numberType},
            {"bool", PropertyType.booleanType},
            { "string", PropertyType.stringType}
        };

        [JsonProperty("id")]
        public uint Id { get; set; }
        [JsonProperty("img")]
        public string? Img { get; set; }
        [JsonProperty("bool")]
        public bool? Bool { get; set; }
        [JsonProperty("number")]
        public int? Number { get; set; }
        [JsonProperty("type")]
        public string? Type { get; set; }
        //[JsonProperty("options")]
        //public OptionsEntity? Options { get; set; }
        [JsonProperty("property")]
        public string? Property { get; set; }

        public PropertyType GetPropertyType()
        {
            return propertyTypes[Type];
        }
    }
}
