using SpaceCraft;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlanetCrafterDroneShow.DroneShowActions
{
    internal class DroneShowSpiral : DroneShowAsyncEffect
    {
        private DateTime startTime = DateTime.MinValue;

        private readonly List<GameObject> gameObjects = new List<GameObject>();

        private readonly GameObject spiralParent = new GameObject("Spiral Parent");
        private readonly Vector3 position;

        private readonly float spinRate;

        private readonly float riseRate;
        private readonly float riseMax;

        public static DroneShowSpiral Parse(int id, string line)
        {
            PlanetCrafterDroneShow.logger.LogInfo("Parsing Spiral...");
            string[] splitLine = line.Split(' ');

            string objectName = splitLine[1];
            
            string[] coords = splitLine[2].Split(',');
            Vector3 position = new Vector3(
                float.Parse(coords[0]),
                float.Parse(coords[1]),
                float.Parse(coords[2])
            );
            
            float radius = float.Parse(splitLine[3]);

            int numPoints = int.Parse(splitLine[4]);
            int numObjects = int.Parse(splitLine[5]);
            float distY = float.Parse(splitLine[6]);

            float spinRate = float.Parse(splitLine[7]);
            float riseRate = float.Parse(splitLine[8]);
            float riseMax = float.Parse(splitLine[9]);

            float delayStartSeconds = float.Parse(splitLine[10]);
            float actionSeconds = float.Parse(splitLine[11]);

            return new DroneShowSpiral(
                id,
                objectName,
                position,
                radius,
                numPoints,
                numObjects,
                distY,
                spinRate,
                riseRate,
                riseMax,
                delayStartSeconds,
                actionSeconds
            );
        }

        private DroneShowSpiral(int id, string objectName, Vector3 position, float radius, int numPoints, int numObjects, float distY, float spinRate, float riseRate, float riseMax, float delayStartSeconds, float actionSeconds) : base(id, delayStartSeconds)
        {
            this.position = position;
            this.spinRate = spinRate;
            this.riseRate = riseRate;
            this.riseMax = riseMax;
            this.actionSeconds = actionSeconds;

            for (int i = 0; i < numObjects; i++)
            {
                var group = GroupsHandler.GetAllGroups().FirstOrDefault(g => g.GetId() == objectName);
                GameObject gameObject = WorldObjectsHandler.CreateAndInstantiateWorldObject(group, Vector3.zero, Quaternion.identity);

                gameObject.transform.localPosition = new Vector3(
                    (float)(radius * Math.Cos(i * 2f * Math.PI / numPoints)),
                    distY * i,
                    (float)(radius * Math.Sin(i * 2f * Math.PI / numPoints))
                );

                gameObject.transform.localRotation = Quaternion.Euler(0f, (180f - (360f / numPoints * i)) % 360f, 0f);//rotations[i % numPoints];

                gameObject.transform.SetParent(spiralParent.transform);
                gameObjects.Add(gameObject);
            }
        }

        public override void Start(DroneParent droneParent, List<ModDrone> drones)
        {
            PlanetCrafterDroneShow.logger.LogInfo("DroneShowSpiral Start!");
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

            TimeSpan timeSpent = DateTime.Now - startTime;

            float curRiseAmount = Math.Min(riseRate * (float)timeSpent.TotalSeconds, riseMax);

            spiralParent.transform.position = position + new Vector3(0f, curRiseAmount, 0f) ;
            
            spiralParent.transform.rotation = Quaternion.Euler(new Vector3(0f, spinRate * (float)timeSpent.TotalSeconds, 0f));
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
