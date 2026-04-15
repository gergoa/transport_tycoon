using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Game.GameModel;
using MiniTransportTycoon.UI.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTransportTycoon.UI.ViewModels
{
    class GameViewModel : ViewModelBase
    {
        private GameModel _model;
        private TickData tickMapData;
        public TickData TickData => tickMapData;
        public DelegateCommand TickCommand { get; private set; }
        public DelegateCommand NewGameCommand { get; private set; }
        public DelegateCommand FieldClickCommand { get; }

        public int Money
        {
            get { return _model.EconomyManager.GetBalance(); }
        }

        public float Time
        {
            get { return _model.ElapsedTime; }
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
            tickMapData = new TickData(new Field[Width,Height], Width, Height);

            _model.GameStarted += _model_GameStarted;
            _model.MapUpdated += _model_MapUpdated;

            TickCommand = new DelegateCommand(param => { model.GameTick(1.0f); OnPropertyChanged(nameof(Time)); });

            NewGameCommand = new DelegateCommand(param => { model.StartNewGame(Width, Height); OnPropertyChanged(nameof(Money)); OnPropertyChanged(nameof(Time)); });

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
                    tickMapData.Fields[i, j] = _model.Board[i, j];
                }
            }
            
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

            if (0 <= x && x < Width && 0 <= y && y < Height)
            {
                Fields[y * Width + x].Type = newType;
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
    }
}
