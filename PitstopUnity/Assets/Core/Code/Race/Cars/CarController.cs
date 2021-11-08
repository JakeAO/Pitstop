using UnityEngine;

namespace SadPumpkin.Game.Pitstop
{
    public class CarController : MonoBehaviour
    {
        public CarComponent[] CarPrefabs;

        public void UpdateCars(CarComponent[] cars, float timeStep)
        {
            foreach (CarComponent carComponent in cars)
            {
                carComponent.UpdateCar(timeStep);
            }
        }
    }
}