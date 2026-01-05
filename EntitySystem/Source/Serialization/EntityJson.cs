using Newtonsoft.Json;
using UnityEngine;



public namespace EntitySystem.Serialization
{
    public static class EntityJson
    {
        // Gemeinsame JSON-Settings (für EntityGhost & Save/Load)
        public static readonly JsonSerializerSettings Settings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            // TypeNameHandling = TypeNameHandling.None // bewusst so lassen
        };
    }

    // Converter kannst du hier lassen (oder in eigene Dateien)
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 v, JsonSerializer s)
            => writer.WriteRawValue($"{{\"x\":{v.x},\"y\":{v.y},\"z\":{v.z}}}");

        public override Vector3 ReadJson(JsonReader r, System.Type t, Vector3 existingValue, bool hasExistingValue, JsonSerializer s)
        {
            var jo = Newtonsoft.Json.Linq.JObject.Load(r);
            return new Vector3((float)jo["x"], (float)jo["y"], (float)jo["z"]);
        }
    }

    public class QuaternionConverter : JsonConverter<Quaternion>
    {
        public override void WriteJson(JsonWriter w, Quaternion q, JsonSerializer s)
            => w.WriteRawValue($"{{\"x\":{q.x},\"y\":{q.y},\"z\":{q.z},\"w\":{q.w}}}");

        public override Quaternion ReadJson(JsonReader r, System.Type t, Quaternion existingValue, bool hasExistingValue, JsonSerializer s)
        {
            var jo = Newtonsoft.Json.Linq.JObject.Load(r);
            return new Quaternion((float)jo["x"], (float)jo["y"], (float)jo["z"], (float)jo["w"]);
        }
    }

    public class ColorConverter : JsonConverter<Color>
    {
        public override void WriteJson(JsonWriter w, Color c, JsonSerializer s)
            => w.WriteRawValue($"{{\"r\":{c.r},\"g\":{c.g},\"b\":{c.b},\"a\":{c.a}}}");

        public override Color ReadJson(JsonReader r, System.Type t, Color existingValue, bool hasExistingValue, JsonSerializer s)
        {
            var jo = Newtonsoft.Json.Linq.JObject.Load(r);
            return new Color((float)jo["r"], (float)jo["g"], (float)jo["b"], (float)jo["a"]);
        }
    }    
}
