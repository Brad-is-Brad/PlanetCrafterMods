using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetCrafterDroneShow
{
    internal class DroneShowFirework : DroneShowAsyncEffect
    {
        Vector3 fireworkCenter;
        private readonly float fireworkRadius;
        private DateTime startTime = DateTime.MinValue;

        private readonly int numGameObjects = 100;
        private readonly List<GameObject> gameObjects = new List<GameObject>();
        private readonly List<Vector3> positions = new List<Vector3>();

        public static DroneShowFirework Parse(int id, string line)
        {
            PlanetCrafterDroneShow.logger.LogInfo("Parsing Firework...");
            string[] splitLine = line.Split(' ');
            string objectName = splitLine[1];
            string[] coords = splitLine[2].Split(',');
            Vector3 position = new Vector3(
                float.Parse(coords[0]),
                float.Parse(coords[1]),
                float.Parse(coords[2])
            );
            float radius = float.Parse(splitLine[3]);
            float delayStartSeconds = float.Parse(splitLine[4]);
            float actionSeconds = float.Parse(splitLine[5]);

            return new DroneShowFirework(id, objectName, position, radius, delayStartSeconds, actionSeconds);
        }

        private DroneShowFirework(int id, string fireworkObjectString, Vector3 fireworkCenter, float fireworkRadius, float delayStartSeconds, float actionSeconds) : base(id, delayStartSeconds)
        {
            this.fireworkCenter = fireworkCenter;
            this.actionSeconds = actionSeconds;
            this.fireworkRadius = fireworkRadius;

            for (int i = 0; i < numGameObjects; i++)
            {
                GameObject gameObject = PlanetCrafterDroneShow.SpawnObject(fireworkObjectString).GetGameObject();
                gameObject.transform.position = Vector3.zero;
                gameObjects.Add(gameObject);

                float inc = Mathf.PI * (3 - Mathf.Sqrt(5));
                float off = 2.0f / numGameObjects;
                float x, y, z, r, phi;

                for (var k = 0; k < numGameObjects; k++)
                {
                    y = k * off - 1 + (off / 2);
                    r = Mathf.Sqrt(1 - y * y);
                    phi = k * inc;
                    x = Mathf.Cos(phi) * r;
                    z = Mathf.Sin(phi) * r;

                    positions.Add(new Vector3(x, y, z));
                }
            }
        }

        public override void Start(DroneParent droneParent, List<ModDrone> drones)
        {
        }

        public override void Update()
        {
            if (startTime == DateTime.MinValue)
            {
                startTime = DateTime.Now;
            }

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

            for (int i = 0; i < gameObjects.Count; i++)
            {
                if (gameObjects[i] == null)
                    return;

                float portionTimeSpent = 1 - (float)(timeLeft.TotalSeconds / actionSeconds);

                Vector3 newPosition =
                    fireworkCenter +
                    (
                        positions[i] *
                        fireworkRadius *
                        (float)Math.Sin(portionTimeSpent * (Math.PI / 2)) // sine wave
                    )
                ;

                try
                {
                    gameObjects[i].transform.position = newPosition;

                    Vector3 playerPosition = PlanetCrafterDroneShow.GetCamera().transform.position;
                    gameObjects[i].transform.LookAt(
                        playerPosition + new Vector3(0, 10000f, 0)
                    );
                }
                catch (Exception ex)
                {
                    PlanetCrafterDroneShow.logger.LogInfo("Moving firework error: " + ex);
                }
            }
        }

        public override void Destroy()
        {
            base.Destroy();

            if (gameObjects == null)
                return;

            for (int i = 0; i < gameObjects.Count; i++)
            {
                if (gameObjects[i] != null)
                {
                    gameObjects[i].SetActive(false);
                    UnityEngine.Object.Destroy(gameObjects[i]);
                }
            }

            gameObjects.Clear();
        }
    }
}
