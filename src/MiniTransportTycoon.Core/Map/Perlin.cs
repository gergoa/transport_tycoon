using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTransportTycoon.Core.Map
{
    public static class Perlin
    {
        private static readonly int[] _perm;

        static Perlin()
        {
            var rand = new Random();
            _perm = Enumerable.Range(0, 256).OrderBy(x => rand.Next()).ToArray();
            _perm = _perm.Concat(_perm).ToArray();
        }

        public static double Noise(double x, double y)
        {
            int X = (int)Math.Floor(x) & 255;
            int Y = (int)Math.Floor(y) & 255;

            x -= Math.Floor(x);
            y -= Math.Floor(y);

            double u = Fade(x);
            double v = Fade(y);

            int a = _perm[X] + Y;
            int aa = _perm[a];
            int ab = _perm[a + 1];
            int b = _perm[X + 1] + Y;
            int ba = _perm[b];
            int bb = _perm[b + 1];

            double res = Lerp(v, Lerp(u, Grad(_perm[aa], x, y), Grad(_perm[ba], x - 1, y)),
                                 Lerp(u, Grad(_perm[ab], x, y - 1), Grad(_perm[bb], x - 1, y - 1)));

            return (res + 1) / 2;
        }

        private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);
        private static double Lerp(double t, double a, double b) => a + t * (b - a);
        private static double Grad(int hash, double x, double y)
        {
            int h = hash & 15;
            double u = h < 8 ? x : y;
            double v = h < 4 ? y : h == 12 || h == 14 ? x : 0;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }
    }
}
