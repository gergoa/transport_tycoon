using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Game.GameModel;
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

        public DelegateCommand TickCommand { get; private set; }

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

        public ObservableCollection<FieldType> Fields { get; private set; }

        public GameViewModel(GameModel model)
        {
            _model = model;

            TickCommand = new DelegateCommand(param => model.GameTick(1.0f));

            Fields = new ObservableCollection<FieldType>();

            for (int i=0;i<Width;i++)
            {
                for(int j=0;j<Height;j++)
                {
                    Fields.Add(_model.Board[i,j].Type);
                }
            }
        }
    }
}
