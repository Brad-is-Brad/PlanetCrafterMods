using System.Collections.Generic;

namespace PlanetCrafterDroneShow
{
    internal abstract class DroneShowAction
    {
        public readonly int id;
        public float actionSeconds = 0f;
        public bool isAsyncEffect = false;
        public bool destroyed = false;

        protected DroneShowAction(int id)
        {
            this.id = id;
        }

        public abstract void Start(DroneParent droneParent, List<ModDrone> drones);

        public abstract void Update();

        public virtual void Destroy()
        {
            destroyed = true;
        }
    }
}
