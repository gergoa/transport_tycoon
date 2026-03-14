namespace MiniTransportTycoon.Game.Economy
{
    public class EconomyManager
    {
        private int money;

        public bool IsBankrupt()
        {
            return money < 0;
        }

        public int GetBalance()
        {
            return money;
        }
    }
}