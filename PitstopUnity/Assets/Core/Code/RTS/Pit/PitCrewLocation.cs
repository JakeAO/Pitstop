using SadPumpkin.Game.Pitstop.Core.Code.Util;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SadPumpkin.Game.Pitstop.Core.Code.RTS
{
    [RequireComponent(typeof(Collider))]
    public class PitCrewLocation : MonoBehaviour, IPointerClickHandler
    {
        public PitstopInteractionUI InteractionUI;
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
            
            InteractionUI.Initialize(_teamController.Team);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_teamController.Player)
            {
                InteractionUI.UpdateActive(!InteractionUI.gameObject.activeSelf);
            }
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