using System.Collections.Generic;
using System.Linq;
using SadPumpkin.Game.Pitstop.Core.Code.Util;
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
        public RaceHUD _raceHUD;
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
            CameraController.ChaseCamera.Init(PlayerTeam.CarInstance);

            CountdownTimer.ShowCountdown(5, 2, "GO!!!");

            // Clear statics, we don't want anything stinky hanging around after we've used it.
            PlayerTeam = null;
            OpponentTeams = null;
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

                    break;
                case RaceStatus.Active:
                    CarController.UpdateCars(timeStep);
                    WinningCar = CarController.CarInstances.FirstOrDefault(x => x.Lap >= RaceLaps);
                    if (WinningCar)
                    {
                        Status = RaceStatus.Finished;
                        CarController.CarInstances.ForEach(x => x.CurrentGoal = CarGoal.Idle);

                        RaceOverUI.Show(WinningCar == CarController.CarInstances[0]);
                    }
                    
                    break;
                case RaceStatus.Finished:
                    CarController.UpdateCars(timeStep);
                    break;
            }

            _raceHUD.UpdateStatus(this);
            
            CameraController.UpdateCameras(timeStep);
        }

        public IReadOnlyList<CarComponent> GetRacePositions()
        {
            return CarController.CarInstances
                .OrderByDescending(x => x.Lap)
                .ThenByDescending(x => PathingHelper.GetCarPositionInRace(x.transform.position))
                .ToArray();
        }
    }
}