using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using SadPumpkin.Game.Pitstop.Core.Code;
using SadPumpkin.Game.Pitstop.Core.Code.Race;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace SadPumpkin.Game.Pitstop
{
    public class MainMenuUI : MonoBehaviour
    {
        public InterimDataHolder DataHolder;
        
        public TMP_Text TitleLabel;
        public CanvasGroup MainPanelGroup;
        public CanvasGroup SetupPanelGroup;
        public Transform PawnPreviewRoot;
        public float PawnPreviewSpacing;
        public Transform CarPreviewRoot;
        public float CarPreviewSpacing;
        
        [AssetList] public TeamColorData[] TeamColor;
        [AssetList] public CarModelData[] CarModel;
        [AssetList] public CarStatusData[] CarStatus;
        [AssetList] public CarControlData[] CarControl;
        [AssetList] public DriverStatusData[] DriverStatus;
        [AssetList] public DriverVisionData[] DriverVision;
        [AssetList] public PawnModelData[] PawnModel;
        [AssetList] public PawnControlData[] PawnControl;

        private readonly List<PawnComponent> _pawnInstances = new List<PawnComponent>(10);
        private readonly List<CarComponent> _carInstances = new List<CarComponent>(10);
        
        private int _currentTeamIndex = 0;
        private int _currentCarIndex = 0;

        private void Start()
        {
            TitleLabel.transform
                .DORotate(Vector3.forward * 20f, 3f)
                .ChangeStartValue(Vector3.back * 20f)
                .SetLoops(-1, LoopType.Yoyo);

            PawnPreviewRoot.DOLocalMoveX(0f, 1f);
            CarPreviewRoot.DOLocalMoveX(0f, 1f);

            for (int i = 0; i < PawnModel.Length; i++)
            {
                PawnModelData modelData = PawnModel[i];
                PawnComponent instance = Instantiate(modelData.Prefab, PawnPreviewRoot);
                instance.transform.localPosition = Vector3.right * PawnPreviewSpacing * i;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.transform
                    .DOLocalRotate(Vector3.up * 360f, 5f, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Incremental);
                _pawnInstances.Add(instance);
            }

            for (int i = 0; i < CarModel.Length; i++)
            {
                CarModelData modelData = CarModel[i];
                CarComponent instance = Instantiate(modelData.Prefab, CarPreviewRoot);
                instance.transform.localPosition = Vector3.right * CarPreviewSpacing * i;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one;
                instance.transform
                    .DOLocalRotate(Vector3.up * 360f, 5f, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Incremental);
                _carInstances.Add(instance);
            }

            if (DataHolder.Peek<TeamData>(nameof(GameController.LocalTeam)) is TeamData localPlayerData)
            {
                _currentTeamIndex = TeamColor.ToList().IndexOf(localPlayerData.TeamColor);
                _currentCarIndex = CarModel.ToList().IndexOf(localPlayerData.CarModel);
            }

            UpdateWithTeamColor();
            UpdateWithCarSelection();
        }

        private void UpdateWithTeamColor()
        {
            TeamColorData teamColorData = TeamColor[_currentTeamIndex];

            foreach (PawnComponent pawnInstance in _pawnInstances)
            {
                teamColorData.ApplyToPawn(pawnInstance);
            }

            foreach (CarComponent carInstance in _carInstances)
            {
                teamColorData.ApplyToCar(carInstance);
            }
        }

        private void UpdateWithCarSelection()
        {
            CarPreviewRoot.DOLocalMoveX(-CarPreviewSpacing * _currentCarIndex, 0.5f);
        }

        public void MainPanelPlayButtonPressed()
        {
            MainPanelGroup.DOFade(0f, 0.5f);
            SetupPanelGroup.gameObject.SetActive(true);
            SetupPanelGroup.DOFade(1f, 0.5f);
        }

        public void PrevTeamPressed()
        {
            _currentTeamIndex -= 1;
            if (_currentTeamIndex < 0)
                _currentTeamIndex += TeamColor.Length;
            UpdateWithTeamColor();
        }

        public void NextTeamPressed()
        {
            _currentTeamIndex += 1;
            _currentTeamIndex %= TeamColor.Length;
            UpdateWithTeamColor();
        }

        public void PrevCarPressed()
        {
            _currentCarIndex -= 1;
            if (_currentCarIndex < 0)
                _currentCarIndex += CarModel.Length;
            UpdateWithCarSelection();
        }

        public void NextCarPressed()
        {
            _currentCarIndex += 1;
            _currentCarIndex %= CarModel.Length;
            UpdateWithCarSelection();
        }

        public void SetupPanelPlayButtonPressed()
        {
            TeamData localTeam = TeamDataFromIndices(_currentTeamIndex, _currentCarIndex);
            TeamData[] rivalTeams = new TeamData[5];

            HashSet<int> usedColors = new HashSet<int> { _currentTeamIndex };

            Random random = new Random();
            for (int i = 0; i < rivalTeams.Length; i++)
            {
                int randomTeamIndex;
                do
                {
                    randomTeamIndex = random.Next(TeamColor.Length);
                } while (!usedColors.Add(randomTeamIndex));

                int randomCarIndex = random.Next(100);

                rivalTeams[i] = TeamDataFromIndices(randomTeamIndex, randomCarIndex);
            }

            DataHolder.Push(localTeam, nameof(GameController.LocalTeam));
            DataHolder.Push(rivalTeams, nameof(GameController.RivalTeams));

            SceneManager.LoadScene("Track1");
        }

        private TeamData TeamDataFromIndices(int teamIndex, int carIndex)
        {
            return new TeamData()
            {
                TeamColor = TeamColor[teamIndex % TeamColor.Length],
                PawnModel = PawnModel[teamIndex % PawnModel.Length],
                PawnControl = PawnControl[teamIndex % PawnControl.Length],
                DriverStatus = DriverStatus[carIndex % DriverStatus.Length],
                DriverVision = DriverVision[carIndex % DriverVision.Length],
                CarModel = CarModel[carIndex % CarModel.Length],
                CarStatus = CarStatus[carIndex % CarStatus.Length],
                CarControl = CarControl[carIndex % CarControl.Length],
            };
        }
    }
}