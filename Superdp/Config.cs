using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Superdp
{
    public interface IHasId
    { 
        public string Id { get; set; }
    }
    public sealed class Client : IHasId
    {
        public class Properties
        {
            public string? Type { get; set; }
            public string Host { get; set; } = "";
            public string Name { get; set; } = "";
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
            public string Key { get; set; } = "";
            public long? LastConnected { get; set; }
        }
        public required string Id { get; set; }
        public required Properties Props { get; set; }
    }
    public sealed class DirectoryEntry : IHasId
    {
        public class Properties
        {
            public string Name { get; set; } = "";
            public bool Collapsed { get; set; }
        }
        public required string Id { get; set; }
        public required Properties Props { get; set; }
        public required List<string> Children { get; set; } = [];
        public bool Root { get; set; }
    }
    public sealed class Config
    {
        private Dictionary<string, Client> _clients = [];
        private Dictionary<string, DirectoryEntry> _dirEntries = [];
        public Client[] Clients {
            get => [.. _clients.Values];
            set => _clients = value.ToDictionary(client => client.Id);
        }
        public DirectoryEntry[] DirEntries
        {
            get => [.. _dirEntries.Values];
            set => _dirEntries = value.ToDictionary(dirEntry => dirEntry.Id);
        }

        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
        };

        public void Patch(Config config)
        {
            foreach (var (id, client) in config._clients)
                _clients[id] = client;

            foreach (var (id, dirEntry) in config._dirEntries)
                _dirEntries[id] = dirEntry;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this, jsonOptions);
        }
        public static Config? FromJson(string json)
        {
            return JsonSerializer.Deserialize<Config>(json, jsonOptions);
        }
    }
}
