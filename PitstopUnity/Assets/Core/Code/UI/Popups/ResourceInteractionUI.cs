using SadPumpkin.Game.Pitstop.Core.Code;
using SadPumpkin.Game.Pitstop.Core.Code.RTS;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Props;
using SadPumpkin.Game.Pitstop.Core.Code.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SadPumpkin.Game.Pitstop
{
    public class ResourceInteractionUI : MonoBehaviour
    {
        public Image MetalIcon;
        public Image OilIcon;
        public TMP_Text ResourceCount;

        public Button DecreasePawns;
        public Button IncreasePawns;
        public TMP_Text PawnCount;

        public InterimDataHolder DataHolder;
        public RtsController RtsController;
        public ResourceNode Node;

        private TeamData _localTeam;
        private TeamPawnController _localPawnController;
        
        private GameplayCamera _gameplayCamera;

        private void OnEnable()
        {
            if (!RtsController)
            {
                RtsController = FindObjectOfType<RtsController>();
            }
            if (!_gameplayCamera)
            {
                _gameplayCamera = FindObjectOfType<GameplayCamera>();
            }

            DecreasePawns.onClick.AddListener(RemovePawn);
            IncreasePawns.onClick.AddListener(AddPawn);

            MetalIcon.UpdateActive(Node.NodeType == ResourceNodeType.Metal);
            OilIcon.UpdateActive(Node.NodeType == ResourceNodeType.Oil);

            _localTeam = DataHolder.Peek<TeamData>(nameof(GameController.LocalTeam));
            _localPawnController = RtsController.ControllerByTeam[_localTeam];
        }

        private void OnDisable()
        {
            DecreasePawns.onClick.RemoveListener(RemovePawn);
            IncreasePawns.onClick.RemoveListener(AddPawn);

            _localTeam = null;
            _localPawnController = null;
        }

        private void Update()
        {
            ResourceCount.text = Mathf.RoundToInt(Node.CurrentCapacity).ToString();

            PawnCount.text = $"{Node.CurrentPawns}/{Node.MaxPawns}";
            DecreasePawns.interactable = _localPawnController.CountAssignedPawns(Node) > 0;
            IncreasePawns.interactable = Node.CurrentPawns < Node.MaxPawns && _localPawnController.PawnsAtHome > 0;

            transform.rotation = _gameplayCamera.transform.rotation;

            Vector3 viewportPosition = _gameplayCamera.Camera.WorldToViewportPoint(transform.position);
            if (viewportPosition.x < 0f || viewportPosition.x > 1f ||
                viewportPosition.y < 0f || viewportPosition.y > 1f)
            {
                gameObject.SetActive(false);
            }
        }

        private void AddPawn()
        {
            _localPawnController.RequestPawnAdd(Node);
        }

        private void RemovePawn()
        {
            _localPawnController.RequestPawnRemove(Node);
        }
    }
}