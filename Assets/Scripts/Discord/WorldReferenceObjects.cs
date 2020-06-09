using System;
using Newtonsoft.Json;

namespace Game.Mods
{
    public class AddressableWorldReferenceObject : WorldReferenceObjectBase
    {
        public string Address { get; set; }
        public Type Type { get; set; }
    }

    public abstract class WorldReferenceObjectBase
    {
    }

    public class WorldReferenceObjects
    {
        public WorldReferenceObjectBase[] Objects { get; set; }
    }


    public class TypeConverter : JsonConverter<Type>
    {
        public override void WriteJson(JsonWriter writer, Type value, JsonSerializer serializer)
        {
            var type = value;
            writer.WriteValue(type.FullName);
        }

        public override Type ReadJson(JsonReader reader, Type objectType, Type existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var text = reader.Value as string;
            return Type.GetType(text);
        }

        public override bool CanRead
        {
            get { return true; }
        }
    }
}