using SadPumpkin.Game.Pitstop.Core.Code.Race;

namespace SadPumpkin.Game.Pitstop.Core.Code
{
    public class TeamData
    {
        public TeamColorData TeamColor;

        public DriverStatusData DriverStatus;
        public DriverVisionData DriverVision;

        public CarModelData CarModel;
        public CarStatusData CarStatus;
        public CarControlData CarControl;

        public PawnModelData PawnModel;
        public PawnControlData PawnControl;

        public CarComponent CarInstance;
    }
}