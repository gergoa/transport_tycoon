using System.Collections.Generic;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Facilities;

namespace MiniTransportTycoon.Game.Economy
{
    /// <summary>
    /// Gazdasági rendszer kezelése (pénz, költségek, szállítási bevételek).
    /// </summary>
    public class EconomyManager
    {
        private int money = 300;
        private const int PassengerValue = 5;


        /// <summary>
        /// egyenleg lekérdezése, a játékos pénzügyi helyzetének megjelenítéséhez.
        /// </summary>
        /// <returns></returns>
        public int GetBalance()
        {
            return money;
        }

        public bool IsBankrupt()
        {
            return money < 0;
        }

        public void ResetBalance()
        {
            money = 300;
        }
        /// <summary>
        /// rakomány szállításának feldolgozása, pénz hozzáadása a bevételekhez.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="amount"></param>
        /// <param name="city"></param>
        public void ProcessDelivery(CargoType type, int amount, City city)
        {
            if (amount <= 0) return;

            if (type == CargoType.Passengers)
            {
                money += PassengerValue * amount;
                return;
            }

            if (city.Demand.ContainsKey(type))
            {
                int value = CargoProperties.GetGrowthValue(type);
                money += value * amount;
            }
        }

        /*public void ApplyVehicleMaintenance(List<Vehicle> vehicles, float deltaTime)
        {
            foreach (var v in vehicles)
            {
                money -= (int)(v.MaintenanceCost * deltaTime);
            }
        }*/
        public bool DeductCost(int amount)
        {
            if (money < amount)
                return false;

            money -= amount;
            return true;
        }
        public void AddMoney(int amount)
        {
            money += amount;
        }
        /// <summary>
        /// Pénz levonása a költségek fedezésére (jármû fenntartás, útépítés, stb.).
        /// </summary>
        /// <param name="amount"></param>
        public void SubtractMoney(int amount)
        {
            money -= amount;
        }
    }
}