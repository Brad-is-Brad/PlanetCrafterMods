using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetCrafterDroneShow
{
    internal class DroneShowCenter : DroneShowAsyncEffect
    {
        private Vector3 position;

        private DateTime startTime;
        private Vector3 startPosition;

        public static DroneShowCenter Parse(int id, string line)
        {
            PlanetCrafterDroneShow.logger.LogInfo("Parsing ShowCenter...");

            string[] splitLine = line.Split(' ');

            string[] coords = splitLine[1].Split(',');
            Vector3 position = new Vector3(
                float.Parse(coords[0]),
                float.Parse(coords[1]),
                float.Parse(coords[2])
            );

            float delayStartSeconds = float.Parse(splitLine[2]);

            float actionSeconds = float.Parse(splitLine[3]);

            if (DroneShow.showCenter == Vector3.zero)
            {
                DroneShow.showCenter = position;
            }

            return new DroneShowCenter(id, position, delayStartSeconds, actionSeconds);
        }

        private DroneShowCenter(int id, Vector3 position, float delayStartSeconds, float actionSeconds) : base(id, delayStartSeconds)
        {
            this.position = position;
            this.actionSeconds = actionSeconds;
        }

        public override void Start(DroneParent droneParent, List<ModDrone> drones)
        {
            PlanetCrafterDroneShow.logger.LogInfo("Starting ShowCenter... " + position);
            startTime = DateTime.Now;
            startPosition = DroneShow.showCenter;
        }

        public override void Update()
        {
            TimeSpan timeLeft = startTime.AddSeconds(delayStartSeconds).AddSeconds(actionSeconds) - DateTime.Now;

            if (timeLeft.TotalSeconds > actionSeconds)
            {
                // not ready to start
                return;
            }

            if (timeLeft.TotalSeconds < 0)
            {
                Destroy();
                return;
            }

            Vector3 startTargetDifference = position - startPosition;

            // prevent dividing by zero
            float portionTimeLeft = actionSeconds == 0 ? 0 : (float)(timeLeft.TotalSeconds / actionSeconds);

            DroneShow.showCenter =
                position - (
                    (startTargetDifference) * (portionTimeLeft)
                )
            ;

            PlanetCrafterDroneShow.logger.LogInfo("ShowCenter done!");
        }
    }
}
