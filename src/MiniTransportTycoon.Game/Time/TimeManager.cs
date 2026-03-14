namespace MiniTransportTycoon.Game.Time
{
    public class TimeManager
    {
        public float speed = 1.0f;

        public float Tick(float deltaTime)
        {
            return deltaTime * speed;
        }
    }
}