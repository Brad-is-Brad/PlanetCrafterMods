using PlanetCrafterDroneShow.DroneShowActions;
using SpaceCraft;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace PlanetCrafterDroneShow
{
    internal class DroneShow
    {
        private string configPath;

        private bool isStarted = false;

        private int droneShowActionId = 0;

        private int numDronesNeeded = 0;
        private List<ModDrone> drones = new List<ModDrone>();

        private DroneParent droneParent;
        private GameObject droneParentGameObject = new GameObject("Drone Parent");

        private List<DroneShowAction> droneShowActions = new List<DroneShowAction>();
        private List<int> activeEffectIds = new List<int>();

        private DateTime showStartTime;
        private int curFormationOffset = -1;

        private bool endingShow = false;
        private DateTime endShowStartTime;
        private float secondsToEndShow = 2f;

        private Dictionary<char, Color> charColors = new Dictionary<char, Color>();

        public static Vector3 showCenter = Vector3.zero;
        public static Vector3 playerPosition = Vector3.zero;
        public PlayerMainController activePlayerController;

        public DroneShow(string configPath)
        {
            this.configPath = configPath;
        }

        private void ParseShow()
        {
            PlanetCrafterDroneShow.logger.LogInfo("DroneShow loading...");
            DroneShowFormation curFormation = null;
            showCenter = Vector3.zero;

            string[] lines = File.ReadAllLines(configPath);

            foreach (string line in lines)
            {
                if (line.StartsWith("ShowCenter"))
                {
                    droneShowActions.Add(
                        DroneShowCenter.Parse(++droneShowActionId, line)
                    );
                }
                else if (line.StartsWith("TeleportPlayer"))
                {
                    droneShowActions.Add(
                        DroneShowTeleportPlayer.Parse(++droneShowActionId, line)
                    );
                }
                else if (line.StartsWith("FormationTime"))
                {
                    PlanetCrafterDroneShow.logger.LogInfo("Parsing FormationTime...");
                    float timeToForm = float.Parse(line.Split(' ')[1]);
                    curFormation = new DroneShowFormation(++droneShowActionId, timeToForm);
                }
                else if (line.StartsWith("EndFormation"))
                {
                    PlanetCrafterDroneShow.logger.LogInfo("Parsing EndFormation...");
                    int dronesNeeded = curFormation.Prepare();
                    if (dronesNeeded > numDronesNeeded)
                    {
                        numDronesNeeded = dronesNeeded;
                    }

                    droneShowActions.Add(curFormation);
                    curFormation = null;
                }
                else if (line.StartsWith("Hold"))
                {
                    droneShowActions.Add(
                        DroneShowHold.Parse(++droneShowActionId, line)
                    );
                }
                else if (line.StartsWith("Color"))
                {
                    PlanetCrafterDroneShow.logger.LogInfo("Parsing Color...");
                    char character = line.Split(' ')[1][0];
                    float red = float.Parse(line.Split(' ')[2]);
                    float green = float.Parse(line.Split(' ')[3]);
                    float blue = float.Parse(line.Split(' ')[4]);
                    Color color = new Color(red, green, blue);

                    charColors.Add(character, color);
                }
                else if (line.StartsWith("Rotate"))
                {
                    droneShowActions.Add(
                        DroneShowRotation.Parse(++droneShowActionId, line)
                    );
                }
                else if (line.StartsWith("Firework"))
                {
                    droneShowActions.Add(
                        DroneShowFirework.Parse(++droneShowActionId, line)
                    );
                }
                else if (line.StartsWith("Rocket"))
                {
                    droneShowActions.Add(
                        DroneShowRocket.Parse(++droneShowActionId, line)
                    );
                }
                else if (line.StartsWith("Spiral"))
                {
                    droneShowActions.Add(
                        DroneShowSpiral.Parse(++droneShowActionId, line)
                    );
                }
                else if (curFormation != null)
                {
                    PlanetCrafterDroneShow.logger.LogInfo("Parsing formation line...");
                    List<DronePosition> positions = new List<DronePosition>();
                    for (var i = 0; i < line.Length; i++)
                    {
                        if (!line[i].Equals(' '))
                        {
                            Color color = charColors.ContainsKey(line[i]) ? charColors[line[i]] : Color.white;

                            positions.Add(
                                new DronePosition(i, color)
                            );
                        }
                    }

                    curFormation.AddLine(positions);
                }
            }

            // add formation to prevent collision phasing
            curFormation = new DroneShowFormation(++droneShowActionId, 1f);

            int lineCount = 0;
            List<DronePosition> collisionFixPositions = new List<DronePosition>();
            for (var i = 0; i < numDronesNeeded; i++)
            {
                collisionFixPositions.Add(
                    new DronePosition(lineCount * 2, Color.black)
                );

                lineCount++;
                if (lineCount >= 10)
                {
                    curFormation.AddLine(collisionFixPositions);
                    collisionFixPositions = new List<DronePosition>();
                    lineCount = 0;
                }
            }

            if (lineCount > 0)
            {
                curFormation.AddLine(collisionFixPositions);
            }
            
            curFormation.Prepare();
            droneShowActions.Insert(0, curFormation);

            // start from single centered position
            curFormation = new DroneShowFormation(++droneShowActionId, 0.5f);
            List<DronePosition> startPosition = new List<DronePosition>
            {
                new DronePosition(0, Color.black)
            };
            curFormation.AddLine(startPosition);
            curFormation.Prepare();
            droneShowActions.Insert(1, curFormation);

            PlanetCrafterDroneShow.logger.LogInfo("DroneShow loaded!");
        }

        public void Start()
        {
            PlanetCrafterDroneShow.logger.LogInfo("DroneShow Start...");
            if (isStarted) { return; }

            ParseShow();

            droneParent = new DroneParent(droneParentGameObject);

            PlanetCrafterDroneShow.logger.LogInfo("DroneShow starting, needs " + numDronesNeeded + " drones...");
            for (var i = 0; i < numDronesNeeded; i++)
            {
                GameObject spawnedDrone = PlanetCrafterDroneShow.SpawnObject("Drone1").GetGameObject();

                spawnedDrone.transform.SetParent(droneParentGameObject.transform, true);

                drones.Add(
                    new ModDrone(spawnedDrone)
                );
            }

            activePlayerController = Managers.GetManager<PlayersManager>().GetActivePlayerController();
            playerPosition = activePlayerController.transform.position;

            showStartTime = DateTime.Now;
            isStarted = true;
        }

        public void Update()
        {
            if (!isStarted) { return; }

            if (droneParentGameObject == null)
            {
                PlanetCrafterDroneShow.logger.LogInfo("Detected that droneParentGameObject is null!");
                Destroy();
                return;
            }

            if (endingShow && DateTime.Now >= endShowStartTime.AddSeconds(secondsToEndShow))
            {
                Destroy();
                return;
            }

            TimeSpan timeSinceShowStart = DateTime.Now - showStartTime;
            TimeSpan formationTime = TimeSpan.Zero;

            bool foundFormation = false;

            activePlayerController.transform.position = playerPosition;

            // TODO: this is causing a camera snap when the player moves the mouse again
            PlanetCrafterDroneShow.GetCamera().transform.LookAt(showCenter);

            for (int i = 0; i < droneShowActions.Count; i++)
            {
                if (droneShowActions[i].destroyed)
                    continue;

                if (droneShowActions[i].isAsyncEffect)
                {
                    if (!activeEffectIds.Contains(droneShowActions[i].id))
                    {
                        activeEffectIds.Add(droneShowActions[i].id);
                        curFormationOffset += 1;
                        droneShowActions[i].Start(droneParent, drones);
                    }

                    droneShowActions[i].Update();
                }
                else
                {
                    formationTime = formationTime.Add(
                        TimeSpan.FromSeconds(
                            droneShowActions[i].actionSeconds
                        )
                    );

                    if (timeSinceShowStart.TotalSeconds <= formationTime.TotalSeconds)
                    {
                        foundFormation = true;
                        if (i > curFormationOffset)
                        {
                            curFormationOffset += 1;
                            droneShowActions[curFormationOffset].Start(droneParent, drones);
                        }

                        droneShowActions[curFormationOffset].Update();

                        // Exit the loop because we've found our current action
                        break;
                    }
                }
            }

            if (!foundFormation && !endingShow)
            {
                PlanetCrafterDroneShow.logger.LogInfo("Ending show...");
                endingShow = true;
                endShowStartTime = DateTime.Now;
                for (int j = 0; j < drones.Count; j++)
                {
                    drones[j].EndShow(secondsToEndShow);
                }
            }
            
            droneParent.Update();

            for (int i = 0; i < drones.Count; i++)
            {
                drones[i].Update();
            }
        }

        public void Destroy()
        {
            PlanetCrafterDroneShow.logger.LogInfo("Deleting drones...");
            for (int j = 0; j < drones.Count; j++)
            {
                drones[j].Destroy();
            }
            drones.Clear();

            foreach (DroneShowAction droneShowAction in droneShowActions)
            {
                if (droneShowAction is DroneShowAsyncEffect effect)
                {
                    effect.Destroy();
                }
            }
            droneShowActions.Clear();

            activeEffectIds.Clear();

            charColors.Clear();

            isStarted = false;
        }
    }
}
