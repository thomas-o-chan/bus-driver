using GTA;
using GTA.Math;
using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Bus_Missions
{
    public class RefVector3
    {
        private Vector3 value = new Vector3(0f,0f,0f);
        public RefVector3(Vector3 initValue)
        {
            value = initValue;
        }
        public Vector3 Get()
        {
            return value;
        }
    }
    /// <summary>
    /// Allows passing of a simple boolean via get/set methods
    /// </summary>
    public class GetSet
    {
        private bool internalValue = false;
        public bool Get()
        {
            return internalValue;
        }
        public bool Set(bool newValue)
        {
            internalValue = newValue;
            return internalValue;
        }
    };
    public class BusList
    {
        private List<Vehicle> internalValue = new List<Vehicle>();
        public List<Vehicle> Get()
        {
            return internalValue;
        }
        public Vehicle Get(int number)
        {
            return internalValue[number];
        }
        public List<Vehicle> Add(Vehicle newValue)
        {
            internalValue.Add(newValue);
            return internalValue;
        }
        public List<Vehicle> Set(List<Vehicle> newValue)
        {
            internalValue = newValue;
            return internalValue;
        }
        public void Delete()
        {
            internalValue.ForEach((vehicle) =>
                {
                    vehicle.Delete();
                }
            );
            internalValue = new List<Vehicle>();
        }
    };
    public class ControlledInt
    {
        private int number = 0;
        public int Get()
        {
            return number;
        }
        public int Set(int newValue)
        {
            number = newValue;
            return number;
        }
    };
    public class Route
    {
        private int locationNumber = 0;
        private List<Vector3> locations = new List<Vector3>();

        public Route(List<Vector3> input)
        {
            locations = input;
            // add the bus station to the start of the list
            locations.Insert(0, new Vector3(0f, 0f, 0f));
        }
        public Vector3 Get()
        {
            if (locationNumber < locations.Count - 1)
            {
                return locations[locationNumber];
            }
            return locations.Last();
        }
        public List<Vector3> GetAll()
        {
            return locations;
        }
        public int GetIndex()
        {
            return locationNumber;
        }
        public bool IsLastStop()
        {
            return locationNumber >= locations.Count - 1;
        }
        public void Reset()
        {
            locationNumber = 0;
        }
        public Vector3 Next()
        {
            if (locationNumber < locations.Count - 1)
            {
                locationNumber++;
            }
            return locations[locationNumber];
        }
    };
    public class CurrentVehicle
    {
        private Vehicle internalValue;
        public Vehicle Get()
        {
            return internalValue;
        }
        public Vehicle Set(Vehicle newValue)
        {
            internalValue = newValue;
            return internalValue;
        }
    };
    class PedList
    {
        private List<Ped> internalValue = new List<Ped>();
        public List<Ped> Get()
        {
            return internalValue;
        }
        public List<Ped> Set(List<Ped> newValue)
        {
            internalValue = newValue;
            return internalValue;
        }
        public List<Ped> Add(Ped newValue)
        {
            internalValue.Add(newValue);
            return internalValue;
        }
        public List<Ped> AddList(List<Ped> newValue)
        {
            internalValue.AddRange(newValue);
            return internalValue;
        }
        public bool EnterVehicle(Vehicle bus, List<VehicleSeat> freeSeats)
        {
            if (internalValue.Count > freeSeats.Count)
            {
                return false;
            }
            int seatIndex = 0;
            internalValue.ForEach((Ped ped) =>
            {
                // remove peds that have errored on creation or are too far away
                if (ped == null)
                {
                    ped = World.CreateRandomPed(bus.Position.Around(4f));
                }
                else if (ped.Position.DistanceTo(bus.Position) > 30)
                {
                    ped.Position = bus.Position.Around(4f);
                }
                if (freeSeats.Count >= seatIndex)
                {
                    // set task to enter vehicle if not already in
                    if (!ped.IsInVehicle(bus))
                    {
                        ped.AddBlip();
                        ped.CurrentBlip.Sprite = BlipSprite.Standard;
                        ped.CurrentBlip.Color = BlipColor.BlueLight;
                        ped.Task.EnterVehicle(bus, freeSeats[seatIndex]);
                        seatIndex++;
                    }
                    else
                    {
                        ped.CurrentBlip.Remove();
                    }
                }
                else
                {
                // if they don't fit in the bus, make them walk away
                Vector3 goTo = new Vector3(0f, 0f, 0f);
                ped.Task.FollowPointRoute(goTo);
                ped.Money = 10;
                internalValue.Remove(ped);
                }
            });
            if (Mission.debug == true) UI.Notify("Ordered all peds to board");
            return true;
        }
        public void SomeExit()
        {
            int pedIndex = 0;
            int pedsToLeave = internalValue.Count / 2;
            internalValue.ForEach((Ped ped) => {
                if (internalValue.Count > 0 && pedsToLeave > pedIndex)
                {
                    ped.Task.LeaveVehicle();
                    ped.Task.WanderAround();
                    internalValue.Remove(ped);
                    pedIndex++;
                }
            });
        }
        public void AllExit()
        {
            internalValue.ForEach((Ped ped) => {
                ped.Task.LeaveVehicle();
                ped.Task.WanderAround();
                internalValue.Remove(ped);
            });
            if (Mission.debug == true) UI.Notify("Ordered all peds to get off");
        }
        public bool AllInVehicle(Vehicle bus)
        {
            bool allIn = true;
            internalValue.ForEach((Ped ped) =>
            {
                if (ped == null) internalValue.Remove(ped);
                if (ped.CurrentVehicle != bus && ped.IsAlive == true && ped.Money != 10)
                {
                    allIn = false;
                }
                else if(ped.CurrentBlip != null) ped.CurrentBlip.Remove();
            });
            if (Mission.debug == true) UI.Notify("Check: Ped list all on board: " + allIn.ToString());
            return allIn;
        }
        public bool TeleportToVehicle(Vehicle bus, List<VehicleSeat> freeSeats)
        {
            if (internalValue.Count > freeSeats.Count)
            {
                return false;
            }
            int seatIndex = 0;
            internalValue.ForEach((Ped ped) =>
            {
                if (freeSeats.Count > seatIndex)
                {
                    // teleport ped into vehicle
                    ped.SetIntoVehicle(bus, freeSeats[seatIndex]);
                    seatIndex++;
                }
                else
                {
                    // if they don't fit in the bus, make them walk away
                    Vector3 goTo = new Vector3(0f, 0f, 0f);
                    ped.Task.FollowPointRoute(goTo);
                    ped.Money = 10;
                }
            });
            if (Mission.debug == true) UI.Notify("Teleported waiting passengers");
            return true;
        }
        public void RemoveBlips()
        {
            internalValue.ForEach((Ped ped) => {
                ped.CurrentBlip.Remove();
                if (Mission.debug == true) UI.Notify("Removed pedesrrian blips");
            });
        }
    };

    class SaveState
    {
        public int Local { get; set; } = 0;
        public int Metro { get; set; } = 0;
        public int Express { get; set; } = 0;
        public int Tour { get; set; } = 0;
        public int Total { get; set; } = 0;

    }
    public class Mission : Script
    {

        static public ControlledInt routeNum = new ControlledInt();
        PedList pedsWaiting = new PedList();
        PedList pedsOnBoard = new PedList();
        GetSet Init = new GetSet();
        Blip Blip;
        /// <summary>
        /// Flag to indicate whether to update the blip
        /// </summary>
        bool isBusBlipShown = false;
        bool stopMessageShown = false;
        bool waitMessageShown = false;
        int teleportCounter = 10000;

        static public bool debug = false;

        SaveState state = new SaveState();

        static public string routeType = "local";
        List<Route> routes = new List<Route>();

        int messageTimer = 0;

        static public CurrentVehicle currentBus = new CurrentVehicle();
        static public CurrentVehicle previousCar = new CurrentVehicle();
        bool changeOrdered = false;
        bool exitsDone = false;
        ControlledInt passengerCount = new ControlledInt();
        public Mission()
        {
            KeyDown += OnKeyDown;
        }

        public void SetupScript()
        {
            Tick += OnTick;
            routes = MakeRoutes();
            var path = AppDomain.CurrentDomain.BaseDirectory + "\\bus-driver-save.ini";
            if (File.Exists(path))
            {
                string text = File.ReadAllText(path);
                var arrayForState = text.Split('\n');
                // parse the file 
                Array.ForEach(arrayForState, (string counter) =>
                {
                    string[] arr = counter.Split(':');
                    if (debug) UI.Notify("State: "+text);
                    if (arr.Length != 2) return;
                    if (arr[0] == "local")
                    {
                        state.Local = Int32.Parse(arr[1]);
                        state.Total += Int32.Parse(arr[1]);
                    }
                    if (arr[0] == "metro")
                    {
                        state.Metro = Int32.Parse(arr[1]);
                        state.Total += Int32.Parse(arr[1]);
                    }
                    if (arr[0] == "express")
                    {
                        state.Express = Int32.Parse(arr[1]);
                        state.Total += Int32.Parse(arr[1]);
                    }
                    if (arr[0] == "tour")
                    {
                        state.Tour = Int32.Parse(arr[1]);
                        state.Total += Int32.Parse(arr[1]);
                    }
                });
            }
            else
            {
                File.WriteAllText(path, "local:0\nmetro:0\nexpress:0\ntour:0");
            }
        }

        private void OnKeyDown(object o, KeyEventArgs e)
        {
            // Only set up the script once a key is pressed.
            // This allows the short delay needed for the references to be set up
            if (Init.Get() != true && e.KeyCode == Keys.W)
            {
                SetupScript();
                Init.Set(true);
            }
            else if (e.KeyCode == Keys.L && routes != null && routeNum != null && debug)
            {
                UI.Notify("Bus Driver: Forced next stage");
                MoveToNextStop(routes[routeNum.Get()]);
            }
            else if (e.KeyCode == Keys.Insert)
            {
                debug = !debug;
                if (debug) UI.Notify("Bus Driver: Debug Mode Active");
            }

            // add additional key handlers here
        }
        public void OnTick(object o, EventArgs e)
        {
            // guard against initialisation problems
            if (currentBus == null || currentBus.Get() == null || routeNum == null)
            {
                if (debug)
                {
                    if (currentBus == null) UI.Notify("Null in Mission.onTick - currentBus");
                    if (routeNum == null) UI.Notify("Null in Mission.onTick - routeNum");
                }
                return;
            }
            // make sure the bus is ok
            if (!IsBusOk(currentBus.Get()) && routeNum.Get() != 0)
            {
                UI.ShowHelpMessage("The bus is not serviceable. Return to the station to get another.");
                currentBus.Set(null);
                routeNum.Set(0);
                return;
            }
            else
            {
                RunMission(currentBus.Get(), routes);
                MoveVehiclesIfNeeded();
            }
        }

        private void RunMission(Vehicle bus, List<Route> routes)
        {
            int num = routeNum.Get();
            // show the subtitle
            if (messageTimer < 0)
            {
                messageTimer = 1000;
            }
            messageTimer--;
            // catch null refs
            if (bus == null || routes == null || state == null) return;
            // Currently running a route
            if (Game.Player.Character.IsInVehicle(bus))
            {
                World.DrawMarker(MarkerType.VerticalCylinder, Blip.Position, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(4f, 4f, 3f), Color.Yellow);
                // route index of zero means we haven't started yet, so push straight to the next one
                if (routes[num].GetIndex() == 0)
                {
                    MoveToNextStop(routes[num]);
                }
                else
                {
                    Vector3 stopLocation = routes[num].Get();
                    // if player is at the stop
                    if (bus.IsInRangeOf(stopLocation, 6f))
                    {
                        if (Game.Player.Character.Velocity.Length() == 0f)
                        {
                            teleportCounter--;
                            // if it is the last stop
                            if (routes[num].IsLastStop())
                            {
                                // mission passed
                                UI.ShowHelpMessage("Bus Route Successfully Completed. Well Done!");
                                // reset all values
                                pedsOnBoard.AllExit();
                                currentBus.Set(null);
                                if (Blip != null) Blip.Remove();
                                isBusBlipShown = false;
                                routes[num].Reset();
                                InitialiseStation.lastRoute = num;
                                // factor in the number of passengers, the type fo route, the player experience and the damage of the vehicle
                                float damage = (1000f - bus.BodyHealth) / 10;
                                int damageReduction = 100 - Convert.ToInt32(damage);
                                int passengers = passengerCount.Get();
                                int expMultiplier = state.Total / 4;
                                int cash = passengerCount.Get() * 100 * num * expMultiplier / (damageReduction + 1);
                                UI.Notify(
                                    "Summary:\nPassengers: " + passengers.ToString()
                                    + " x 100\nRoute difficulty multiplier: "+num.ToString()
                                    + "\nDamage: " + damage.ToString("0.0")
                                    + "%\nExperience: " + expMultiplier.ToString()
                                    + "\nTotal: $" + cash.ToString());
                                Game.Player.Money += cash;
                                // Save to the save file
                                if (routeType == "local") state.Local++;
                                if (routeType == "metro") state.Metro++;
                                if (routeType == "express") state.Express++;
                                if (routeType == "tour") state.Tour++;
                                var path = AppDomain.CurrentDomain.BaseDirectory + "\\bus-driver-save.ini";
                                File.WriteAllText(path,
                                    "local:" + state.Local.ToString()
                                    + "\nmetro:" + state.Metro.ToString()
                                    + "\nexpress:" + state.Express.ToString()
                                    + "\ntour:"+state.Tour.ToString());
                                }
                            else
                            {
                                // if passengers transit is done, set the next objective
                                if (pedsWaiting.AllInVehicle(bus))
                                {
                                    Vector3 blipLocation = MoveToNextStop(routes[num]);
                                    World.DrawMarker(MarkerType.VerticalCylinder, blipLocation, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(4f, 4f, 1f), Color.Yellow);
                                }
                                else
                                {
                                    // only tell the passengers to get on or off once, to avoid queueing tasks
                                    if (debug) UI.ShowSubtitle("Teleport Counter: " + teleportCounter.ToString());
                                    if (!changeOrdered)
                                    {
                                        // do shuffling of passengers and tell player to wait
                                        if (exitsDone == false)
                                        {
                                            pedsOnBoard.AllExit();
                                            exitsDone = true;
                                        }
                                        // The teleport counter also controls adding the task to enter the vehicle for waiting peds
                                        teleportCounter = 240;
                                        changeOrdered = true;
                                        if (debug) UI.Notify("Teleport counter started");
                                    }
                                    else if (teleportCounter < 0)
                                    {
                                        if (debug) UI.Notify("Teleporting passengers");
                                        // after 10 seconds just teleport the passengers inside
                                        pedsWaiting.TeleportToVehicle(bus, FreeSeats(bus));
                                        teleportCounter = 9999999;
                                    }
                                    // repeat the order every 3 seconds. Peds often forget tasks
                                    if (teleportCounter % 60 == 0)
                                    {
                                        pedsWaiting.EnterVehicle(bus, FreeSeats(bus));
                                        changeOrdered = true;
                                    }
                                    if (!waitMessageShown)
                                    {
                                        UI.ShowHelpMessage("Wait for the passengers to board");
                                        waitMessageShown = true;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (!stopMessageShown)
                            {
                                // tell player to stop
                                UI.ShowHelpMessage("Stop at the bus stop");
                                stopMessageShown = true;
                            }
                            changeOrdered = false;
                        }
                    }
                    else
                    {
                        UI.ShowHelpMessage("Drive to the next stop");
                        // reset the variables for handling stops
                        changeOrdered = false;
                        exitsDone = false;
                        stopMessageShown = false;
                        waitMessageShown = false;
                    }
                }
            }
            else
            {
                Vector3 markerLocation = bus.Position;
                if (isBusBlipShown != true)
                {
                    if (Blip != null) Blip.Remove();
                    Blip = World.CreateBlip(markerLocation);
                    Blip.Sprite = BlipSprite.Standard;
                    Blip.Color = BlipColor.Blue;
                    Blip.ShowRoute = true;
                    // tell player to get in the bus
                    UI.ShowHelpMessage("Get in the bus");
                    isBusBlipShown = true;
                }
                World.DrawMarker(MarkerType.UpsideDownCone, AddVect(markerLocation, new Vector3(0f, 0f, 5f)), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(2f, 2f, 1f), Color.DeepSkyBlue);
            }
        }
        private Vector3 MoveToNextStop(Route route)
        {
            if (passengerCount == null || pedsWaiting == null || pedsOnBoard == null && debug)
            {
                UI.Notify("Null Value in MoveToNextStop");
            }
            // move the marker to the next stop
            if (Blip != null) Blip.Remove();
            Vector3 nextLocation = route.Next();
            // Test to see if we can use the raw location
            //Vector2 markerIn2D = GetVec2(nextLocation);
            //float groundHeight = World.GetGroundHeight(markerIn2D);
            //if (groundHeight > 10f) nextLocation = new Vector3(markerIn2D.X, markerIn2D.Y, groundHeight);
            Blip = World.CreateBlip(nextLocation);
            Blip.Sprite = BlipSprite.Standard;
            Blip.Color = BlipColor.Yellow;
            Blip.ShowRoute = true;
            UI.ShowHelpMessage("Drive to the next stop");
            // update passenger lists
            pedsOnBoard.AddList(pedsWaiting.Get());
            pedsOnBoard.RemoveBlips();
            Wait(300);
            pedsWaiting.Set(new List<Ped>());
            int waiting = SpawnPassengers(nextLocation, 5);
            if (debug) UI.Notify("Spawned " + waiting.ToString() + " Peds");
            int currentPassengers = passengerCount.Get();
            passengerCount.Set(currentPassengers + pedsWaiting.Get().Count);
            // handle the ped lists
            return nextLocation;
        }
        /// <summary>
        /// Adds a random number of peds to the world near a location, returning these as a list.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        private int SpawnPassengers(Vector3 location, int multiplier)
        {
            // find the nearest pavement (sidewalk)
            Vector3 offsetLocation = World.GetNextPositionOnSidewalk(location);
            // if we didt find a pavement, just use the stop location instead
            if (offsetLocation == null)
            {
                offsetLocation = location;
            }
            // add a random offset
            Random rng = new Random();
            int count = rng.Next(1, multiplier);
            for (int i = 0; i < count; i++)
            {
                float pedLoc = Convert.ToSingle(i * 2);
                pedsWaiting.Add(World.CreateRandomPed(AddVect(offsetLocation, new Vector3(pedLoc, pedLoc, pedLoc))));
            }
            return count;
        }

        /// <summary>
        /// Returns the free seats available in a vehicle
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        private List<VehicleSeat> FreeSeats(Vehicle vehicle)
        {
            int maxSeats = vehicle.PassengerSeats;
            List<VehicleSeat> freeSeats = new List<VehicleSeat>();
            for (int i = 0; i < maxSeats; i++)
            {
                if (vehicle.IsSeatFree((VehicleSeat)i))
                {
                    freeSeats.Add((VehicleSeat)i);
                }
            }
            return freeSeats;
        }
        private List<Route> MakeRoutes()
        {
            List<Route> routes = new List<Route>();
            // usually best if the final stop is the terminal
            //Vector3 terminalStop = new Vector3(437.1692f, -1158.936f, 28.29196f);
            Vector3 terminalStop = new Vector3(429.4806f, -640.0822f, 27.5f);
            // add a blank route to catch if we fail to initialise
            List<Vector3> routeZero = new List<Vector3>();
            Route route0 = new Route(routeZero);
            /* TEST ROUTE
            List<Vector3> locs1 = new List<Vector3>();
            locs1.Add(new Vector3(489.0201f, -1130.798f, 28.4f));
            locs1.Add(new Vector3(499.548f, -1157.896f, 28.4f));
            locs1.Add(terminalStop);
            Route route1 = new Route(locs1); */

            List<Vector3> locs1 = new List<Vector3>();
            locs1.Add(new Vector3(227.67f, -702.11f, 35.81f));
            locs1.Add(new Vector3(235.19f, -346.34f, 44.23f));
            locs1.Add(new Vector3(337f, 50f, 89.91f));
            locs1.Add(new Vector3(275.51f, 179.94f, 104.5f));
            locs1.Add(new Vector3(105f, -71.22f, 65.01f));
            locs1.Add(new Vector3(-21.4f, -373.93f, 39.52f));
            locs1.Add(new Vector3(-163.20f, -791.11f, 30f));
            locs1.Add(new Vector3(138.17f, -1025.07f, 28.2f));
            locs1.Add(terminalStop);
            Route route1 = new Route(locs1);

            List<Vector3> locs2 = new List<Vector3>();
            locs2.Add(new Vector3(308.68f, -759.6f, 29.2f));
            locs2.Add(new Vector3(253.56f, -917.11f, 28.9f));
            locs2.Add(new Vector3(301.4f, -1136.59f, 28.7f));
            locs2.Add(new Vector3(402.8f, -1081.77f, 29.3f));
            locs2.Add(new Vector3(851.7f, -1010f, 28.7f));
            locs2.Add(new Vector3(1076.54f, -971.9f, 45.16f));
            locs2.Add(new Vector3(411.2f, -805.68f, 29.18f));
            locs2.Add(terminalStop);
            Route route2 = new Route(locs2);

            List<Vector3> locs3 = new List<Vector3>();
            locs3.Add(new Vector3(192.74f, 141.19f, 101.3f));
            locs3.Add(new Vector3(321.72f, 968.04f, 209.57f));
            locs3.Add(new Vector3(231.05f, 1173.27f, 225.46f));
            locs3.Add(new Vector3(-453.41f, 1391.52f, 297.04f));
            locs3.Add(new Vector3(-503.25f, 1199.08f, 323.96f));
            locs3.Add(new Vector3(-411.5f, 1174.78f, 325.64f));
            locs3.Add(new Vector3(-198.87f, 1306.96f, 304.42f));
            locs3.Add(new Vector3(231.05f, 1173.27f, 225.46f));
            Route route3 = new Route(locs3);

            List<Vector3> locs4 = new List<Vector3>();
            locs4.Add(new Vector3(457.15f, -654.61f, 27f));
            locs4.Add(new Vector3(216.82f, -936.97f, 20.14f));
            locs4.Add(new Vector3(184.87f, -1333.77f, 24.22f));
            locs4.Add(new Vector3(-138.67f, -1983.01f, 22.84f));
            locs4.Add(new Vector3(-1023.06f, -2495.94f, 13.69f));
            locs4.Add(new Vector3(-1045.76f, -2716.25f, 13.67f));
            locs4.Add(new Vector3(-146.7f, -1982.13f, 22.77f));
            locs4.Add(new Vector3(-265.84f, -1332.48f, 30.2f));
            locs4.Add(new Vector3(-160.6f, -857.2f, 28.7f));
            locs4.Add(new Vector3(457.15f, -654.61f, 27f));
            Route route4 = new Route(locs4);

            List<Vector3> locs5 = new List<Vector3>();
            locs5.Add(new Vector3(457.15f, -654.61f, 27f));
            locs5.Add(new Vector3(-1945.01f, -433.56f, 17.9f));
            locs5.Add(new Vector3(-1600.43f, -1043.36f, 11.5f));
            locs5.Add(new Vector3(-1282.3f, -1239.2f, 4f));
            locs5.Add(new Vector3(-1124.2f, -1336.3f, 5f));
            locs5.Add(new Vector3(-621.25f, -921.76f, 22.28f));
            locs5.Add(new Vector3(457.15f, -654.61f, 27f));
            Route route5 = new Route(locs5);

            List<Vector3> locs6 = new List<Vector3>();
            locs6.Add(new Vector3(458.14f, -640.605f, 28.4f));
            locs6.Add(new Vector3(970.83f, 184.44f, 80.83f));
            locs6.Add(new Vector3(2568.11f, 476.33f, 108.52f));
            locs6.Add(new Vector3(294.29f, -446.52f, 43.51f));
            locs6.Add(new Vector3(458.14f, -640.605f, 28.4f));
            Route route6 = new Route(locs6);

            List<Vector3> locs7 = new List<Vector3>();

            routes.Add(route0);
            routes.Add(route1);
            routes.Add(route2);
            routes.Add(route3);
            routes.Add(route4);
            routes.Add(route5);
            routes.Add(route6);
            return routes;
        }

        private void MoveVehiclesIfNeeded()
        {
            if (previousCar == null && previousCar.Get() == null) return;
            // delete destroyed vehicles instead
            if (!previousCar.Get().IsAlive)
            {
                previousCar.Set(null);
                return;
            }
            // move the player's car to a parking spot if they've left and we're on a mission
            if (Game.Player.Character.Position.DistanceTo(previousCar.Get().Position) > 50)
            {
                Vehicle car = previousCar.Get();
                car.Position = new Vector3(432.89f, -616.27f, 27.9f);
                car.Rotation = new Vector3(0f, 0f, 85.63f);
            }
        }
        private bool IsBusOk(Vehicle bus)
        {
            if (bus.IsDriveable && !bus.IsOnFire) return true;
            if (Blip != null) Blip.Remove();
            return false;
        }

        private Vector3 AddVect(Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(vec1.X + vec2.X, vec1.Y + vec2.Y, vec1.Z + vec2.Z);
        }
        private Vector2 GetVec2(Vector3 vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

    }
    public class InitialiseStation : Script
    {
        GetSet Init = new GetSet();
        GetSet done = new GetSet();
        BusList buses = new BusList();
        bool isMessageShown = false;
        static public int lastRoute = 0;
        RefVector3 terminal = new RefVector3(new Vector3(434.8162f, -651.6543f, 27.73288f));
        public InitialiseStation()
        {
            KeyDown += OnKeyDown;
        }
        public void SetupScript()
        {
            Wait(1000);
            Tick += OnTick;
        }

        private void OnKeyDown(object o, KeyEventArgs e)
        {
            // Only set up the script once a key is pressed.
            // This allows the short delay needed for the references to be set up
            if (Init.Get() != true && e.KeyCode == Keys.W)
            {
                SetupScript();
                Init.Set(true);
            }
            else
            {
                // add additional key handlers here
                StartMission(o, e);
            }

        }

        private void StartMission(object o, KeyEventArgs eventArgs)
        {
            // guard against initialisation problems
            if (done == null || Mission.currentBus == null || buses == null || terminal == null) {
                if (Mission.debug == true)
                {
                    if (Mission.currentBus == null) UI.Notify("Null in Init.startMission - Mission.currentBus");
                    if (buses == null) UI.Notify("Null in Init.startMission - buses");
                    if (terminal == null) UI.Notify("Null in Init.startMission - terminal");
                    if (done == null) UI.Notify("Null in Init.startMission - done");
                }
                return;
            }
            // make sure the buses have been placed and mission is not underway
            if (done.Get() == true && Mission.currentBus.Get() == null)
            {
                // if player is by the mission dispenser and everything is ready
                // todo check for wanted level, mission done and that the bus to use isn't destroyed.
                if (Game.Player.Character.IsInRangeOf(terminal.Get(), 2) && eventArgs.KeyCode == Keys.E && Game.Player.Character.IsInVehicle() == false)
                {
                    SetupAndStartMission();
                }
            }
        }

        private void SetupAndStartMission()
        {
            // save player vehicle (we'll move it later if we need to)
            Vehicle playerVehicle = Game.Player.Character.LastVehicle;
            playerVehicle.IsPersistent = true;
            if (Mission.previousCar != null)
            {
                if (Mission.previousCar.Get() != null) Mission.previousCar.Get().Delete();
                Mission.previousCar.Set(playerVehicle);
            }
            // randomly generate route number
            Random rng = new Random();
            int route = rng.Next(1, 6);
            // don't repeat routes
            if (route == lastRoute) route++;
            UI.ShowSubtitle("You have been assigned Route " + route.ToString());

            // pick a bus of the right type/colour
            int busIndex = 0;
            if (route == 3 || route == 4)
            {
                Mission.routeType = "metro";
                busIndex = 1;
            }
            if (route == 5 || route == 6)
            {
                Mission.routeType = "express";
                busIndex = 2;
            }
            if (route == 7)
            {
                Mission.routeType = "tour";
                busIndex = 3;
            }
            Vehicle busToUse = buses.Get(busIndex);
            if (busToUse == null)
            {
                UI.Notify("No available buses. Use the replacement bus.");
                busToUse = World.CreateVehicle(new Model(-713569950), new Vector3(418.87f, -610.8074f, 28.64928f), 0f);
                busToUse.PrimaryColor = VehicleColor.MetallicOrange;
            }
            busToUse.Repair();
            busToUse.LockStatus = VehicleLockStatus.Unlocked;
            Mission.currentBus.Set(busToUse);
            Mission.routeNum.Set(route);
        }
        private void OnTick(object o, EventArgs e)
        {
            // guard against initialisation problems
            if (done == null || terminal == null || Mission.currentBus == null || buses == null)
            {
                if (Mission.debug == true)
                {
                    if (done == null) UI.Notify("Null in Init.onTick - done");
                    else if (terminal == null) UI.Notify("Null in Init.onTick - terminal");
                    else if (buses == null) UI.Notify("Null in Init.onTick - buses");
                    else if (Mission.currentBus == null) UI.Notify("Null in Init.onTick - Mission.currentBus");
                    else UI.Notify("Null in Init.onTick - OTHER");
                }
                return;
            }
            // make sure the objects have been placed
            else if (done.Get() == false)
            {
                SpawnStationObjects();
            }
            else
            {
                World.DrawMarker(MarkerType.UpsideDownCone, new Vector3(434.8162f, -651.6543f, 31f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f), Color.Yellow);
            }
            // show UI message
            Ped playerPed = Game.Player.Character;
            if (playerPed.IsInRangeOf(terminal.Get(), 3) && Mission.currentBus.Get() == null && !playerPed.IsInVehicle())
            {
                if (!isMessageShown)
                {
                    UI.ShowHelpMessage("Press E to start a bus route");
                    isMessageShown = true;
                }
            }
            else
            {
                isMessageShown = false;
            }
        }
        private void SpawnStationObjects()
        {
            try
            {
                Prop StartBooth = World.CreateProp(new Model(-544726684), new Vector3(434.8162f, -651.6543f, 27.73288f), new Vector3(0f, 0f, -95f), false, false);
                StartBooth.FreezePosition = true;
                StartBooth.IsInvincible = true;
                // Remove any vehicles that intersect the bus spawn locations
                RemoveVehiclesInZone(new Vector3(425.4479f, -586.4362f, 28.50595f), 3f);
                RemoveVehiclesInZone(new Vector3(446.6568f, -591.438f, 28.49563f), 3f);
                RemoveVehiclesInZone(new Vector3(418.87f, -610.8074f, 28.64928f), 3f);
                RemoveVehiclesInZone(new Vector3(461.326f, -610.5151f, 29.33031f), 3f);
                // Spawn the buses
                List <Vehicle> busesList = new List<Vehicle>();
                busesList.Add(World.CreateVehicle(new Model(-713569950), new Vector3(425.4479f, -586.4362f, 28.50595f), -37.54238f));
                busesList.Last().PrimaryColor = VehicleColor.MetallicRaceYellow;
                busesList.Last().LockStatus = VehicleLockStatus.CannotBeTriedToEnter;
                busesList.Add(World.CreateVehicle(new Model(-713569950), new Vector3(446.6568f, -591.438f, 28.49563f), -98.87653f));
                busesList.Last().PrimaryColor = VehicleColor.MetallicFormulaRed;
                busesList.Last().LockStatus = VehicleLockStatus.CannotBeTriedToEnter;
                busesList.Add(World.CreateVehicle(new Model(-713569950), new Vector3(418.87f, -610.8074f, 28.64928f), -0.001061411f));
                busesList.Last().PrimaryColor = VehicleColor.MetallicGreen;
                busesList.Last().LockStatus = VehicleLockStatus.CannotBeTriedToEnter;


                // Dashound coach
                busesList.Add(World.CreateVehicle(new Model(-2072933068), new Vector3(461.326f, -610.5151f, 29.33031f), 34.9943f));
                busesList.Last().LockStatus = VehicleLockStatus.CannotBeTriedToEnter;
                buses.Set(busesList);

                done.Set(true);
                Blip blip = World.CreateBlip(new Vector3(429.4806f, -640.0822f, 27.5f));
                blip.Sprite = BlipSprite.Bus;

            }
            catch
            {
                if (Mission.debug == true) UI.Notify("Failed Spawning some objects. Will retry soon.");
                buses.Get().ForEach((bus) =>
                {
                    bus.Delete();
                });
                buses.Set(new List<Vehicle>());
                done.Set(false);
            }
        }
        private void RemoveVehiclesInZone(Vector3 location, float range)
        {
            Vehicle[] vehiclesInTheWay = World.GetNearbyVehicles(location, range);
            if (vehiclesInTheWay != null && vehiclesInTheWay.Length > 0)
            {
                Array.ForEach(vehiclesInTheWay, (vehicle) =>
                {
                    vehicle.Delete();
                });
            }

        }
    }
}
