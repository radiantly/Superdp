using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Superdp
{
    internal sealed class DynJson : DynamicObject
    {
        private readonly JsonNode? root;

        internal DynJson(JsonNode node)
        {
            root = node;
        }

        internal DynJson(string jsonString)
        {
            root = JsonSerializer.Deserialize<JsonNode>(jsonString);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            if (root != null)
            {
                var node = root[binder.Name];
                if (node is JsonObject jObj)
                {
                    result = new DynJson(jObj);
                    return true;
                }

                if (node is JsonValue jValue)
                {
                    switch (jValue.GetValue<JsonElement>().ValueKind)
                    {
                        case JsonValueKind.String:
                            result = jValue.GetValue<string>();
                            return true;
                        case JsonValueKind.Number:
                            result = jValue.GetValue<int>();
                            return true;
                        case JsonValueKind.True:
                            result = true;
                            return true;
                        case JsonValueKind.False:
                            result = false;
                            return true;
                        case JsonValueKind.Null:
                            result = null;
                            return true;
                    }
                }
            }

            result = null;
            return false;
        }
    }
    public interface IConnectionManager
    {
        public void Connect(dynamic options);
        public void Disconnect(dynamic options);
        public void Update(dynamic options);
    }
}