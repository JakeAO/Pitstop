using SadPumpkin.Game.Pitstop.Core.Code.Race;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Pit;

namespace SadPumpkin.Game.Pitstop.Core.Code.RTS.Pawns
{
    public class TeamPawnController_Player : TeamPawnController
    {
        public override bool Player => true;
        
        public TeamPawnController_Player(TeamData team, PitCrewLocation pitLocation) : base(team, pitLocation)
        {
        }
    }
}