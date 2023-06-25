using UnityEngine;

namespace PlanetCrafterDroneShow
{
    internal class DronePosition
    {
        public int offset;
        public Vector3 coords;
        public Color color;

        public DronePosition(int myOffset, Color myColor)
        {
            offset = myOffset;
            color = myColor;
        }
    }
}
