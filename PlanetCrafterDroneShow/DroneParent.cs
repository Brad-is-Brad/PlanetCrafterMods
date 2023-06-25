using System;
using UnityEngine;

namespace PlanetCrafterDroneShow
{
    internal class DroneParent
    {
        public float rotationDegrees = 0;
        enum RotationAxis
        {
            X,
            Y,
            Z
        }
        private RotationAxis rotationAxis;

        private DateTime curStartTime;
        private float secondsToPosition;

        private Vector3 previousPlayerPosition = Vector3.zero;

        // This is the top-most object
        private readonly GameObject facePlayerGameObject = new GameObject("Drone Parent - Face Player Game Object");

        private readonly GameObject rotateShowGameObject;

        private static readonly float playerEyeHeight = 1.4f;
        private static readonly Vector3 playerEyePositionOffset = new Vector3(0, playerEyeHeight, 0);

        public DroneParent(GameObject gameObject)
        {
            facePlayerGameObject.transform.position = DroneShow.showCenter;

            rotateShowGameObject = gameObject;
            rotateShowGameObject.transform.position = DroneShow.showCenter;
            rotateShowGameObject.transform.SetParent(facePlayerGameObject.transform, true);
        }

        public Vector3 Position
        {
            get { return rotateShowGameObject.transform.position; }
        }

        public void SetRotation(string rotationAxis, float rotationDegrees, float rotationSeconds)
        {
            if (rotationAxis == "X") { this.rotationAxis = RotationAxis.X; }
            else if (rotationAxis == "Y") { this.rotationAxis = RotationAxis.Y; }
            else if (rotationAxis == "Z") { this.rotationAxis = RotationAxis.Z; }
            curStartTime = DateTime.Now;
            this.rotationDegrees = rotationDegrees;
            secondsToPosition = rotationSeconds;
        }

        public void Update()
        {
            if (facePlayerGameObject == null)
                return;

            if (previousPlayerPosition != DroneShow.playerPosition)
            {
                // Only update if we need to, to avoid jiggling
                facePlayerGameObject.transform.position = DroneShow.showCenter;
                facePlayerGameObject.transform.rotation = Quaternion.LookRotation(facePlayerGameObject.transform.position - (DroneShow.playerPosition + playerEyePositionOffset));
                previousPlayerPosition = DroneShow.playerPosition;
            }

            TimeSpan timeLeft = curStartTime.AddSeconds(secondsToPosition) - DateTime.Now;

            if (timeLeft.TotalSeconds > 0 && rotationDegrees != 0)
            {
                float curDegrees = rotationDegrees - (rotationDegrees * (float)timeLeft.TotalSeconds / secondsToPosition);
                Quaternion rotationActionQuat = Quaternion.identity;

                if (rotationAxis == RotationAxis.X) { rotationActionQuat = Quaternion.Euler(curDegrees, 0, 0); }
                if (rotationAxis == RotationAxis.Y) { rotationActionQuat = Quaternion.Euler(0, curDegrees, 0); }
                if (rotationAxis == RotationAxis.Z) { rotationActionQuat = Quaternion.Euler(0, 0, curDegrees); }

                rotateShowGameObject.transform.localRotation = rotationActionQuat;
            } else
            {
                // Reset rotation
                rotateShowGameObject.transform.localRotation = Quaternion.identity;
            }
        }
    }
}
