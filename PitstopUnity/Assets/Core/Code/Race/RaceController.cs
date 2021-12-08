using System;
using System.Collections.Generic;
using System.Linq;
using SadPumpkin.Game.Pitstop.Core.Code.Camera;
using SadPumpkin.Game.Pitstop.Core.Code.Race.Cars;
using SadPumpkin.Game.Pitstop.Core.Code.Race.Track;
using SadPumpkin.Game.Pitstop.Core.Code.UI.Gameplay;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
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
        public int RaceLaps = 3;
        [ReadOnly] public RaceStatus Status = RaceStatus.Preparing;

        public TrackComponent TrackController;
        public CarController CarController;

        public RaceHUD RaceHUD;

        public Action<RaceStatus> RaceStatusUpdated;

        [ReadOnly] public RacePathingHelper PathingHelper;

        public CarComponent WinningCar => _raceResults.Count == 0 ? null : _raceResults[0];
        public IReadOnlyList<CarComponent> RaceResults => _raceResults;

        private CarPositionComparer _racePositionComparer;
        private readonly List<CarComponent> _racePositions = new List<CarComponent>(8);
        private readonly List<CarComponent> _raceResults = new List<CarComponent>(8);
        
        public void Initialize(CameraController cameraController, TeamData localTeam, TeamData[] rivalTeams)
        {
            PathingHelper = new RacePathingHelper();
            PathingHelper.InitializeRacePaths();

            CarController.Init(PathingHelper);
            CarController.SpawnCarInstances(
                TrackController.PolePositions,
                TrackController.PitPositions,
                rivalTeams.PrependWith(localTeam).ToArray(),
                RaceLaps);
            cameraController.ChaseCamera.Init(localTeam.CarInstance);
            cameraController.SwitchCameraMode(CameraController.CameraMode.Gameplay);

            _racePositionComparer = new CarPositionComparer(PathingHelper);
            _racePositions.AddRange(CarController.CarInstances);
            
            RaceStatusUpdated?.Invoke(Status);
        }

        public void UpdateRace(float timeStep)
        {
            switch (Status)
            {
                case RaceStatus.Preparing:
                    if (Time.timeSinceLevelLoad > 5f)
                    {
                        Status = RaceStatus.Active;
                        CarController.CarInstances.ForEach(x =>
                        {
                            x.CurrentGoal = CarGoal.Race;
                            x.CurrentStatus = CarStatus.Race;
                        });
                        RaceStatusUpdated?.Invoke(Status);
                    }

                    break;
                case RaceStatus.Active:
                case RaceStatus.Finished:
                    CarController.UpdateCars(timeStep);
                    break;
            }
            
            // Check for race results
            foreach (CarComponent carInstance in CarController.CarInstances)
            {
                if (carInstance.Lap > RaceLaps &&
                    carInstance.CurrentStatus == CarStatus.Race &&
                    !_raceResults.Contains(carInstance))
                {
                    carInstance.CurrentStatus = CarStatus.Finished;
                    _raceResults.Add(carInstance);

                    Status = RaceStatus.Finished;
                    RaceStatusUpdated?.Invoke(Status);
                }
            }

            RaceHUD.UpdateStatus();
        }

        public IReadOnlyList<CarComponent> GetRacePositions()
        {
            _racePositions.Sort(_racePositionComparer);
            return _racePositions;
        }

        private class CarPositionComparer : IComparer<CarComponent>
        {
            private readonly RacePathingHelper _racePathingHelper;

            public CarPositionComparer(RacePathingHelper racePathingHelper)
            {
                _racePathingHelper = racePathingHelper;
            }

            public int Compare(CarComponent x, CarComponent y)
            {
                int result = y.Lap.CompareTo(x.Lap);
                if (result != 0)
                {
                    return result;
                }

                float xProgress = _racePathingHelper.GetCarPositionInRace(x.transform.position);
                float yProgress = _racePathingHelper.GetCarPositionInRace(y.transform.position);

                return yProgress.CompareTo(xProgress);
            }
        }
    }
}