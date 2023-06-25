using System.Collections.Generic;

namespace PlanetCrafterDroneShow
{
    internal class DroneShowRotation : DroneShowAction
    {
        public string rotationAxis;
        public float rotationDegrees;

        public static DroneShowRotation Parse(int id, string line)
        {
            PlanetCrafterDroneShow.logger.LogInfo("Parsing Rotate...");
            string[] splitLine = line.Split(' ');

            string axis = splitLine[1];
            float degrees = float.Parse(splitLine[2]);
            float seconds = float.Parse(splitLine[3]);

            return new DroneShowRotation(id, axis, degrees, seconds);
        }

        private DroneShowRotation(int id, string rotationAxis, float rotationDegrees, float actionSeconds) : base(id)
        {
            this.rotationAxis = rotationAxis;
            this.rotationDegrees = rotationDegrees;
            this.actionSeconds = actionSeconds;
        }

        public override void Start(DroneParent droneParent, List<ModDrone> drones)
        {
            PlanetCrafterDroneShow.logger.LogInfo("Running DroneShowRotation...");
            droneParent.SetRotation(
                rotationAxis,
                rotationDegrees,
                actionSeconds
            );
        }

        public override void Update() { }
    }
}
