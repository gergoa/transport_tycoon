using MiniTransportTycoon.Core.Vehicles;

namespace MiniTransportTycoon.UI.ViewModels
{
    public class ViewVehicle : ViewModelBase
    {
        private double _x;
        public double X
        {
            get => _x;
            set
            {
                if (_x != value)
                {
                    _x = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _y;
        public double Y
        {
            get => _y;
            set
            {
                if (_y != value)
                {
                    _y = value;
                    OnPropertyChanged();
                }
            }
        }
        public Vehicle CoreVehicleRef { get; set; } = null!;
    }
}