using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetCrafterDroneShow
{
    internal class DroneShowTeleportPlayer : DroneShowAsyncEffect
    {
        public Vector3 position;

        private DateTime startTime;
        private Vector3 startPosition;

        public static DroneShowTeleportPlayer Parse(int id, string line)
        {
            PlanetCrafterDroneShow.logger.LogInfo("Parsing TeleportPlayer...");

            string[] splitLine = line.Split(' ');

            string[] coords = splitLine[1].Split(',');
            Vector3 position = new Vector3(
                float.Parse(coords[0]),
                float.Parse(coords[1]),
                float.Parse(coords[2])
            );

            float delayStartSeconds = float.Parse(splitLine[2]);

            float actionSeconds = float.Parse(splitLine[3]);

            return new DroneShowTeleportPlayer(id, position, delayStartSeconds, actionSeconds);
        }

        private DroneShowTeleportPlayer(int id, Vector3 position, float delayStartSeconds, float actionSeconds) : base(id, delayStartSeconds)
        {
            this.position = position;
            this.actionSeconds = actionSeconds;
        }

        public override void Start(DroneParent droneParent, List<ModDrone> drones)
        {
            PlanetCrafterDroneShow.logger.LogInfo("Starting TeleportPlayer... " + position);
            startTime = DateTime.Now;
            startPosition = PlanetCrafterDroneShow.GetCamera().transform.position;
        }

        public override void Update()
        {
            if (this.destroyed)
            {
                DroneShow.playerPosition = position;
                return;
            }

            TimeSpan timeLeft = startTime.AddSeconds(delayStartSeconds).AddSeconds(actionSeconds) - DateTime.Now;

            if (timeLeft.TotalSeconds > actionSeconds)
            {
                // not ready to start
                PlanetCrafterDroneShow.logger.LogInfo("TeleportPlayer not ready to start.");
                return;
            }

            Vector3 startTargetDifference = position - startPosition;

            // prevent dividing by zero
            float portionTimeLeft = actionSeconds == 0 ? 0 : (float)(timeLeft.TotalSeconds / actionSeconds);

            Vector3 currentPosition = position - (startTargetDifference * portionTimeLeft);

            DroneShow.playerPosition = currentPosition;

            PlanetCrafterDroneShow.logger.LogInfo("TeleportPlayer done!");

            if (timeLeft.TotalSeconds < 0)
            {
                Destroy();
                return;
            }
        }
    }
}
