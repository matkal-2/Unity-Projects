using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace BOXRTS
{
    public static class PlayerManager
    {
        private struct PlayerDetails
        {
            private string name;
            public PlayerDetails(string name)
            {
                this.name = name;
            }
            public string Name { get { return name; } }
        }


        private static List<PlayerDetails> players = new List<PlayerDetails>();
        private static PlayerDetails currentPlayer;

        public static void SelectPlayer(string name)
        {
            //check player doesnt already exist
            bool playerExists = false;
            foreach (PlayerDetails player in players)
            {
                if (player.Name == name)
                {
                    currentPlayer = player;
                    playerExists = true;
                }
            }
            if (!playerExists)
            {
                Directory.CreateDirectory("SavedGames" + Path.DirectorySeparatorChar + name);
                PlayerDetails newPlayer = new PlayerDetails(name);
                players.Add(newPlayer);
                currentPlayer = newPlayer;
            }
            Save();
        }

        public static string GetPlayerName()
        {
            return currentPlayer.Name == "" ? "Unknown" : currentPlayer.Name;
        }

        public static void Save()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            using (StreamWriter sw = new StreamWriter("SavedGames" + Path.DirectorySeparatorChar + "Players.json"))
            {
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    writer.WriteStartObject();

                    writer.WritePropertyName("Players");
                    writer.WriteStartArray();
                    foreach (PlayerDetails player in players) SavePlayer(writer, player);
                    writer.WriteEndArray();

                    writer.WriteEndObject();
                }
            }
        }

        private static void SavePlayer(JsonWriter writer, PlayerDetails player)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("Name");
            writer.WriteValue(player.Name);

            writer.WriteEndObject();
        }

        public static void Load()
        {
            players.Clear();

            string filename = "SavedGames" + Path.DirectorySeparatorChar + "Players.json";
            if (File.Exists(filename))
            {
                //read contents of file
                string input;
                using (StreamReader sr = new StreamReader(filename))
                {
                    input = sr.ReadToEnd();
                }
                if (input != null)
                {
                    //parse contents of file
                    using (JsonTextReader reader = new JsonTextReader(new StringReader(input)))
                    {
                        while (reader.Read())
                        {
                            if (reader.Value != null)
                            {
                                if (reader.TokenType == JsonToken.PropertyName)
                                {
                                    if ((string)reader.Value == "Players") LoadPlayers(reader);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void LoadPlayers(JsonTextReader reader)
        {
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.StartObject) LoadPlayer(reader);
                else if (reader.TokenType == JsonToken.EndArray) return;
            }
        }

        private static void LoadPlayer(JsonTextReader reader)
        {
            string currValue = "", name = "";
            int avatar = 0;
            while (reader.Read())
            {
                if (reader.Value != null)
                {
                    if (reader.TokenType == JsonToken.PropertyName)
                    {
                        currValue = (string)reader.Value;
                    }
                    else
                    {
                        switch (currValue)
                        {
                            case "Name": name = (string)reader.Value; break;
                            default: break;
                        }
                    }
                }
                else
                {
                    if (reader.TokenType == JsonToken.EndObject)
                    {
                        players.Add(new PlayerDetails(name));
                        return;
                    }
                }
            }
        }

        public static string[] GetPlayerNames()
        {
            string[] playerNames = new string[players.Count];
            for (int i = 0; i < playerNames.Length; i++) playerNames[i] = players[i].Name;
            return playerNames;
        }
    }
}