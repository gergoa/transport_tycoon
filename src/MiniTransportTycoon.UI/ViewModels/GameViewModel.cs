using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Game.GameModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using MiniTransportTycoon.Game.Time;

namespace MiniTransportTycoon.UI.ViewModels
{
    class GameViewModel : ViewModelBase
    {
        private GameModel _model;
        private DispatcherTimer _timer;
        public DelegateCommand TickCommand { get; private set; }
        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand FieldClickCommand { get; }
        public DelegateCommand PauseCommand { get; private set; }
        public DelegateCommand NormalSpeedCommand { get; private set; }
        public DelegateCommand FastSpeedCommand { get; private set; }
        public DelegateCommand VeryFastSpeedCommand { get; private set; }

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

        public string DebugText { get; private set; } = string.Empty;

        public ObservableCollection<ViewField> Fields { get; private set; }

        public GameViewModel(GameModel model)
        {
            _model = model;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(100);
            _timer.Tick += OnTimerTick;
            _timer.Start();

            _model.GameStarted += _model_GameStarted;
            _model.MapUpdated += _model_MapUpdated;

            TickCommand = new DelegateCommand(param => { model.GameTick(0.1f, true); OnPropertyChanged(nameof(Time)); });

            NewGameCommand = new DelegateCommand(param => { model.StartNewGame(Width, Height); OnPropertyChanged(nameof(Money)); OnPropertyChanged(nameof(Time)); });

            PauseCommand = new DelegateCommand(_ =>
                _model.TimeManager.SetSpeed(TimeSpeed.Paused));

            NormalSpeedCommand = new DelegateCommand(_ =>
                _model.TimeManager.SetSpeed(TimeSpeed.Normal));

            FastSpeedCommand = new DelegateCommand(_ =>
                _model.TimeManager.SetSpeed(TimeSpeed.Fast));

            VeryFastSpeedCommand = new DelegateCommand(_ =>
                _model.TimeManager.SetSpeed(TimeSpeed.VeryFast));

            Fields = new ObservableCollection<ViewField>();

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
                }
            }      
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _model.GameTick(0.1f); // 100ms = 0.1 sec

            OnPropertyChanged(nameof(Time));
            OnPropertyChanged(nameof(Money));
        }

        private void OnFieldClick(object? param)
        {
            if (param is ViewField position)
            {
                _model.BuildRoad(_model.Board[position.X, position.Y]);

                OnPropertyChanged(nameof(Money));

                DebugText = $"X: {position.X} Y: {position.Y}";
                OnPropertyChanged(nameof(DebugText));
            }
        }

        private void OnFieldChanged(int x, int y, FieldType newType)
        {
            /*ViewField? targetField = null;

            foreach (var field in Fields)
            {
                if (field.X == x && field.Y == y)
                {
                    targetField = field;
                    break;
                }
            }
            if (targetField != null) { targetField.Type = newType; }*/

            if (0 <= x && x < Width && 0 <= y && y < Height)
            {
                Fields[y * Width + x].Type = newType;
            }
        }

        private void _model_GameStarted(object? sender, EventArgs e)
        {
            for (int j = 0; j < Height; j++)
            {
                for (int i = 0; i < Width; i++)
                {
                    Fields[j*Width+i].Type = _model.Board[i, j].Type;
                }
            }
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
                }
            }
        }
    }
}
