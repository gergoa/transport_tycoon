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
            get { return _model.economyManager.GetBalance(); }
        }

        public int SizeN
        {
            get { return _model.board.GetLength(0); }
        }

        public int SizeM
        {
            get { return _model.board.GetLength(1); }
        }

        public ObservableCollection<FieldType> Fields { get; private set; }

        public GameViewModel(GameModel model)
        {
            _model = model;

            TickCommand = new DelegateCommand(param => model.GameTick(1.0f));

            Fields = new ObservableCollection<FieldType>();

            for (int i=0;i<SizeN;i++)
            {
                for(int j=0;j<SizeM;j++)
                {
                    Fields.Add(_model.board[i,j].type);
                }
            }
        }
    }
}
