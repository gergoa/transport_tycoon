namespace MiniTransportTycoon.Game.Time
{
    public class TimeManager
    {
        private TimeSpeed _currentSpeed = TimeSpeed.Normal;

        //public TimeSpeed CurrentSpeed => _currentSpeed;

        public void SetSpeed(TimeSpeed speed)
        {
            _currentSpeed = speed;
        }

        private float GetSpeedMultiplier()
        {
            return _currentSpeed switch
            {
                TimeSpeed.Paused => 0f,
                TimeSpeed.Normal => 1f,
                TimeSpeed.Fast => 2f,
                TimeSpeed.VeryFast => 4f,
                _ => 1f
            };
        }

        public float Tick(float deltaTime)
        {
            return deltaTime * GetSpeedMultiplier();
        }
    }
}