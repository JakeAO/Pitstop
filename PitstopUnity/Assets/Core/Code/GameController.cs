using SadPumpkin.Game.Pitstop.Core.Code.Camera;
using SadPumpkin.Game.Pitstop.Core.Code.Race;
using SadPumpkin.Game.Pitstop.Core.Code.RTS;
using SadPumpkin.Game.Pitstop.Core.Code.UI.Gameplay;
using SadPumpkin.Game.Pitstop.Core.Code.Util;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code
{
    public class GameController : MonoBehaviour
    {
        public InterimDataHolder DataHolder;
        
        [ReadOnly] public TeamData LocalTeam;
        [ReadOnly] public TeamData[] RivalTeams;
        
        [Header("SubControllers")]
        public CameraController CameraController;
        public RaceController RaceController;
        public RtsController RtsController;
        
        [Header("UI Elements")]
        public CountdownTimer CountdownTimer;
        public RaceOverUI RaceOverUI;

        private void Awake()
        {
            // Rip interim data out of the static realm
            LocalTeam = DataHolder.Peek<TeamData>(nameof(LocalTeam));
            RivalTeams = DataHolder.Peek<TeamData[]>(nameof(RivalTeams));

            // Subscribe to events
            RaceController.RaceStatusUpdated += OnRaceStatusUpdated;
            
            // Initialize subcontrollers
            CameraController.Initialize();
            RaceController.Initialize(CameraController, LocalTeam, RivalTeams);
            RtsController.Initialize(LocalTeam, RivalTeams, RaceController.TrackController);

            // Initalize HUD
            RaceController.RaceHUD.Initialize(LocalTeam.CarInstance, RaceController, RtsController.ControllerByTeam[LocalTeam], CameraController);

            // Show pre-race countdown
            CountdownTimer.ShowCountdown(5, 2, "GO!!!");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            RaceController.RaceStatusUpdated -= OnRaceStatusUpdated;
        }

        private void OnRaceStatusUpdated(RaceStatus newStatus)
        {
            switch (newStatus)
            {
                case RaceStatus.Preparing:
                    break;
                case RaceStatus.Active:
                    break;
                case RaceStatus.Finished:
                    RaceOverUI.Show(RaceController.WinningCar == LocalTeam.CarInstance);
                    break;
            }
        }

        private void Update()
        {
            float timeStep = Time.smoothDeltaTime;

            RaceController.UpdateRace(timeStep);
            RtsController.UpdateRts(timeStep);
            CameraController.UpdateCameras(timeStep);
        }
    }
}