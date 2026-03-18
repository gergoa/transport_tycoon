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
        public Field[,] board;
        public List<Facility> facilities;

        public TimeManager timeSystem;
        public EconomyManager economyManager = new EconomyManager();
        public Pathfinder pathfinder;

        public void StartNewGame()
        {
            int width = 100;
            int height = 100;

            board = new Field[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    board[x, y] = new Field(x, y, FieldType.EMPTY);
                }
            }
        }

        public void GameTick(float deltaTime)
        {

        }

        public void GameOver()
        {

        }
    }
}