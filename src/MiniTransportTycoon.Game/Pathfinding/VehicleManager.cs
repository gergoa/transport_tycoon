using System;
using System.Collections.Generic;
using MiniTransportTycoon.Game.Routes;
using MiniTransportTycoon.Core.Vehicles;
using MiniTransportTycoon.Core.Facilities;
using MiniTransportTycoon.Core.Map;
using MiniTransportTycoon.Game.Economy;
using MiniTransportTycoon.Core.Cargo;
using MiniTransportTycoon.Core.Buildings;

namespace MiniTransportTycoon.Game.Pathfinding
{
    /// <summary>
    /// a járművek kezeléséért felelős osztály, amely a járművek útvonalait, állapotát és viselkedését kezeli a játék során. Ez magában foglalja a járművek mozgását, rakodási és kirakodási folyamatát, valamint a karbantartási költségek kezelését.
    /// </summary>
    public class VehicleManager
    {
        private Dictionary<Route, List<Vehicle>> assignedRoutes = new();
        private Dictionary<Vehicle, Route> _vehicleRoutes = new();

        private const float transferRate = 5.0f;

        public void AssignVehicle(Route r, Vehicle v)
        {
            if (!assignedRoutes.ContainsKey(r)) assignedRoutes[r] = new List<Vehicle>();
            if (!assignedRoutes[r].Contains(v))
            {
                assignedRoutes[r].Add(v);
                _vehicleRoutes[v] = r;
                v.CurrentSpeed = v.MaxSpeed;
                v.State = VehicleState.MOVING;
            }
        }
        /// <summary>
        /// a járművek frissítése minden frame-ben, a jelenlegi állapotuk alapján meghatározva a viselkedésüket (mozgás, rakodás, kirakodás).
        /// </summary>
        /// <param name="deltaTime">Az eltelt idő a legutóbbi frissítés óta.</param>
        /// <param name="board">A játékmező mátrixa.</param>
        /// <param name="pathfinder">Az útvonaltervező objektum.</param>
        /// <param name="economy">A gazdasági rendszer kezelője.</param>
        /// <param name="vehicles">A játékban lévő járművek listája.</param>
        public void UpdateVehicles(float deltaTime, Field[,] board, Pathfinder pathfinder, EconomyManager economy, List<Vehicle> vehicles)
        {
            foreach (var vehicle in vehicles)
            {
                if (!_vehicleRoutes.TryGetValue(vehicle, out Route? route))
                    continue;

                switch (vehicle.State)
                {
                    case VehicleState.MOVING:
                        UpdateDrivingState(vehicle, route, board, pathfinder, deltaTime);
                        break;

                    case VehicleState.UNLOADING:
                        Stop? unloadStop = route.Stops.Find(s => s.AssignedField == vehicle.CurrentField);
                        if (unloadStop?.AssignedFacility != null)
                            UpdateUnloadingState(vehicle, unloadStop.AssignedFacility, economy, deltaTime);
                        break;

                    case VehicleState.LOADING:
                        Stop? loadStop = route.Stops.Find(s => s.AssignedField == vehicle.CurrentField);
                        if (loadStop?.AssignedFacility != null)
                            UpdateLoadingState(vehicle, route, loadStop.AssignedFacility, board, pathfinder, deltaTime);
                        break;
                }
            }
        }

        private void UpdateDrivingState(Vehicle vehicle, Route route, Field[,] board, Pathfinder pathfinder, float deltaTime)
        {

            if (vehicle.NextField == null) vehicle.NextField = route.Next(vehicle.CurrentField, board, pathfinder);

            vehicle.CurrentSpeed = vehicle.MaxSpeed;
            if (vehicle.NextField != null)
            {
                int dx = vehicle.NextField.X - vehicle.CurrentField.X;
                int dy = vehicle.NextField.Y - vehicle.CurrentField.Y;
                Vehicle? carAhead = (dx > 0 || dy < 0) ? vehicle.NextField.SlotR : vehicle.NextField.SlotL;

                if (carAhead != null && carAhead.PreviousField == vehicle.CurrentField)
                    vehicle.CurrentSpeed = Math.Min(vehicle.CurrentSpeed, carAhead.CurrentSpeed);
            }

            vehicle.Progress += vehicle.CurrentSpeed * deltaTime;

            if (vehicle.Progress >= 1.0f)
            {
                Stop? currentStop = route.Stops.Find(s => s.AssignedField == vehicle.CurrentField);

                if (currentStop != null && currentStop.AssignedFacility != null)
                {
                    // MOVING -> UNLOADING
                    vehicle.Progress = 1.0f;
                    vehicle.CurrentSpeed = 0f;
                    vehicle.elapsedInState = 0f;
                    vehicle.State = VehicleState.UNLOADING;
                }
                else
                {
                    TryEnterNextTile(vehicle, route, board, pathfinder);
                }
            }
        }

        /// <summary>
        /// a jármű kirakodási folyamatának kezelését végzi, ahol a járműben lévő rakományt a megfelelő létesítménybe szállítja, és frissíti a gazdasági rendszert a szállítási tranzakciók alapján.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="facility"></param>
        /// <param name="economy"></param>
        /// <param name="deltaTime"></param>
        private void UpdateUnloadingState(Vehicle vehicle, Facility facility, EconomyManager economy, float deltaTime)
        {
            // 1. Find all cargo in the truck that this facility actually wants
            var unloadableCargo = new List<CargoType>();
            foreach (var kvp in vehicle.currentCargoInventory)
            {
                if (kvp.Value > 0 && FacilityAcceptsCargo(facility, kvp.Key))
                {
                    unloadableCargo.Add(kvp.Key);
                }
            }

            if (unloadableCargo.Count == 0)
            {
                vehicle.State = VehicleState.LOADING;
                vehicle.elapsedInState = 0f;
                return;
            }

            // Accumulate time
            vehicle.elapsedInState += deltaTime;
            int itemsToUnload = (int)(vehicle.elapsedInState * transferRate);

            if (itemsToUnload > 0)
            {
                int totalUnloadedThisFrame = 0;

                foreach (var type in unloadableCargo)
                {
                    if (totalUnloadedThisFrame >= itemsToUnload) break;

                    int amountInTruck = vehicle.currentCargoInventory[type];
                    int toUnload = Math.Min(amountInTruck, itemsToUnload - totalUnloadedThisFrame);

                    DeliverCargoToFacility(facility, economy, type, toUnload);
                    vehicle.currentCargoInventory[type] -= toUnload;

                    totalUnloadedThisFrame += toUnload;
                }
                vehicle.elapsedInState -= (totalUnloadedThisFrame / transferRate);
            }
        }

        /// <summary>
        /// a jármű rakodási folyamatának kezelését végzi, ahol a jármű a létesítményből felveszi a rakományt, amire szüksége van a következő úti céljához, és frissíti a létesítmény készletét ennek megfelelően.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="route"></param>
        /// <param name="facility"></param>
        /// <param name="board"></param>
        /// <param name="pathfinder"></param>
        /// <param name="deltaTime"></param>
        private void UpdateLoadingState(Vehicle vehicle, Route route, Facility facility, Field[,] board, Pathfinder pathfinder, float deltaTime)
        {
            int currentTotalCargo = 0;
            foreach (var amount in vehicle.currentCargoInventory.Values)
            {
                currentTotalCargo += amount;
            }
            int spaceLeft = vehicle.CargoCapacity - currentTotalCargo;

            var loadableCargo = new List<CargoType>();
            foreach (var type in vehicle.CarriedTypes)
            {
                if (facility.InventoryOut.ContainsKey(type) && facility.InventoryOut[type] > 0)
                {
                    loadableCargo.Add(type);
                }
            }

            if (spaceLeft == 0 || loadableCargo.Count == 0)
            {
                vehicle.State = VehicleState.MOVING;
                vehicle.elapsedInState = 0f;
                TryEnterNextTile(vehicle, route, board, pathfinder);
                return;
            }

            // Accumulate time
            vehicle.elapsedInState += deltaTime;
            int itemsToLoad = (int)(vehicle.elapsedInState * transferRate);

            if (itemsToLoad > 0)
            {
                int totalLoadedThisFrame = 0;

                foreach (var type in loadableCargo)
                {
                    if (totalLoadedThisFrame >= itemsToLoad || spaceLeft == 0) break;

                    int amountWaiting = facility.InventoryOut[type];
                    int toLoad = Math.Min(amountWaiting, Math.Min(spaceLeft, itemsToLoad - totalLoadedThisFrame));

                    if (!vehicle.currentCargoInventory.ContainsKey(type)) vehicle.currentCargoInventory[type] = 0;

                    vehicle.currentCargoInventory[type] += toLoad;
                    facility.InventoryOut[type] -= toLoad;

                    totalLoadedThisFrame += toLoad;
                    spaceLeft -= toLoad;
                }

                vehicle.elapsedInState -= (totalLoadedThisFrame / transferRate);
            }
        }
        /// <summary>
        /// a jármű következő mezőre lépésének megkísérlését végzi, ellenőrizve, hogy a célmező szabad-e, és ha igen, akkor frissíti a jármű helyzetét és sebességét ennek megfelelően. Ha a célmező foglalt, akkor a jármű megáll, amíg az útvonal következő mezője el nem szabadul.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="route"></param>
        /// <param name="board"></param>
        /// <param name="pathfinder"></param>
        private void TryEnterNextTile(Vehicle vehicle, Route route, Field[,] board, Pathfinder pathfinder)
        {
            if (vehicle.NextField != null)
            {
                int dx = vehicle.NextField.X - vehicle.CurrentField.X;
                int dy = vehicle.NextField.Y - vehicle.CurrentField.Y;
                bool isTargetSlotFree = (dx > 0 || dy < 0) ? vehicle.NextField.SlotR == null : vehicle.NextField.SlotL == null;

                if (isTargetSlotFree)
                {
                    bool success = Pathfinder.MoveVehicle(vehicle, vehicle.CurrentField, vehicle.NextField);
                    if (success)
                    {
                        vehicle.PreviousField = vehicle.CurrentField;
                        vehicle.CurrentField = vehicle.NextField;
                        vehicle.NextField = route.Next(vehicle.CurrentField, board, pathfinder);
                        vehicle.Progress = 0.0f;
                        vehicle.CurrentSpeed = vehicle.MaxSpeed;
                    }
                }
                else
                {
                    vehicle.Progress = 1.0f;
                    vehicle.CurrentSpeed = 0f;
                }
            }
            else
            {
                vehicle.Progress = 1.0f;
                vehicle.CurrentSpeed = 0f;
            }
        }

        /// <summary>
        /// a járművek útvonalainak újraszámítását végzi, például amikor egy új útvonalat rendelünk egy járműhöz, vagy amikor a térképen változás történik (például egy új épület épül), ami miatt a járműveknek új útvonalakat kell találniuk a céljaik eléréséhez. Ez a metódus iterál a járműveken, és ha van hozzárendelt útvonaluk, akkor újraszámolja az útvonalukat a jelenlegi helyzetük és a térképen történt változások alapján.
        /// </summary>
        /// <param name="pathfinder"></param>
        /// <param name="vehicles"></param>
        public void RecalculateAllPaths(Pathfinder pathfinder, List<Vehicle> vehicles)
        {
            foreach (var vehicle in vehicles)
            {
                if (_vehicleRoutes.ContainsKey(vehicle))
                {
                    _vehicleRoutes[vehicle].RecalculatePath(vehicle.CurrentField, pathfinder);
                    vehicle.NextField = null!;
                }
            }
        }
        /// <summary>
        /// a járművek által szállított rakomány típusának ellenőrzését végzi, hogy a cél létesítmény elfogadja-e azt a rakománytípust, amelyet a jármű szállít. Ez a metódus ellenőrzi, hogy a létesítmény egy város-e, és ha igen, akkor megnézi, hogy a város keresletében szerepel-e a rakománytípus. Ha a létesítmény egy ipartelep, akkor megnézi, hogy az ipartelep bemeneti követelményei között szerepel-e a rakománytípus.
        /// </summary>
        /// <param name="facility"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool FacilityAcceptsCargo(Facility facility, CargoType type)
        {
            if (facility is City c && (c.Demand.ContainsKey(type))) return true;
            if (facility is Industry ind && ind.InputRequirements.ContainsKey(type)) return true;
            return false;
        }

        private void DeliverCargoToFacility(Facility facility, EconomyManager economy, CargoType type, int amount)
        {
            if (facility is City city)
            {
                economy.ProcessDelivery(type, amount, city);
                if (!city.InventoryIn.ContainsKey(type)) city.InventoryIn[type] = 0;
                city.InventoryIn[type] += amount;
            }
            else if (facility is Industry industry)
            {
                if (!industry.InventoryIn.ContainsKey(type)) industry.InventoryIn[type] = 0;
                industry.InventoryIn[type] += amount;
            }
        }

        public void ChargeMaintenance(EconomyManager economy, List<Vehicle> vehicles)
        {
            foreach (var vehicle in vehicles)
            {
                economy.SubtractMoney(vehicle.MaintenanceCost);
            }
        }
    }
}