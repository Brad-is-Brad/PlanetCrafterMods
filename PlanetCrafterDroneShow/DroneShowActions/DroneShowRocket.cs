using SpaceCraft;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetCrafterDroneShow
{
    internal class DroneShowRocket : DroneShowAsyncEffect
    {
        MachineRocket rocket;
        Vector3 position;
        Quaternion rotation;
        float speed;
        float acceleration;

        DateTime startTime;

        public static DroneShowRocket Parse(int id, string line)
        {
            PlanetCrafterDroneShow.logger.LogInfo("Parsing Rocket...");

            string[] splitLine = line.Split(' ');

            string rocketType = splitLine[1];

            string[] coords = splitLine[2].Split(',');
            Vector3 position = new Vector3(
                float.Parse(coords[0]),
                float.Parse(coords[1]),
                float.Parse(coords[2])
            );

            string[] degrees = splitLine[3].Split(',');
            Quaternion rotation = Quaternion.Euler(
                float.Parse(degrees[0]),
                float.Parse(degrees[1]),
                float.Parse(degrees[2])
            );

            float delayStartSeconds = float.Parse(splitLine[4]);

            float speed = float.Parse(splitLine[5]);
            float acceleration = float.Parse(splitLine[6]);

            return new DroneShowRocket(id, rocketType, position, rotation, delayStartSeconds, speed, acceleration);
        }

        private DroneShowRocket(int id, string rocketType, Vector3 position, Quaternion rotation, float delayStartSeconds, float speed, float acceleration) : base(id, delayStartSeconds)
        {
            this.position = position;
            this.rotation = rotation;
            this.speed = speed;
            this.acceleration = acceleration;

            WorldObject worldObject = PlanetCrafterDroneShow.SpawnObject(rocketType);
            rocket = worldObject.GetGameObject().GetComponent<MachineRocket>();
            rocket.transform.position = Vector3.zero;
        }

        public override void Start(DroneParent droneParent, List<ModDrone> drones)
        {
            if (rocket == null)
                return;

            rocket.Ignite();
            startTime = DateTime.Now;
        }

        public override void Update()
        {
            if (rocket != null)
            {
                TimeSpan timeSinceStart = DateTime.Now - startTime;
                rocket.transform.rotation = rotation;

                float totalSeconds = (float)timeSinceStart.TotalSeconds;
                rocket.transform.position = position + rocket.transform.up * ((speed * totalSeconds) + (0.5f * acceleration * totalSeconds * totalSeconds));
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            if (rocket != null && rocket.gameObject != null)
            {
                rocket.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(rocket);
            }
        }
    }
}
