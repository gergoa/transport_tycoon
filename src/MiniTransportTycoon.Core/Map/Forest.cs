using System;
using System.Collections.Generic;

namespace MiniTransportTycoon.Core.Map
{
    public class Forest
    {
        public int TreeCount { get; private set; } = 1;

        private float _growTimer = 0f;
        private float _spreadTimer = 0f;
        private const float SpreadInterval = 5f;


        public void Tick(float deltaTime)
        {
            _growTimer += deltaTime;

            if (_growTimer >= 5f)
            {
                _growTimer = 0f;

                if (TreeCount < 4)
                    TreeCount++;
            }
        }
        public bool UpdateSpread(float deltaTime)
        {
            _spreadTimer += deltaTime;

            if (_spreadTimer >= SpreadInterval && TreeCount >= 3)
            {
                _spreadTimer = 0f;
                return true;
            }

            return false;
        }
    }
}