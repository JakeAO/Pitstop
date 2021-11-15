using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using TMPro;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race
{
    public enum RaceStatus : int
    {
        Preparing = 0,
        Active = 1,
        Finished = 2,
    }

    public class RaceController : MonoBehaviour
    {
        // BAD BAD HATE THIS
        // These should be set from the main menu, we'll figure out something better later hopefully.
        public static TeamData PlayerTeam;
        public static TeamData[] OpponentTeams;

        public int RaceLaps = 3;
        [ReadOnly] public RaceStatus Status = RaceStatus.Preparing;
        [ReadOnly] public CarComponent WinningCar = null;

        public TrackComponent TrackController;
        public CarController CarController;
        public CameraController CameraController;

        public CountdownTimer CountdownTimer;
        public RaceStatusUI RaceStatusUI;
        public RaceOverUI RaceOverUI;

        [ReadOnly] public RacePathingHelper PathingHelper;

        public void Start()
        {
            PathingHelper = new RacePathingHelper();
            PathingHelper.InitializeRacePaths();

            CarController.Init(PathingHelper);
            CarController.SpawnCarInstances(
                TrackController.PolePositions,
                OpponentTeams.Prepend(PlayerTeam).ToArray(),
                RaceLaps);
            CameraController.ChaseCamera.Init(CarController);

            CountdownTimer.ShowCountdown(5, 2, "GO!!!");
        }

        public void Update()
        {
            float timeStep = Time.smoothDeltaTime;

            switch (Status)
            {
                case RaceStatus.Preparing:
                    if (Time.timeSinceLevelLoad > 5f)
                    {
                        Status = RaceStatus.Active;
                        CarController.CarInstances.ForEach(x => x.CurrentGoal = CarGoal.Race);
                    }

                    RaceStatusUI.UpdateStatus(this);

                    break;
                case RaceStatus.Active:
                    CarController.UpdateCars(timeStep);
                    WinningCar = CarController.CarInstances.FirstOrDefault(x => x.Lap >= RaceLaps);
                    if (WinningCar)
                    {
                        Status = RaceStatus.Finished;
                        CarController.CarInstances.ForEach(x => x.CurrentGoal = CarGoal.Idle);

                        CameraController.ChaseCamera.CurrentTarget = WinningCar;
                        CameraController.ChaseCamera.Offset = new Vector3(0f, 7.5f, -10f);
                        CameraController.ChaseCamera.ShowGuiControls = false;

                        RaceOverUI.Show();
                    }

                    RaceStatusUI.UpdateStatus(this);
                    
                    break;
                case RaceStatus.Finished:
                    CarController.UpdateCars(timeStep);
                    break;
            }

            CameraController.UpdateCameras(timeStep);
        }

        public IReadOnlyList<TeamData> GetRacePositions()
        {
            return OpponentTeams.Prepend(PlayerTeam)
                .OrderByDescending(x => x.CarInstance.Lap)
                .ThenByDescending(x => PathingHelper.GetCarPositionInRace(x.CarInstance.transform.position))
                .ToArray();
        }
    }
}