using System.Collections.Generic;
using System.Linq;
using SadPumpkin.Game.Pitstop.Core.Code.Race;
using SadPumpkin.Game.Pitstop.Core.Code.Race.Cars;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Pit;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Props;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.RTS.Pawns
{
    public class TeamPawnController_AI : TeamPawnController
    {
        public override bool Player => false;

        private PawnAIData _pawnAI;

        private float _bodyThreshold;
        private float _fuelThreshold;
        private float _staminaThreshold;

        private uint _targetPawnsGatheringMetal;
        private uint _targetPawnsGatheringOil;

        private float _aiEvaluationTimer;

        private IReadOnlyList<ResourceNode> _allMetalNodes;
        private IReadOnlyList<ResourceNode> _allOilNodes;

        public TeamPawnController_AI(TeamData team, PitCrewLocation pitLocation) : base(team, pitLocation)
        {
            _pawnAI = team.PawnAI;
            _aiEvaluationTimer += _pawnAI.AiEvaluationFrequency.Evaluate(Random.Range(_pawnAI.AiEvaluationFrequency[0].time, _pawnAI.AiEvaluationFrequency[_pawnAI.AiEvaluationFrequency.length - 1].time));
        }

        public override void SpawnPawns(float spawnRadius, uint pawnCount)
        {
            base.SpawnPawns(spawnRadius, pawnCount);

            _bodyThreshold = Team.CarInstance.MaxBody * _pawnAI.CarRepairThreshold.Evaluate(Random.Range(_pawnAI.CarRepairThreshold[0].time, _pawnAI.CarRepairThreshold[_pawnAI.CarRepairThreshold.length - 1].time));
            _fuelThreshold = Team.CarInstance.MaxFuel * _pawnAI.CarRefuelThreshold.Evaluate(Random.Range(_pawnAI.CarRefuelThreshold[0].time, _pawnAI.CarRefuelThreshold[_pawnAI.CarRefuelThreshold.length - 1].time));
            _staminaThreshold = Team.CarInstance.MaxStamina * _pawnAI.DriverStaminaThreshold.Evaluate(Random.Range(_pawnAI.DriverStaminaThreshold[0].time, _pawnAI.DriverStaminaThreshold[_pawnAI.DriverStaminaThreshold.length - 1].time));

            float pitWeight = _pawnAI.PitCrewWeight.Evaluate(Random.Range(_pawnAI.PitCrewWeight[0].time, _pawnAI.PitCrewWeight[_pawnAI.PitCrewWeight.length - 1].time));
            float metalWeight = _pawnAI.GatherMetalWeight.Evaluate(Random.Range(_pawnAI.GatherMetalWeight[0].time, _pawnAI.GatherMetalWeight[_pawnAI.GatherMetalWeight.length - 1].time));
            float oilWeight = _pawnAI.GatherOilWeight.Evaluate(Random.Range(_pawnAI.GatherOilWeight[0].time, _pawnAI.GatherOilWeight[_pawnAI.GatherOilWeight.length - 1].time));
            float totalWeight = pitWeight + metalWeight + oilWeight;

            _targetPawnsGatheringMetal = (uint)Mathf.RoundToInt(pawnCount * metalWeight / totalWeight);
            _targetPawnsGatheringOil = (uint)Mathf.RoundToInt(pawnCount * oilWeight / totalWeight);

            var allNodes = Object.FindObjectsOfType<ResourceNode>();
            _allMetalNodes = allNodes.Where(x => x.NodeType == ResourceNodeType.Metal).ToArray();
            _allOilNodes = allNodes.Where(x => x.NodeType == ResourceNodeType.Oil).ToArray();
        }

        public override void UpdatePawns(float timeStep)
        {
            base.UpdatePawns(timeStep);

            _aiEvaluationTimer -= timeStep;
            if (_aiEvaluationTimer <= 0f)
            {
                // Evaluate Pawns
                List<PawnComponent> pawnsGatheringMetal = PawnsAtWork.Where(x => x.CurrentNodeTarget.NodeType == ResourceNodeType.Metal).ToList();
                if (pawnsGatheringMetal.Count < _targetPawnsGatheringMetal)
                {
                    var metalNodes = _allMetalNodes
                        .Where(x => x.CurrentPawns < x.MaxPawns && x.CurrentCapacity / Mathf.Max(x.CurrentPawns, 1) > 5f)
                        .ToArray();
                    if (metalNodes.Length > 0)
                    {
                        for (int i = pawnsGatheringMetal.Count; i < _targetPawnsGatheringMetal; i++)
                        {
                            ResourceNode targetNode = metalNodes[Random.Range(0, metalNodes.Length)];
                            RequestPawnAdd(targetNode);
                        }
                    }
                }
                else if (pawnsGatheringMetal.Count > _targetPawnsGatheringMetal)
                {
                    while (pawnsGatheringMetal.Count > _targetPawnsGatheringMetal)
                    {
                        pawnsGatheringMetal.RemoveAt(Random.Range(0, pawnsGatheringMetal.Count));
                    }
                }

                List<PawnComponent> pawnsGatheringOil = PawnsAtWork.Where(x => x.CurrentNodeTarget.NodeType == ResourceNodeType.Oil).ToList();
                if (pawnsGatheringOil.Count < _targetPawnsGatheringOil)
                {
                    var oilNodes = _allOilNodes
                        .Where(x => x.CurrentPawns < x.MaxPawns && x.CurrentCapacity / Mathf.Max(x.CurrentPawns, 1) > 5f)
                        .ToArray();
                    if (oilNodes.Length > 0)
                    {
                        for (int i = pawnsGatheringOil.Count; i < _targetPawnsGatheringOil; i++)
                        {
                            ResourceNode targetNode = oilNodes[Random.Range(0, oilNodes.Length)];
                            RequestPawnAdd(targetNode);
                        }
                    }
                }
                else if (pawnsGatheringOil.Count > _targetPawnsGatheringOil)
                {
                    while (pawnsGatheringOil.Count > _targetPawnsGatheringOil)
                    {
                        pawnsGatheringOil.RemoveAt(Random.Range(0, pawnsGatheringOil.Count));
                    }
                }

                // Evaluate Car
                if (Team.CarInstance.CurrentGoal == CarGoal.Race)
                {
                    if (Team.CarInstance.CurrentCarBody <= _bodyThreshold ||
                        Team.CarInstance.CurrentCarFuel <= _fuelThreshold ||
                        Team.CarInstance.CurrentDriverStamina <= _staminaThreshold)
                    {
                        Team.CarInstance.CurrentGoal = CarGoal.GoToPit;
                    }
                }

                // Update Evaluation Timer
                _aiEvaluationTimer += _pawnAI.AiEvaluationFrequency.Evaluate(Random.Range(_pawnAI.AiEvaluationFrequency[0].time, _pawnAI.AiEvaluationFrequency[_pawnAI.AiEvaluationFrequency.length - 1].time));
            }
        }
    }
}