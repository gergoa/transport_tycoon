namespace MiniTransportTycoon.Game.Economy
{
    public class EconomyManager
    {
        private int money = 100;

        public bool IsBankrupt()
        {
            return money < 0;
        }

        public int GetBalance()
        {
            return money;
        }

        public void SubtractMoney(int amount)
        {
            money -= amount;
        }

        public void ResetBalance()
        {
            money = 100;
        }
    }
}