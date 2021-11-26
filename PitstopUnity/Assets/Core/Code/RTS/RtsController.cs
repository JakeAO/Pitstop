using System.Collections.Generic;
using System.Linq;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Props;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.RTS
{
    public class RtsController : MonoBehaviour
    {
        public PitCrewLocation[] PawnSpawnLocations;
        public float PawnSpawnRadius;
        public uint PawnSpawnCount;

        [ReadOnly] public Dictionary<TeamData, TeamPawnController> ControllerByTeam;
        [ReadOnly] public Dictionary<TeamData, PitCrewLocation> PitLocationByTeam;
        [ReadOnly] public Dictionary<ResourceNodeType, HashSet<ResourceNode>> ResourceNodesByType;

        public void Initialize(TeamData localTeam, TeamData[] rivalTeams)
        {
            ControllerByTeam = new Dictionary<TeamData, TeamPawnController>(rivalTeams.Length + 1);
            PitLocationByTeam = new Dictionary<TeamData, PitCrewLocation>(rivalTeams.Length + 1);
            ControllerByTeam[localTeam] = new TeamPawnController(localTeam, PawnSpawnLocations[0]);
            ControllerByTeam[localTeam].SpawnPawns(PawnSpawnRadius, PawnSpawnCount);
            PitLocationByTeam[localTeam] = PawnSpawnLocations[0];
            for (int i = 0; i < rivalTeams.Length; i++)
            {
                TeamData rivalTeam = rivalTeams[i];
                ControllerByTeam[rivalTeam] = new TeamPawnController(rivalTeam, PawnSpawnLocations[i + 1]);
                ControllerByTeam[rivalTeam].SpawnPawns(PawnSpawnRadius, PawnSpawnCount);
                PitLocationByTeam[rivalTeam] = PawnSpawnLocations[i + 1];
            }

            ResourceNodesByType = FindObjectsOfType<ResourceNode>()
                .GroupBy(x => x.NodeType)
                .ToDictionary(
                    x => x.Key,
                    x => new HashSet<ResourceNode>(x));
        }

        public void UpdateRts(float timeStep)
        {
            foreach (var teamControllerKvp in ControllerByTeam)
            {
                teamControllerKvp.Value.UpdatePawns(timeStep);
            }
        }
    }
}