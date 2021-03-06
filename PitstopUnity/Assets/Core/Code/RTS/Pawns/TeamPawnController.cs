using System.Collections.Generic;
using System.Linq;
using SadPumpkin.Game.Pitstop.Core.Code.Race;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Pit;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Props;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.RTS.Pawns
{
    public abstract class TeamPawnController
    {
        public abstract bool Player { get; }
        
        [ReadOnly] public TeamData Team;
        [ReadOnly] public PitCrewLocation CrewLocation;
        [ReadOnly] public List<PawnComponent> PawnInstances;

        public float CurrentMetal = 0f;
        public float CurrentOil = 0f;
        
        public IEnumerable<PawnComponent> PawnsAtWork => PawnInstances.Where(x => x.CurrentNodeTarget != null);
        public IEnumerable<PawnComponent> PawnsAtHome => PawnInstances.Where(x => x.CurrentNodeTarget == null);
        public uint NumPawnsAtWork => (uint)PawnsAtWork.Count();
        public uint NumPawnsAtHome => (uint)PawnsAtHome.Count();

        protected TeamPawnController(TeamData team, PitCrewLocation pitLocation)
        {
            Team = team;
            CrewLocation = pitLocation;
        }

        public virtual void SpawnPawns(float spawnRadius, uint pawnCount)
        {
            PawnInstances = new List<PawnComponent>((int)pawnCount);
            for (int i = 0; i < pawnCount; i++)
            {
                float angle = Random.Range(0f, 360f);
                Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

                float distance = Random.Range(0f, spawnRadius);
                Vector3 localPosition = direction * distance;

                PawnComponent pawn = GameObject.Instantiate(Team.PawnModel.Prefab, CrewLocation.transform);
                pawn.transform.localPosition = localPosition;
                pawn.transform.localRotation = Quaternion.LookRotation(direction, Vector3.up);
                pawn.transform.localScale = Vector3.one;
                pawn.Initialize(this);

                PawnInstances.Add(pawn);
                Team.TeamColor.ApplyToPawn(pawn);
            }
        }

        public virtual void UpdatePawns(float timeStep)
        {
            foreach (PawnComponent pawnInstance in PawnInstances)
            {
                pawnInstance.UpdatePawn(timeStep);
            }
        }

        public bool RequestPawnAdd(ResourceNode node)
        {
            PawnComponent pawnClosestToNode = PawnInstances
                .Where(x =>
                    x.CurrentGoal == PawnGoal.PitCrew ||
                    (x.CurrentNodeTarget == null && x.CurrentGoal == PawnGoal.ReturnToPit && x.CurrentCarry.Type == node.NodeType))
                .OrderBy(x => Vector3.Distance(x.transform.position, node.transform.position))
                .FirstOrDefault();

            if (pawnClosestToNode &&
                node.CurrentPawns < node.MaxPawns)
            {
                pawnClosestToNode.CurrentNodeTarget = node;
                pawnClosestToNode.CurrentGoal = PawnGoal.Gather;
                node.AssignedPawns.Add(pawnClosestToNode);
                return true;
            }

            return false;
        }

        public bool RequestPawnRemove(ResourceNode node)
        {
            PawnComponent pawnFarthestFromNode = PawnInstances
                .Where(x => x.CurrentNodeTarget == node)
                .OrderByDescending(x => Vector3.Distance(x.transform.position, node.transform.position))
                .FirstOrDefault();

            if (pawnFarthestFromNode)
            {
                pawnFarthestFromNode.CurrentNodeTarget = null;
                pawnFarthestFromNode.CurrentGoal = PawnGoal.ReturnToPit;
                node.AssignedPawns.Remove(pawnFarthestFromNode);
                return true;
            }

            return false;
        }

        public uint CountAssignedPawns(ResourceNode node)
        {
            uint count = 0;
            foreach (PawnComponent pawn in PawnInstances)
            {
                if (pawn.CurrentNodeTarget == node)
                    count++;
            }

            return count;
        }
    }
}