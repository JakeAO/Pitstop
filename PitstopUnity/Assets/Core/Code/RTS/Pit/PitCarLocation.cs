using System;
using System.Collections.Generic;
using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.RTS
{
    [RequireComponent(typeof(Collider))]
    public class PitCarLocation : MonoBehaviour
    {
        private TeamPawnController _teamPawnController;

        private CarComponent CarInstance => _teamPawnController?.Team?.CarInstance;
        private float AvailableMetal => _teamPawnController.CurrentMetal;
        private float AvailableOil => _teamPawnController.CurrentOil;
        private IEnumerable<PawnComponent> AvailableWorkers => _teamPawnController.PawnsAtHome;

        public void Initialize(TeamPawnController teamPawnController)
        {
            _teamPawnController = teamPawnController;

            gameObject.SetActive(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CarInstance.CarCollider == other &&
                CarInstance.CurrentGoal == CarGoal.GoToPit)
            {
                CarInstance.CurrentGoal = CarGoal.PitStop;
            }
        }

        private void Update()
        {
            if (CarInstance != null &&
                CarInstance.CurrentGoal == CarGoal.PitStop)
            {
                if ((CarInstance.CurrentCarBody < CarInstance.MaxBody && _teamPawnController.CurrentMetal > 0f) ||
                    (CarInstance.CurrentCarFuel < CarInstance.MaxFuel && _teamPawnController.CurrentOil > 0f) ||
                    CarInstance.CurrentDriverStamina < CarInstance.MaxStamina)
                {
                    float timeStep = Time.deltaTime;
                    float maxPossibleRepairValue = 0f;
                    float maxPossibleRefuelValue = 0f;
                    foreach (PawnComponent worker in AvailableWorkers)
                    {
                        if (worker.CurrentGoal == PawnGoal.PitCrew)
                        {
                            maxPossibleRepairValue += worker.RepairSpeed;
                            maxPossibleRefuelValue += worker.RefuelSpeed;
                        }
                    }

                    float repairValue = Mathf.Min(Mathf.Min(maxPossibleRepairValue, AvailableMetal) * timeStep, CarInstance.MaxBody - CarInstance.CurrentCarBody);
                    float refuelValue = Mathf.Min(Mathf.Min(maxPossibleRefuelValue, AvailableOil) * timeStep, CarInstance.MaxFuel - CarInstance.CurrentCarFuel);
                    float staminaValue = Mathf.Min(CarInstance.StaminaRecoverySpeed * timeStep, CarInstance.MaxStamina - CarInstance.CurrentDriverStamina);

                    if (CarInstance.CurrentCarBody < CarInstance.MaxBody)
                    {
                        CarInstance.CurrentCarBody += repairValue;
                        _teamPawnController.CurrentMetal -= repairValue;
                    }

                    if (CarInstance.CurrentCarFuel < CarInstance.MaxFuel)
                    {
                        CarInstance.CurrentCarFuel += refuelValue;
                        _teamPawnController.CurrentOil -= refuelValue;
                    }

                    if (CarInstance.CurrentDriverStamina < CarInstance.MaxStamina)
                    {
                        CarInstance.CurrentDriverStamina += staminaValue;
                    }
                }
                else
                {
                    CarInstance.CurrentGoal = CarGoal.Race;
                }
            }
        }
    }
}