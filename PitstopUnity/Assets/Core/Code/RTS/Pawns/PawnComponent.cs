using System;
using SadPumpkin.Game.Pitstop.Core.Code;
using SadPumpkin.Game.Pitstop.Core.Code.RTS;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Props;
using SadPumpkin.Game.Pitstop.Core.Code.Util;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace SadPumpkin.Game.Pitstop
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PawnComponent : MonoBehaviour
    {
        public class PawnInventory
        {
            public ResourceNodeType Type = ResourceNodeType.Invalid;
            public float Amount = 0f;
        }

        [ReadOnly] public TeamPawnController TeamController;
        public NavMeshAgent NavAgent => _navMeshAgent;

        public Renderer[] Meshes;
        public Renderer MinimapPip;

        [ReadOnly] public PawnGoal CurrentGoal = PawnGoal.ReturnToPit;
        [ReadOnly] public ResourceNode CurrentNodeTarget = null;
        [ReadOnly] public bool ActivelyMining = false;
        [ReadOnly] public bool ActivelyDumping = false;

        public readonly PawnInventory CurrentCarry = new PawnInventory();
        
        public float MoveSpeed { get; private set; }
        public float TurnSpeed { get; private set; }
        public float Acceleration { get; private set; }
        public float CarryCapacity { get; private set; }
        public float MetalGatherSpeed { get; private set; }
        public float OilGatherSpeed { get; private set; }

        private NavMeshAgent _navMeshAgent;
        private PawnControlData _controlData;

        public void Initialize(TeamPawnController teamController)
        {
            TeamController = teamController;

            _navMeshAgent = GetComponent<NavMeshAgent>();
            _controlData = teamController.Team.PawnControl;

            _navMeshAgent.acceleration = Acceleration = _controlData.Acceleration.Evaluate(Random.Range(0f, 1f));
            _navMeshAgent.speed = MoveSpeed = _controlData.MoveSpeed.Evaluate(Random.Range(0f, 1f));
            _navMeshAgent.angularSpeed = TurnSpeed = _controlData.TurnSpeed.Evaluate(Random.Range(0f, 1f));
            CarryCapacity = _controlData.CarryCapacity.Evaluate(Random.Range(0f, 1f));
            MetalGatherSpeed = _controlData.MetalGatherSpeed.Evaluate(Random.Range(0f, 1f));
            OilGatherSpeed = _controlData.OilGatherSpeed.Evaluate(Random.Range(0f, 1f));
        }

        public void UpdatePawn(float timeStep)
        {
            switch (CurrentGoal)
            {
                case PawnGoal.PitCrew:
                {
                    break;
                }
                case PawnGoal.Gather:
                {
                    if (ActivelyMining)
                    {
                        if (CurrentNodeTarget.CurrentCapacity > 0f)
                        {
                            switch (CurrentNodeTarget.NodeType)
                            {
                                case ResourceNodeType.Metal:
                                    float metalMineAmount = MetalGatherSpeed * timeStep;
                                    CurrentNodeTarget.CurrentCapacity -= metalMineAmount;
                                    CurrentCarry.Type = ResourceNodeType.Metal;
                                    CurrentCarry.Amount += metalMineAmount;
                                    break;
                                case ResourceNodeType.Oil:
                                    float oilMineAmount = OilGatherSpeed * timeStep;
                                    CurrentNodeTarget.CurrentCapacity -= oilMineAmount;
                                    CurrentCarry.Type = ResourceNodeType.Oil;
                                    CurrentCarry.Amount += oilMineAmount;
                                    break;
                            }

                            if (CurrentCarry.Amount >= CarryCapacity)
                            {
                                CurrentGoal = PawnGoal.ReturnToPit;
                                ActivelyMining = false;
                            }
                        }
                        else
                        {
                            CurrentGoal = PawnGoal.ReturnToPit;
                            CurrentNodeTarget.AssignedPawns.Remove(this);
                            CurrentNodeTarget = null;
                            ActivelyMining = false;
                        }
                    }
                    else if (!NavAgent.destination.IsApproximately(CurrentNodeTarget.transform.position))
                    {
                        NavAgent.SetDestination(CurrentNodeTarget.transform.position);
                    }

                    break;
                }

                case PawnGoal.ReturnToPit:
                {
                    if (ActivelyDumping)
                    {
                        if (CurrentCarry.Amount > 0f)
                        {
                            switch (CurrentCarry.Type)
                            {
                                case ResourceNodeType.Metal:
                                    float metalDumpAmount = MetalGatherSpeed * 2f * timeStep;
                                    CurrentCarry.Amount -= metalDumpAmount;
                                    TeamController.CurrentMetal += metalDumpAmount;
                                    break;
                                case ResourceNodeType.Oil:
                                    float oilDumpAmount = OilGatherSpeed * 2f * timeStep;
                                    CurrentCarry.Amount -= oilDumpAmount;
                                    TeamController.CurrentOil += oilDumpAmount;
                                    break;
                            }
                        }

                        if (CurrentCarry.Amount <= 0f)
                        {
                            ActivelyDumping = false;
                            CurrentCarry.Type = ResourceNodeType.Invalid;
                            if (CurrentNodeTarget != null)
                            {
                                if (CurrentNodeTarget.CurrentCapacity <= 0f)
                                {
                                    CurrentGoal = PawnGoal.PitCrew;
                                    CurrentNodeTarget.AssignedPawns.Remove(this);
                                    CurrentNodeTarget = null;
                                }
                                else
                                {
                                    CurrentGoal = PawnGoal.Gather;
                                }
                            }
                            else
                            {
                                CurrentGoal = PawnGoal.PitCrew;
                            }
                        }
                    }
                    else if (!NavAgent.destination.IsApproximately(TeamController.CrewLocation.transform.position))
                    {
                        NavAgent.SetDestination(TeamController.CrewLocation.transform.position);
                    }

                    break;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CurrentGoal == PawnGoal.Gather &&
                CurrentNodeTarget != null &&
                CurrentNodeTarget == other.GetComponent<ResourceNode>())
            {
                ActivelyMining = true;
            }
            else if (CurrentGoal == PawnGoal.ReturnToPit &&
                     TeamController.CrewLocation == other.GetComponent<PitCrewLocation>())
            {
                ActivelyDumping = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (CurrentNodeTarget == other.GetComponent<ResourceNode>())
            {
                ActivelyMining = false;
            }
            else if (TeamController.CrewLocation == other.GetComponent<PitCrewLocation>())
            {
                ActivelyDumping = false;
            }
        }
    }
}