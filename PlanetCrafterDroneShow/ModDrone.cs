using SpaceCraft;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlanetCrafterDroneShow
{
    internal class ModDrone
    {
        readonly Drone drone;
        private Vector3 curStartPosition;
        private Vector3 curTargetPosition;

        private DateTime curStartTime;
        private float secondsToPosition;

        private readonly int numLights = 1;
        private readonly List<GameObject> lights = new List<GameObject>();

        public ModDrone(GameObject drone)
        {
            this.drone = drone.GetComponent<Drone>();
            curStartPosition = new Vector3(0f, 0f, 0f);
            curTargetPosition = new Vector3(0f, 0f, 0f);
            this.drone.transform.localPosition = curStartPosition;

            for (int i = 0; i < numLights; i++)
            {
                GameObject lamp = new GameObject("DroneLight");
                Light lightComp = lamp.AddComponent<Light>();
                lightComp.type = LightType.Spot;
                lightComp.color = Color.black;
                lightComp.intensity = 8f;
                lightComp.enabled = true;
                lightComp.spotAngle = 65f;
                lightComp.range = 1.5f;

                lights.Add(lamp);
            }
        }

        public void SetTarget(Vector3 newTargetPosition, float newSecondsToPosition, Color color)
        {
            curStartPosition = curTargetPosition;
            curTargetPosition = newTargetPosition;
            secondsToPosition = newSecondsToPosition;
            curStartTime = DateTime.Now;

            foreach(GameObject light in lights)
            {
                if (light == null)
                    return;

                light.GetComponent<Light>().color = color;
            }
        }

        public void EndShow(float secondsToEndShow)
        {
            SetTarget(
                new Vector3(0f, 0f, 0f),
                secondsToEndShow,
                Color.black
            );
        }

        public void Update()
        {
            if (drone == null)
                return;

            TimeSpan timeLeft = curStartTime.AddSeconds(secondsToPosition) - DateTime.Now;

            if (timeLeft.TotalSeconds <= 0)
            {
                drone.transform.localPosition = curTargetPosition;
            }
            else
            {
                Vector3 startTargetDifference = curTargetPosition - curStartPosition;

                float portionTimeLeft = (float)(timeLeft.TotalSeconds / secondsToPosition);

                drone.transform.localPosition =
                    curTargetPosition - (
                        (startTargetDifference) * (portionTimeLeft)
                    )
                ;
            }

            drone.transform.LookAt(
                PlanetCrafterDroneShow.GetCamera().transform.position,
                Vector3.up
            );

            foreach (GameObject light in lights)
            {
                light.transform.position = drone.transform.position + drone.transform.forward;
                light.transform.LookAt(drone.transform);
            }
        }

        public void Destroy()
        {
            if (drone != null)
            {
                drone.gameObject.SetActive(false);
                UnityEngine.Object.Destroy(drone);
            }

            foreach (GameObject light in lights)
            {
                if (light != null)
                {
                    light.SetActive(false);
                    UnityEngine.Object.Destroy(light);
                }
            }
        }
    }
}
