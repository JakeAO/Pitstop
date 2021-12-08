using System.Collections.Generic;
using SadPumpkin.Game.Pitstop.Core.Code.Race.Track;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Pit;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race.Cars
{
    public class CarController : MonoBehaviour
    {
        [ReadOnly] public CarComponent[] CarInstances;

        private RacePathingHelper _pathingHelper;

        public void Init(RacePathingHelper pathingHelper)
        {
            _pathingHelper = pathingHelper;
        }

        public void SpawnCarInstances(
            IReadOnlyList<Transform> polePositions,
            IReadOnlyList<PitCarLocation> pitPositions,
            IReadOnlyList<TeamData> teamDatas,
            int raceLaps)
        {
            CarInstances = new CarComponent[teamDatas.Count];

            Transform carRoot = transform;
            for (int i = 0; i < teamDatas.Count; i++)
            {
                TeamData teamData = teamDatas[i];
                Transform polePosition = polePositions[i];
                PitCarLocation pitPosition = pitPositions[i];

                CarInstances[i] = Instantiate(teamData.CarModel.Prefab);
                CarInstances[i].transform.SetParent(carRoot);
                CarInstances[i].transform.position = polePosition.position;
                CarInstances[i].transform.rotation = polePosition.rotation;
                CarInstances[i].transform.localScale = polePosition.localScale;
                CarInstances[i].Init(
                    teamData.DriverStatus,
                    teamData.DriverVision,
                    teamData.CarStatus,
                    teamData.CarControl,
                    pitPosition,
                    raceLaps);
                teamData.TeamColor.ApplyToCar(CarInstances[i]);
                teamData.CarInstance = CarInstances[i];
            }
        }

        public void UpdateCars(float timeStep)
        {
            foreach (CarComponent carComponent in CarInstances)
            {
                carComponent.UpdateCar(
                    timeStep,
                    _pathingHelper.GetCarForwardPoint(carComponent.transform.position, carComponent.CurrentGoal));
            }
        }
    }
}