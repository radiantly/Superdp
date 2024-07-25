using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Superdp
{
    // TODO: Remove this abomination
    internal sealed class DynJson : DynamicObject
    {
        private readonly JsonNode? root;

        private DynJson(JsonNode? node)
        {
            root = node;
        }

        public static dynamic? Parse(string jsonString)
        {
            var node = JsonSerializer.Deserialize<JsonNode>(jsonString);
            return Parse(node);
        }

        private static dynamic? Parse(JsonNode? node)
        {
            if (node is JsonObject jObj)
                return new DynJson(jObj);

            if (node is JsonArray jArr)
                return jArr.Select(nd => new DynJson(nd)).ToArray();

            if (node is JsonValue jValue)
            {
                switch (jValue.GetValue<JsonElement>().ValueKind)
                {
                    case JsonValueKind.String:
                        return jValue.GetValue<string>();
                    case JsonValueKind.Number:
                        if (jValue.TryGetValue(out int number))
                            return number;
                        return (int)(jValue.GetValue<double>() + 0.5);
                    case JsonValueKind.True:
                        return true;
                    case JsonValueKind.False:
                        return false;
                    case JsonValueKind.Null:
                        return null;
                }
            }

            return null;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = null;

            if (binder?.Name == null || root?[binder.Name] == null)
                return false;

            result = Parse(root[binder.Name]);
            return true;
        }
    }
    public interface IConnectionManager
    {
        public void Connect(dynamic options);
        public void Disconnect(dynamic options);
        public void Update(dynamic options);
        public void Transfer(dynamic options);
    }
}