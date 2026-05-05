using MiniTransportTycoon.Core.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTransportTycoon.UI.ViewModels
{
    public class ViewField : ViewModelBase
    {
        private FieldType type;

        public FieldType Type
        {
            get { return type; }
            set
            {
                if (type != value)
                {
                    type = value;
                    OnPropertyChanged();
                }
            }
        }

        public int X {  get; set; }
        public int Y { get; set; }

        private bool _hasStop;
        public bool HasStop
        {
            get => _hasStop;
            set { if (_hasStop != value) { _hasStop = value; OnPropertyChanged(); } }
        }
        private BridgeType? _bridgeType;

        public BridgeType? BridgeType
        {
            get => _bridgeType;
            set
            {
                if (_bridgeType != value)
                {
                    _bridgeType = value;
                    OnPropertyChanged();
                }
            }
        }
        /*public Tuple<int, int> XY
        {
            get { return new(X, Y); }
        }*/

        //public DelegateCommand? FieldClickCommand { get; set; }
    }
}
