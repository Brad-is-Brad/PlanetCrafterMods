using System.Collections.Generic;

namespace PlanetCrafterDroneShow
{
    internal class DroneShowHold : DroneShowAction
    {
        public static DroneShowHold Parse(int id, string line)
        {
            PlanetCrafterDroneShow.logger.LogInfo("Parsing Hold...");
            float holdTime = float.Parse(line.Split(' ')[1]);

            return new DroneShowHold(id, holdTime);
        }

        private DroneShowHold(int id, float actionSeconds) : base(id)
        {
            this.actionSeconds = actionSeconds;
        }

        public override void Start(DroneParent droneParent, List<ModDrone> drones) { }

        public override void Update() { }
    }
}
