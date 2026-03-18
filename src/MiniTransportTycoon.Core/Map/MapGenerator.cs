using MiniTransportTycoon.Core.Facilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTransportTycoon.Core.Map
{
    public class MapGenerator
    {
        private readonly Random random = new Random();
        private readonly int _width;
        private readonly int _height;

        public MapGenerator(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public Field[,] Generate()
        {
            Field[,] board = new Field[_width, _height];

            GenerateTerrain(board);

            //+ide írnám a városok és gyárak lehelyezését is

            return board;
        }

        private void GenerateTerrain(Field[,] board)
        {
            double frequency = 0.07; //minnél kisebb annál jobban elkülönülnek a típusok
            double seedX = random.NextDouble() * 1000;
            double seedY = random.NextDouble() * 1000;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    double noise = Perlin.Noise(x * frequency + seedX, y * frequency + seedY);

                    FieldType type;
                    //int treeCount = 0;

                    // Küszöbértékek a Perlin zajhoz
                    if (noise < 0.30)
                    {
                        type = FieldType.WATER;
                    }
                    else if (noise > 0.65)
                    {
                        type = FieldType.FOREST;
                        //treeCount = _random.Next(1, 5);
                    }
                    else
                    {
                        type = FieldType.EMPTY;
                    }

                    board[x, y] = new Field(x, y, type);
                }
            }
        }
    }
}
