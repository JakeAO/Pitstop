using System.Collections.Generic;
using System.Linq;
using SadPumpkin.Game.Pitstop.Core.Code.Race;
using SadPumpkin.Game.Pitstop.Core.Code.Race.Track;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Pawns;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Pit;
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
        [ReadOnly] public Dictionary<TeamData, PitCrewLocation> PitCrewLocationByTeam;
        [ReadOnly] public Dictionary<TeamData, PitCarLocation> PitCarLocationByTeam;
        [ReadOnly] public Dictionary<ResourceNodeType, HashSet<ResourceNode>> ResourceNodesByType;

        public void Initialize(TeamData localTeam, TeamData[] rivalTeams, TrackComponent track)
        {
            ControllerByTeam = new Dictionary<TeamData, TeamPawnController>(rivalTeams.Length + 1);
            PitCrewLocationByTeam = new Dictionary<TeamData, PitCrewLocation>(rivalTeams.Length + 1);
            PitCarLocationByTeam = new Dictionary<TeamData, PitCarLocation>(rivalTeams.Length + 1);
            ControllerByTeam[localTeam] = new TeamPawnController_Player(localTeam, PawnSpawnLocations[0]);
            ControllerByTeam[localTeam].SpawnPawns(PawnSpawnRadius, PawnSpawnCount);
            PitCrewLocationByTeam[localTeam] = PawnSpawnLocations[0];
            PitCrewLocationByTeam[localTeam].Initialize(ControllerByTeam[localTeam]);
            PitCarLocationByTeam[localTeam] = track.PitPositions[0];
            PitCarLocationByTeam[localTeam].Initialize(ControllerByTeam[localTeam]);
            for (int i = 0; i < rivalTeams.Length; i++)
            {
                TeamData rivalTeam = rivalTeams[i];
                ControllerByTeam[rivalTeam] = new TeamPawnController_AI(rivalTeam, PawnSpawnLocations[i + 1]);
                ControllerByTeam[rivalTeam].SpawnPawns(PawnSpawnRadius, PawnSpawnCount);
                PitCrewLocationByTeam[rivalTeam] = PawnSpawnLocations[i + 1];
                PitCrewLocationByTeam[rivalTeam].Initialize(ControllerByTeam[rivalTeam]);
                PitCarLocationByTeam[rivalTeam] = track.PitPositions[i + 1];
                PitCarLocationByTeam[rivalTeam].Initialize(ControllerByTeam[rivalTeam]);
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