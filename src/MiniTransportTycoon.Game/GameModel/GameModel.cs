using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Game.Economy;
using MiniTransportTycoon.Game.Pathfinding;
using MiniTransportTycoon.Game.Time;
using System.Collections.Generic;

namespace MiniTransportTycoon.Game.GameModel
{
    public class GameModel
    {
        private int width = 100;
        private int height = 100;
        private Field[,] board = null!;
        private List<Facility> facilities = new List<Facility>();
        private TimeManager timeSystem = new TimeManager();
        private EconomyManager economyManager = new EconomyManager();
        private Pathfinder pathfinder = new Pathfinder();

        public event EventHandler? GameStarted;

        public GameModel()
        {
            StartNewGame(width, height);
        }

        public Field[,] Board => board;
        public int Width => width;
        public int Height => height;

        public void StartNewGame(int width, int height)
        {
            //tábla generálás segédosztállyokkal
            MapGenerator generator = new MapGenerator(width, height);
            board = generator.Generate();

            OnGameStarted();
        }

        public void GameTick(float deltaTime)
        {

        }

        public void GameOver()
        {

        }

        public void OnGameStarted()
        {
            GameStarted?.Invoke(this, EventArgs.Empty);
        }
    }
}