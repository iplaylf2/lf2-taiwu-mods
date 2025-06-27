using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace LF2.Kit;

public static class StringSerializer
{
    public static string SerializeToString<T>(T obj)
    {
        if (obj == null) throw new ArgumentNullException(nameof(obj));

        using var stream = new MemoryStream();

        new BinaryFormatter().Serialize(stream, obj);

        return Convert.ToBase64String(stream.ToArray());
    }

    public static T DeserializeFromString<T>(string base64String)
    {
        byte[] bytes = Convert.FromBase64String(base64String);
        using var stream = new MemoryStream(bytes);

        var formatter = new BinaryFormatter
        {
            Binder = new CrossAssemblyBinder()
        };

        return (T)formatter.Deserialize(stream);
    }

    protected class CrossAssemblyBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return Type.GetType(typeName);
        }
    }
}
