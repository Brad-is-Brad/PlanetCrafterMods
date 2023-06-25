
namespace PlanetCrafterDroneShow
{
    internal abstract class DroneShowAsyncEffect : DroneShowAction
    {
        public readonly float delayStartSeconds;

        protected DroneShowAsyncEffect(int id, float delayStartSeconds) : base(id)
        {
            isAsyncEffect = true;
            this.delayStartSeconds = delayStartSeconds;
        }
    }
}
