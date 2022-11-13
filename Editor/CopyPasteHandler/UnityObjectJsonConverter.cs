#nullable enable
using System;
using Newtonsoft.Json;
using UnityEditor;
using Object = UnityEngine.Object;

namespace com.michalpogodakotwica.graphite.Editor.CopyPasteHandler
{
    public class UnityObjectJsonConverter : JsonConverter<Object>
    {
        public override void WriteJson(JsonWriter writer, Object? value, JsonSerializer serializer)
        {
            if (value != null)
                writer.WriteValue(value.GetInstanceID().ToString());
            else
                writer.WriteNull();
        }

        public override Object? ReadJson(JsonReader reader, Type objectType, Object? existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.Value == null)
                return null;

            var readerValue = (string) reader.Value;
            return EditorUtility.InstanceIDToObject(int.Parse(readerValue));
        }
    }
}