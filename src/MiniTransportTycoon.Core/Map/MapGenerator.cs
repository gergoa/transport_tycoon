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
            double frequency = 0.07; //minél kisebb annál jobban elkülönülnek a típusok
            double seedX = random.NextDouble() * 1000;
            double seedY = random.NextDouble() * 1000;

            //folyó - egyik oldalról szembe - random irányban és kanyarokkal/vastagsággal
            bool[,] isRiver = new bool[_width, _height];
            bool isHorizontal = random.Next(0, 2) == 0;
            int currX = isHorizontal ? 0 : random.Next(20, 80);
            int currY = isHorizontal ? random.Next(20, 80) : 0;

            while ((isHorizontal && currX < 100) || (!isHorizontal && currY < 100))
            {
                if (currX >= 0 && currX < 100 && currY >= 0 && currY < 100)
                {
                    isRiver[currX, currY] = true;
                    if (isHorizontal && currY + 1 < 100) isRiver[currX, currY + 1] = true;
                    if (!isHorizontal && currX + 1 < 100) isRiver[currX + 1, currY] = true;
                }

                int dir = random.Next(100);

                if (isHorizontal)
                {
                    if (dir < 30) currX++;
                    else if (dir < 65 && currY > 5) currY--;
                    else if (dir < 100 && currY < 95) currY++;
                }
                else
                {
                    if (dir < 30) currY++;
                    else if (dir < 65 && currX > 5) currX--;
                    else if (dir < 100 && currX < 95) currX++;
                }
            }

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    double noise = Perlin.Noise(x * frequency + seedX, y * frequency + seedY);

                    FieldType type;
                    //int treeCount = 0;

                    // Küszöbértékek a Perlin zajhoz
                    if ((noise < 0.25) || (isRiver[x,y]))
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
