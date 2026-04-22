using MiniTransportTycoon.Core.Cargo;

namespace MiniTransportTycoon.UI.ViewModels
{
    public class InventoryItem : ViewModelBase
    {
        public CargoType Cargo { get; set; }

        private int _amount;
        public int Amount
        {
            get => _amount;
            set { if (_amount != value) { _amount = value; OnPropertyChanged(); } }
        }
    }
}