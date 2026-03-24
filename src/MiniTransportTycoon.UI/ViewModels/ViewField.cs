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

        public Tuple<int, int> XY
        {
            get { return new(X, Y); }
        }

        public DelegateCommand? FieldClickCommand { get; set; }
    }
}
