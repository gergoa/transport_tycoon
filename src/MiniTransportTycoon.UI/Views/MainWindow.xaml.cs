using MiniTransportTycoon.Game.GameModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MiniTransportTycoon.Core.Map;

namespace MiniTransportTycoon.UI
{
    public partial class MainWindow : Window
    {
        GameModel model = new GameModel();

        public MainWindow()
        {
            InitializeComponent();
            //DrawMap();
        }

        /*private void DrawMap()
        {   
            var board = model.Board;

            MapGrid.Children.Clear();

            for (int i = 0; i < model.Width; i++)
            {
                for (int j = 0; j < model.Height; j++)
                {
                    var field = board[i, j];

                    //tudom, hogy ez nagyon béna, csak így volt a legegyszerűbb egyből látni
                    Rectangle r = new Rectangle { };
                    r.Fill = field.Type switch
                    {
                        FieldType.WATER => Brushes.Blue,
                        FieldType.FOREST => Brushes.DarkGreen,
                        FieldType.EMPTY => Brushes.LightGreen,
                        FieldType.ROAD => Brushes.Gray,
                        FieldType.FACILITY => Brushes.Red,
                        _ => Brushes.White
                    };
                    MapGrid.Children.Add(r);
                }
            }
        }*/
    }
}