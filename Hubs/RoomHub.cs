using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SudokuGameApp.Hubs
{
    public class RoomHub : Hub
    {
        private static Dictionary<string, RoomState> Rooms = new Dictionary<string, RoomState>();

        public async Task JoinRoom(string gamePin, string playerName, string playerNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gamePin);

            if (!Rooms.ContainsKey(gamePin))
            {
                Rooms[gamePin] = new RoomState();
            }

            Rooms[gamePin].Players[playerNumber] = playerName;

            await Clients.Group(gamePin).SendAsync("PlayerJoined", playerName, playerNumber);
            await Clients.Group(gamePin).SendAsync("ReceiveMembersUpdate", Rooms[gamePin].Players);
        }

        public async Task LeaveRoom(string gamePin, string playerNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gamePin);

            if (Rooms.ContainsKey(gamePin))
            {
                Rooms[gamePin].Players.Remove(playerNumber);

                if (Rooms[gamePin].Players.Count == 0)
                {
                    Rooms.Remove(gamePin);
                }
                else
                {
                    await Clients.Group(gamePin).SendAsync("PlayerLeft", playerNumber);
                    await Clients.Group(gamePin).SendAsync("ReceiveMembersUpdate", Rooms[gamePin].Players);
                }
            }
        }

        public async Task StartGame(string gamePin)
        {
            if (Rooms.ContainsKey(gamePin) && !Rooms[gamePin].IsGameStarted)
            {
                Rooms[gamePin].IsGameStarted = true;
                await Clients.Group(gamePin).SendAsync("GameStarted");
            }
            else
            {
                await Clients.Caller.SendAsync("ErrorMessage", "Unable to start the game. It may have already started or the room doesn't exist.");
            }
        }

        public bool CheckGameStatus(string gamePin)
        {
            return Rooms.ContainsKey(gamePin) && Rooms[gamePin].IsGameStarted;
        }
    }

    public class RoomState
    {
        public Dictionary<string, string> Players { get; set; } = new Dictionary<string, string>();
        public bool IsGameStarted { get; set; } = false;
    }
}