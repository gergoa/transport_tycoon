using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Game.GameModel;
using MiniTransportTycoon.UI.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using MiniTransportTycoon.Game.Time;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Cargo;

namespace MiniTransportTycoon.UI.ViewModels
{
    public enum BuildMode
    {
        Road,
        Bridge,
        Stop
    }

    class GameViewModel : ViewModelBase
    {
        private GameModel _model;
        private DispatcherTimer _timer;
        private TickData tickMapData;
        private (int X, int Y)? _bridgeStart = null;
        public TickData TickData => tickMapData;
        public DelegateCommand TickCommand { get; private set; }
        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand FieldClickCommand { get; }
        public DelegateCommand PauseCommand { get; private set; }
        public DelegateCommand NormalSpeedCommand { get; private set; }
        public DelegateCommand FastSpeedCommand { get; private set; }
        public DelegateCommand VeryFastSpeedCommand { get; private set; }

        // for debug
        public DelegateCommand DebugGrowCitiesCommand { get; private set; }
        public DelegateCommand DebugSpawnVehicleCommand { get; private set; }

        // facility overlay, will have to abstract over or something
        private Facility? _selectedFacility;

        private bool _isOverlayVisible = false;
        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set { if (_isOverlayVisible != value) { _isOverlayVisible = value; OnPropertyChanged(); } }
        }
        private string _overlayTitle = string.Empty;
        public string OverlayTitle
        {
            get => _overlayTitle;
            set { if (_overlayTitle != value) { _overlayTitle = value; OnPropertyChanged(); } }
        }

        public ObservableCollection<InventoryItem> InventoryInList { get; } = new();
        public ObservableCollection<InventoryItem> InventoryOutList { get; } = new();

        public DelegateCommand CloseOverlayCommand { get; private set; }

        // build mode
        private BuildMode _currentBuildMode = BuildMode.Road;
        public BuildMode CurrentBuildMode
        {
            get => _currentBuildMode;
            set { if (_currentBuildMode != value) { _currentBuildMode = value; OnPropertyChanged(); } }
        }

        public DelegateCommand SelectRoadModeCommand { get; private set; }
        public DelegateCommand SelectBridgeModeCommand { get; private set; }
        public DelegateCommand SelectStopModeCommand { get; private set; }

        public int Money
        {
            get { return _model.EconomyManager.GetBalance(); }
        }

        public string Time
        {
            get { return _model.ElapsedTime.ToString("0.0"); }
        }

        public int Width
        {
            get { return _model.Width; }
        }

        public int Height
        {
            get { return _model.Height; }
        }
        public bool IsGameOver => _model.IsGameOver;

        public string DebugText { get; private set; } = string.Empty;

        public ObservableCollection<ViewField> Fields { get; private set; }
        public ObservableCollection<ViewVehicle> Vehicles { get; private set; } = new();

        public GameViewModel(GameModel model)
        {
            _model = model;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += OnTimerTick;
            _timer.Start();
            tickMapData = new TickData(new Field[Width,Height], Width, Height);

            _model.GameStarted += _model_GameStarted;
            _model.MapUpdated += _model_MapUpdated;

            TickCommand = new DelegateCommand(param => { model.GameTick(0.1f, true); OnPropertyChanged(nameof(Time)); });

            NewGameCommand = new DelegateCommand(param => { model.StartNewGame(Width, Height); OnPropertyChanged(nameof(Money)); OnPropertyChanged(nameof(Time)); _timer.Start(); });

            PauseCommand = new DelegateCommand(_ =>
                _model.TimeManager.SetSpeed(TimeSpeed.Paused));

            NormalSpeedCommand = new DelegateCommand(_ =>
                _model.TimeManager.SetSpeed(TimeSpeed.Normal));

            FastSpeedCommand = new DelegateCommand(_ =>
                _model.TimeManager.SetSpeed(TimeSpeed.Fast));

            VeryFastSpeedCommand = new DelegateCommand(_ =>
                _model.TimeManager.SetSpeed(TimeSpeed.VeryFast));

            DebugGrowCitiesCommand = new DelegateCommand(_ => _model.DebugGrowCities());

            DebugSpawnVehicleCommand = new DelegateCommand(_ =>
            {
                _model.DebugSpawnVehicle();
                SyncVehicles();
            });

            CloseOverlayCommand = new DelegateCommand(_ =>
            {
                IsOverlayVisible = false;
                _selectedFacility = null;
            });

            Fields = new ObservableCollection<ViewField>();
            Vehicles = new();

            SelectRoadModeCommand = new DelegateCommand(_ => CurrentBuildMode = BuildMode.Road);
            SelectBridgeModeCommand = new DelegateCommand(_ => CurrentBuildMode = BuildMode.Bridge);
            SelectStopModeCommand = new DelegateCommand(_ => CurrentBuildMode = BuildMode.Stop);

            _model.FieldChanged += OnFieldChanged;

            FieldClickCommand = new DelegateCommand(OnFieldClick);

            InitializeFields();
        }

        private void InitializeFields()
        {
            Fields.Clear();
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    Fields.Add(new ViewField
                    { Type = _model.Board[i, j].Type,
                      X = i, 
                      Y = j
                    });
                    tickMapData.Fields[i, j] = _model.Board[i, j];
                }
            }
            
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            if (_model.IsGameOver)
            {
                _timer.Stop();
                OnPropertyChanged(nameof(IsGameOver));
                return;
            }
            _model.GameTick(0.1f); // 100ms = 0.1 sec

            SyncVehicles();

            if (IsOverlayVisible)
            {
                SyncOverlay();
            }

            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(Money));
            OnPropertyChanged(nameof(IsGameOver));
        }

        private void OnFieldClick(object? param)
        {
            if (param is ViewField position)
            {

                var coreField = _model.Board[position.X, position.Y];

                // check if facility is clicked
                if (coreField.Facility != null)
                {
                    if (_selectedFacility != coreField.Facility)
                    {
                        _selectedFacility = coreField.Facility;
                        InventoryInList.Clear();
                        InventoryOutList.Clear();
                    }
                    OverlayTitle = _selectedFacility is City ? "City Inventory" : "Industry Inventory";
                    IsOverlayVisible = true;
                    SyncOverlay();
                }
                else
                {
                    switch (CurrentBuildMode)
                    {
                        case BuildMode.Road:
                            _model.BuildRoad(coreField);
                            break;
                        case BuildMode.Bridge:

                            if (_bridgeStart == null)
                            {
                                _bridgeStart = (position.X, position.Y);
                                DebugText = $"Bridge start set: {_bridgeStart.Value.X},{_bridgeStart.Value.Y}";
                                OnPropertyChanged(nameof(DebugText));
                                return;
                            }

                            var start = _model.Board[_bridgeStart.Value.X, _bridgeStart.Value.Y];
                            var end = coreField;

                            var path = _model.GetStraightLine(start, end);

                            if (path.Count == 0)
                            {
                                DebugText = "Invalid bridge (not straight)";
                                OnPropertyChanged(nameof(DebugText));
                                _bridgeStart = null;
                                return;
                            }

                            var type = GetBridgeTypeByLength(path.Count);

                            _model.BuildBridge(path, type);

                            _bridgeStart = null;
                            break;
                        case BuildMode.Stop:
                            _model.BuildStop(coreField);
                            break;
                    }
                    OnPropertyChanged(nameof(Money));
                }

                DebugText = $"X: {position.X} Y: {position.Y}";
                OnPropertyChanged(nameof(DebugText));
            }
        }

        private void OnFieldChanged(int x, int y, FieldType newType)
        {
            if (0 <= x && x < Width && 0 <= y && y < Height)
            {
                Fields[y * Width + x].Type = newType;
                Fields[y * Width + x].HasStop = _model.Board[x, y].HasStop;
                Fields[y * Width + x].BridgeType = _model.Board[x, y].Bridge?.Type;
            }

            tickMapData.Fields[x,y].Type = newType;
            
        }

        private void _model_GameStarted(object? sender, EventArgs e)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    Fields[j*Width+i].Type = _model.Board[i, j].Type;
                    tickMapData.Fields[i, j] = _model.Board[i, j];
                }
            }
            tickMapData.Height = Height;
            tickMapData.Width = Width;
        }

        private void _model_MapUpdated(object? sender, EventArgs e)
        {
            SyncMap();
        }

        private void SyncMap()
        {
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    Fields[j * Width + i].Type = _model.Board[i, j].Type;
                    tickMapData.Fields[i, j] = _model.Board[i, j];
                }
            }
        }

        public void HandleGridClick(int gridX, int gridY)
        {
            if (0 > gridX || gridX >= Width || 0 > gridY || gridY >= Height) return;
            var coreField = _model.Board[gridX, gridY];

            // check if facility is clicked
            if (coreField.Facility != null)
            {
                if (_selectedFacility != coreField.Facility)
                {
                    _selectedFacility = coreField.Facility;
                    InventoryInList.Clear();
                    InventoryOutList.Clear();
                }
                OverlayTitle = _selectedFacility is City ? "City Inventory" : "Industry Inventory";
                IsOverlayVisible = true;
                SyncOverlay();
            }
            else
            {
                switch (CurrentBuildMode)
                {
                    case BuildMode.Road:
                        _model.BuildRoad(coreField);
                        break;
                    case BuildMode.Bridge:

                            if (_bridgeStart == null)
                            {
                            _bridgeStart = (gridX, gridY);
                            DebugText = $"Bridge start set: {gridX},{gridY}";
                            OnPropertyChanged(nameof(DebugText));
                            return;
                        }

                            var start = _model.Board[_bridgeStart.Value.X, _bridgeStart.Value.Y];
                            var end = coreField;

                            var path = _model.GetStraightLine(start, end);

                            if (path.Count == 0)
                            {
                                DebugText = "Invalid bridge (not straight)";
                                OnPropertyChanged(nameof(DebugText));
                                _bridgeStart = null;
                                return;
                            }
                            path.Insert(0, start);

                            var type = GetBridgeTypeByLength(path.Count);

                            _model.BuildBridge(path, type);

                            _bridgeStart = null;
                            break;
                    case BuildMode.Stop:
                        _model.BuildStop(coreField);
                        break;
                }
                OnPropertyChanged(nameof(Money));
            }

            DebugText = $"X: {gridX} Y: {gridY}";
            OnPropertyChanged(nameof(DebugText));
        }

        private void SyncVehicles()
        {
            float tileSize = 10f;
            float laneOffset = 2f;

            Vehicles.Clear();

            foreach (var coreVehicle in _model.Vehicles)
            {
                float x = coreVehicle.CurrentField.X * tileSize + (tileSize / 2f);
                float y = coreVehicle.CurrentField.Y * tileSize + (tileSize / 2f);

                if (coreVehicle.NextField != null)
                {
                    float targetX = coreVehicle.NextField.X * tileSize + (tileSize / 2f);
                    float targetY = coreVehicle.NextField.Y * tileSize + (tileSize / 2f);

                    x += (targetX - x) * coreVehicle.Progress;
                    y += (targetY - y) * coreVehicle.Progress;

                }

                Vehicles.Add(new ViewVehicle { X = x, Y = y });
            }
        }

        private void SyncOverlay()
        {
            if (_selectedFacility == null) return;

            SyncDictToOBC(_selectedFacility.InventoryIn, InventoryInList);
            SyncDictToOBC(_selectedFacility.InventoryOut, InventoryOutList);
        }

        private void SyncDictToOBC(Dictionary<CargoType, int> sourceDict, ObservableCollection<InventoryItem> targetList)
        {
            foreach (var kv in sourceDict)
            {
                var existingItem = targetList.FirstOrDefault(i => i.Cargo == kv.Key);

                if (existingItem != null)
                {
                    existingItem.Amount = kv.Value;
                }
                else
                {
                    targetList.Add(new InventoryItem { Cargo = kv.Key, Amount = kv.Value });
                }
            }
        }
        private BridgeType GetBridgeTypeByLength(int length)
        {
            if (length <= 3) return BridgeType.Wooden;
            if (length <= 6) return BridgeType.Steel;
            return BridgeType.Highway;
        }
    }
}
