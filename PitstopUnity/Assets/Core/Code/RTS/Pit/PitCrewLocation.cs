using SadPumpkin.Game.Pitstop.Core.Code.Race.Cars;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Pawns;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.RTS.Pit
{
    [RequireComponent(typeof(Collider))]
    public class PitCrewLocation : MonoBehaviour
    {
        public Transform FlagObject;
        public Renderer FlagRenderer;
        public Vector3 PitStopFlagTarget;
        public Vector3 IdleFlagTarget;
        public float FlagMoveSmooth;
        
        private TeamPawnController _teamController;
        private Vector3 _flagVelocity;
        
        public void Initialize(TeamPawnController teamController)
        {
            _teamController = teamController;

            _teamController.Team.TeamColor.ApplyToFlag(FlagRenderer);
        }

        private void Update()
        {
            if (_teamController?.Team?.CarInstance != null)
            {
                FlagObject.localPosition = Vector3.SmoothDamp(
                    FlagObject.localPosition,
                    _teamController.Team.CarInstance.CurrentGoal == CarGoal.GoToPit
                        ? PitStopFlagTarget
                        : IdleFlagTarget,
                    ref _flagVelocity,
                    FlagMoveSmooth);
            }
        }
    }
}