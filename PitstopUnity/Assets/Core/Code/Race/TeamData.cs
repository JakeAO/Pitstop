using SadPumpkin.Game.Pitstop.Core.Code.Race.Cars;
using SadPumpkin.Game.Pitstop.Core.Code.Race.Drivers;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Pawns;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race
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
        public PawnAIData PawnAI;

        public CarComponent CarInstance;
    }
}