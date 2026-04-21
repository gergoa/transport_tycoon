using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Core.Routes;

namespace MiniTransportTycoon.Core.Vehicles
{
    public abstract class Vehicle
    {
        public Field? PreviousField { get; set; }
        public Field CurrentField { get; set; }
        public Field? NextField { get; set; }
        public float MaxSpeed { get; set; } = 2.0f;
        public float CurrentSpeed { get; private set; } = 2f;
        public float Progress { get; private set; } = 0; // [0,1] range
        public int Capacity { get; private set; } = 10;
        public VehicleState State { get; private set; }
        public int MaintenanceCost { get; private set; } = 1;
        private CargoType cargoType;
        private int cargoAmount = 0;

        private Route route;
        private int stopIndex=0;
        private int fieldIndex=0;

        public Vehicle(CargoType type, Field field, Route route, float speed, int capacity, int maintenancecost)
        {
            CurrentField= field;
            cargoType = type;
            this.route = route;
            MaxSpeed= speed;
            Capacity = capacity;
            MaintenanceCost = maintenancecost;
        }

        public void tick(float deltaTime)
        {
            /*
             * Állapot alapján:
             * Elõszõr progress növelése
             * Ha elér egy küszöböt, akkor mûvelet végrehajtása
             * és következõ állapot beállítása, progress nullázása
            */
            switch(State)
            {
                case VehicleState.MOVING:
                    Progress += CurrentSpeed * deltaTime;
                    if (Progress >= 1)
                    {
                        //Move



                        //EndMove

                        if(CurrentField.Stop==route.GetStop(stopIndex))
                        {
                            State = VehicleState.UNLOADING;
                        }
                        Progress = 0;
                    }
                    break;
                case VehicleState.UNLOADING:
                    Progress += deltaTime;
                    if(Progress >= 1)
                    {
                        //Unload
                        CurrentField.Stop!.UnloadFrom(cargoType, cargoAmount);
                        cargoAmount = 0;

                        State = VehicleState.LOADING;
                        Progress = 0;
                    }
                    break;
                case VehicleState.LOADING:
                    Progress += deltaTime;
                    if(Progress>=1)
                    {
                        //Load
                        int loadAmount = CurrentField.Stop!.LoadTo(cargoType, Capacity - cargoAmount);
                        cargoAmount += loadAmount;

                        if (cargoAmount < Capacity) return; //Csak akkor megy tovább ha megtelt rakománnyal

                        State = VehicleState.MOVING;

                        //Következõ megálló kijelölése
                        stopIndex++;
                        if(stopIndex>route.Stops.Count)
                        {
                            stopIndex = 0;
                        }
                        Progress = 0;
                    }
                    break;

            }
        }
    }
}