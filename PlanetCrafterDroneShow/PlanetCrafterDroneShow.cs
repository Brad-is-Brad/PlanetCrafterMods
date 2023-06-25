using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SpaceCraft;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlanetCrafterDroneShow
{
    [BepInPlugin("bradisbrad.PlanetCrafterMods.PlanetCrafterDroneShow", "Planet Crafter Drone Show", "0.0.1")]
    public class PlanetCrafterDroneShow : BaseUnityPlugin
    {
        public static ManualLogSource logger;
        static string droneShowPath;
        static DroneShow droneShow;

        private static GameObject mainCamera;

        private void Awake()
        {
            logger = Logger;
            Logger.LogInfo("PlanetCrafterMods Awake");
            DontDestroyOnLoad(this);

            droneShowPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "droneshow.txt");

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

        [HarmonyPatch(typeof(PlayerInputDispatcher), "Update")]
        static class PlayerInputDispatcher_Update_Patch
        {
            static void Postfix()
            {
                if (Keyboard.current[Key.J].wasPressedThisFrame)
                {
                    logger.LogInfo("Drone show key was pressed!");
                    try
                    {
                        if(droneShow != null)
                        {
                            droneShow.Destroy();
                        }

                        droneShow = new DroneShow(droneShowPath);
                        droneShow.Start();
                    }
                    catch (Exception ex)
                    {
                        logger.LogInfo("Exception starting drone show: " + ex);
                    }
                }

                if (droneShow != null)
                {
                    try
                    {
                        droneShow.Update();
                    }
                    catch (Exception ex)
                    {
                        logger.LogInfo("Exception on droneShow.Update: " + ex);
                    }
                }
            }
        }

        public static GameObject GetCamera()
        {
            if (mainCamera != null)
            {
                return mainCamera;
            }

            mainCamera = GameObject.FindWithTag("MainCamera");
            return mainCamera;
        }

        public static WorldObject SpawnObject(string objectId)
        {
            GameObject camera = GetCamera();
            Vector3 position = camera.transform.position + (camera.transform.forward * 3);
            var group = GroupsHandler.GetAllGroups().FirstOrDefault(g => g.GetId() == objectId);
            return WorldObjectsHandler.CreateAndDropOnFloor(group, position);
        }
    }
}
