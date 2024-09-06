// Models/Room.cs
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SudokuGameApp.Models
{
    public class Room
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string GamePin { get; set; }

        [BsonElement("Members")]
        public Dictionary<string, string> Members { get; private set; } = new Dictionary<string, string>();
        public Dictionary<string, string> members { get; internal set; }

        [BsonIgnore]
        public bool IsFull => Members.Count >= 3;

        public bool IsGameStarted { get; set; }


        public int[,] SudokuGrid { get; set; }
        public int[,] SudokuSolution { get; set; }

        public string AddMember(string memberId)
        {
            if (IsFull) return null;
            string playerNumber = $"Player {Members.Count + 1}";
            Members[memberId] = playerNumber;
            return playerNumber;
        }

        public void UpdateMembers(Dictionary<string, string> newMembers)
        {
            Members.Clear();
            foreach (var member in newMembers)
            {
                Members.Add(member.Key, member.Value);
            }
        }
    }
}