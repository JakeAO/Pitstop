using SadPumpkin.Game.Pitstop.Core.Code;
using SadPumpkin.Game.Pitstop.Core.Code.RTS;

namespace SadPumpkin.Game.Pitstop
{
    public class TeamPawnController_Player : TeamPawnController
    {
        public override bool Player => true;
        
        public TeamPawnController_Player(TeamData team, PitCrewLocation pitLocation) : base(team, pitLocation)
        {
        }
    }
}