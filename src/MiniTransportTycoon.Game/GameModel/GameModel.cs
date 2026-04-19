using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Game.Economy;
using MiniTransportTycoon.Core.Pathfinding;
using MiniTransportTycoon.Game.Time;
using System.Collections.Generic;
using MiniTransportTycoon.Core.Cargo;
using System;

namespace MiniTransportTycoon.Game.GameModel
{
    public class GameModel
    {
        private int width = 50;
        private int height = 50;
        private float _spawnTimer = 0f;
        private const float SpawnInterval = 20f;
        private Field[,] board = null!;
        private List<Facility> facilities = new List<Facility>();
        private List<Field> forestFields = new List<Field>();
        private TimeManager timeManager = new TimeManager();
        private EconomyManager economyManager = new EconomyManager();
        private Pathfinder pathfinder = new Pathfinder();
        public bool IsGameOver { get; private set; } = false;
        public float ElapsedTime { get; private set; } = 0f;
        public Field[,] Board => board;
        public List<Facility> Facilities => facilities;
        public int Width => width;
        public int Height => height;
        public EconomyManager EconomyManager => economyManager;
        public TimeManager TimeManager => timeManager;
        private Random _random = new Random();
        public event EventHandler? GameStarted;
        public event EventHandler? MapUpdated;

        //útgeneráláshoz használt akció
        public event Action<int, int, FieldType>? FieldChanged;

        public GameModel()
        {
            StartNewGame(width, height);
        }

        public void StartNewGame(int width, int height)
        {
            //tábla generálás segédosztállyokkal
            MapGenerator generator = new MapGenerator(width, height);
            board = generator.Generate();
            FacilityManager.CleanFacilities(this);
            FacilityManager.InitializeFacilities(this);
            forestFields.Clear();

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                {
                    var field = board[x, y];

                    if (field.Type == FieldType.FOREST)
                    {
                        field.SetForest(new Forest());
                        forestFields.Add(field);
                    }
                }

            economyManager.ResetBalance();
            ElapsedTime = 0f;

            OnGameStarted();
        }

        public void GameTick(float deltaTime, bool ignoreTimeScale = false)
        {
            float scaledTime = ignoreTimeScale
                ? deltaTime
                : timeManager.Tick(deltaTime);

            ElapsedTime += scaledTime;

            foreach (var field in forestFields.ToList())
            {
                field.Forest.Tick(scaledTime);

                if (field.Forest.UpdateSpread(scaledTime))
                    TrySpread(field);
            }
            UpdateSpawn(scaledTime);

            foreach (var facility in facilities)
            {
                facility.Tick(scaledTime);
            }

            if (economyManager.IsBankrupt())
            {
                GameOver();
            }

            /* for showcase purposes, TODO: Remove
            foreach (var facility in facilities)
            {
                if (facility is City c) ExpandCity(c);
            }*/

            MapUpdated?.Invoke(this, EventArgs.Empty);

        }

        public void BuildRoad(Field field)
        {
            int cost = 0;

            if (field.Type == FieldType.EMPTY) { cost = 1; }
            else if (field.Type == FieldType.FOREST) { cost = 2; }
            else { return; }

            if (EconomyManager.GetBalance() >= cost)
            {
                RemoveForest(field);
                EconomyManager.SubtractMoney(cost);

                field.Type = FieldType.ROAD;

                FieldChanged?.Invoke(field.X, field.Y, FieldType.ROAD);
            }
        }
        #region Forest management
        public void RemoveForest(Field field)
        {
            if (field.Forest == null)
                return;

            forestFields.Remove(field);
            field.SetForest(null);
        }
        private void TrySpread(Field field)
        {
            if (_random.NextDouble() > 0.01)
                return;

            int[] dx = { 0, 0, -1, 1 };
            int[] dy = { -1, 1, 0, 0 };

            var candidates = new List<Field>();

            for (int i = 0; i < 4; i++)
            {
                int nx = field.X + dx[i];
                int ny = field.Y + dy[i];

                if (nx < 0 || ny < 0 || nx >= Width || ny >= Height)
                    continue;

                var neighbor = Board[nx, ny];

                if (neighbor.Type != FieldType.EMPTY)
                    continue;

                candidates.Add(neighbor);
            }

            if (candidates.Count == 0)
                return;

            var target = candidates[_random.Next(candidates.Count)];

            target.Type = FieldType.FOREST;
            target.SetForest(new Forest());

            forestFields.Add(target);
        }
        private void UpdateSpawn(float deltaTime)
        {
            _spawnTimer += deltaTime;

            if (_spawnTimer >= SpawnInterval)
            {
                _spawnTimer = 0f;
                TrySpawnRandomForest();
            }
        }
        private void TrySpawnRandomForest()
        {
            if (_random.NextDouble() > 0.5)
                return;

            var emptyFields = new List<Field>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var f = Board[x, y];

                    if (f.Type == FieldType.EMPTY && f.Forest == null)
                        emptyFields.Add(f);
                }
            }

            if (emptyFields.Count == 0)
                return;

            var target = emptyFields[_random.Next(emptyFields.Count)];

            target.Type = FieldType.FOREST;
            target.SetForest(new Forest());

            forestFields.Add(target);
        }
        #endregion

        public void GameOver()
        {
            IsGameOver = true;
        }

        public void OnGameStarted()
        {
            GameStarted?.Invoke(this, EventArgs.Empty);
        }

        public bool ExpandCity(City city)
        {
            if (city == null || !facilities.Contains(city)) return false;

            return FacilityManager.TryGrowCity(this, city);
        }
        
    }
}