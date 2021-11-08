using System;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race
{
    public class RaceController : MonoBehaviour
    {
        public TrackComponent TrackController;
        public CarController CarController;

        public CarComponent[] Cars;

        public void Start()
        {
            SetupTrack();
        }

        public void Update()
        {
            CarController.UpdateCars(Cars, Time.deltaTime);
        }

        private void SetupTrack()
        {
            Cars = new CarComponent[Math.Min(TrackController.PolePositions.Length, CarController.CarPrefabs.Length)];

            Transform carRoot = CarController.transform;
            for (int i = 0; i < TrackController.PolePositions.Length; i++)
            {
                Transform polePosition = TrackController.PolePositions[i];
                if (i < CarController.CarPrefabs.Length)
                {
                    Cars[i] = Instantiate(CarController.CarPrefabs[i]);
                    Cars[i].transform.SetParent(carRoot);
                    Cars[i].transform.position = polePosition.position;
                    Cars[i].transform.rotation = polePosition.rotation;
                    Cars[i].transform.localScale = polePosition.localScale;
                }
            }
        }
    }
}