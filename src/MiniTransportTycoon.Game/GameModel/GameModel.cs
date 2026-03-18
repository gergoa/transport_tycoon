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
        public bool IsGameOver { get; private set; } = false;
        public float ElapsedTime { get; private set; } = 0f;
        public Field[,] Board => board;
        public int Width => width;
        public int Height => height;
        public EconomyManager EconomyManager => economyManager;

        public event EventHandler? GameStarted;

        public GameModel()
        {
            StartNewGame(width, height);
        }

        public void StartNewGame(int width, int height)
        {
            //tábla generálás segédosztállyokkal
            MapGenerator generator = new MapGenerator(width, height);
            board = generator.Generate();

            OnGameStarted();
        }

        public void GameTick(float deltaTime)
        {
            float scaledTime = timeSystem.Tick(deltaTime);

            ElapsedTime += scaledTime;

            foreach (var facility in facilities)
            {
                facility.Tick(scaledTime);
            }

            if (economyManager.IsBankrupt())
            {
                GameOver();
            }
        }

        public void GameOver()
        {
            IsGameOver = true;
        }

        public void OnGameStarted()
        {
            GameStarted?.Invoke(this, EventArgs.Empty);
        }
    }
}