using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SudokuGameApp.Hubs
{
    public class SudokuHub : Hub
    {
        private static ConcurrentDictionary<string, int[]> GameStates = new ConcurrentDictionary<string, int[]>();
        private static ConcurrentDictionary<string, bool> GameStarted = new ConcurrentDictionary<string, bool>();
        private static ConcurrentDictionary<string, string> ConnectionIdToPlayerName = new ConcurrentDictionary<string, string>();
        private static ConcurrentDictionary<string, ConcurrentDictionary<string, int>> GameScores = new ConcurrentDictionary<string, ConcurrentDictionary<string, int>>();

        public async Task JoinGame(string gamePin, string playerName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gamePin);

            if (!GameScores.ContainsKey(gamePin))
            {
                GameScores[gamePin] = new ConcurrentDictionary<string, int>();
            }
            GameScores[gamePin][playerName] = 0;

            await Clients.Group(gamePin).SendAsync("ReceiveAllScores", GameScores[gamePin]);
        }


        public async Task UpdateScore(string gamePin, string playerName, int score)
        {
            if (GameScores.ContainsKey(gamePin))
            {
                GameScores[gamePin][playerName] = score;
                await Clients.Group(gamePin).SendAsync("ReceiveAllScores", GameScores[gamePin]);
            }
        }

        public async Task NewGame(string gamePin, int[] board)
        {
            GameStates[gamePin] = board;
            GameStarted[gamePin] = true;
            GameScores[gamePin] = new ConcurrentDictionary<string, int>();
            await Clients.Group(gamePin).SendAsync("NewGame", board);
        }

        public async Task RequestGameState(string gamePin)
        {
            if (GameStates.TryGetValue(gamePin, out var board) && GameStarted.TryGetValue(gamePin, out bool started) && started)
            {
                await Clients.Caller.SendAsync("NewGame", board);
            }
        }

        public async Task RequestAllScores(string gamePin)
        {
            if (GameScores.ContainsKey(gamePin))
            {
                await Clients.Caller.SendAsync("ReceiveAllScores", GameScores[gamePin]);
            }
        }

        public async Task UpdateCell(string gamePin, string playerName, int row, int col, int value)
        {
            await Clients.Group(gamePin).SendAsync("UpdateCell", playerName, row, col, value);
        }

        public async Task PlayerCompletedGame(string gamePin, string playerName)
        {
            await Clients.Group(gamePin).SendAsync("PlayerCompleted", playerName);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (ConnectionIdToPlayerName.TryRemove(Context.ConnectionId, out string playerName))
            {
                foreach (var game in GameScores)
                {
                    if (game.Value.TryRemove(playerName, out _))
                    {
                        await Clients.Group(game.Key).SendAsync("ReceiveAllScores", game.Value);
                    }
                }
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}