using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlanetCrafterDroneShow
{
    internal class DroneShowFormation : DroneShowAction
    {
        private readonly List<List<DronePosition>> lines = new List<List<DronePosition>>();
        public List<DronePosition> positions;

        private readonly float droneSpacing = 1f;

        public DroneShowFormation(int id, float actionSeconds) : base(id)
        {
            this.actionSeconds = actionSeconds;
        }

        public void AddLine(List<DronePosition> line)
        {
            lines.Add(line);
        }

        public int Prepare()
        {
            int longestLine = 0;

            foreach (var line in lines)
            {
                if (line.Count == 0)
                    continue;

                int lineMax = line.Max(l => l.offset);
                if (lineMax > longestLine)
                {
                    longestLine = lineMax;
                }
            }

            float startX = -droneSpacing * (longestLine / 2);
            float startY = droneSpacing * (lines.Count / 2);
            float startZ = 0f;

            positions = new List<DronePosition>();
            for (var i = 0; i < lines.Count; i++)
            {
                foreach (DronePosition position in lines[i])
                {
                    Vector3 pos = new Vector3(
                        startX + (droneSpacing * position.offset),
                        startY - (droneSpacing * i),
                        startZ
                    );

                    position.coords = pos;
                    positions.Add(position);
                }
            }

            return positions.Count;
        }

        public override void Start(DroneParent droneParent, List<ModDrone> drones)
        {
            PlanetCrafterDroneShow.logger.LogInfo("Running DroneShowFormation...");

            Vector3 positionCoords;
            Color positionColor;

            for (int j = 0; j < drones.Count; j++)
            {
                if (j >= positions.Count)
                {
                    // If we have too many drones, have the excess drones overlap on the last position
                    positionCoords = positions.Last().coords;
                    positionColor = Color.black;
                }
                else
                {
                    positionCoords = positions[j].coords;
                    positionColor = positions[j].color;
                }

                drones[j].SetTarget(
                    positionCoords,
                    actionSeconds,
                    positionColor
                );
            }
        }

        public override void Update() { }
    }
}
