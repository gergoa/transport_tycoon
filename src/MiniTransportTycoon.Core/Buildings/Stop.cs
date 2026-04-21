using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Vehicles;

namespace MiniTransportTycoon.Core.Buildings
{
    public class Stop
    {
        private int waitingPassengers;
        private Dictionary<CargoType, int> cargoStorage;

        public Stop()
        {
            cargoStorage= new Dictionary<CargoType, int>();
        }

        //amount: a jármûben szabad hely
        //visszatérési érték: 

        /// <summary>Jármûbe tölti a rakományt</summary>
        /// <param name="amount">A jármûben szabad hely</param>
        /// <param name="type">A jármû rakomány típusa</param>
        /// <returns>A jármûbe töltött rakomány mennyisége</returns>
        public int LoadTo(CargoType type, int amount)
        {
            int n = 0;
            if(cargoStorage.TryGetValue(type, out n))
            {
                int loadamount = Math.Min(amount, n);
                cargoStorage[type] = n-loadamount;
                return loadamount;
            }
            return 0;
        }

        /// <summary>Jármûbõl kiszedi a rakományt</summary>
        /// <param name="amount">A rakomány mennyisége</param>
        /// <param name="type">A jármû rakomány típusa</param>
        public void UnloadFrom(CargoType type, int amount)
        {
            if(cargoStorage.ContainsKey(type))
            {
                cargoStorage[type] += amount;
            }
            else
            {
                cargoStorage.Add(type, amount);
            }
        }
    }
}