using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTransportTycoon.UI.ViewModels
{
    class MenuViewModel : ViewModelBase
    {
        public DelegateCommand StartGameCommand { get; private set; }

        public event EventHandler? StartGame;

        public MenuViewModel()
        {
            StartGameCommand = new DelegateCommand(param => OnGameStart());
        }

        private void OnGameStart()
        {
            StartGame?.Invoke(this, EventArgs.Empty);
        }
    }
}
