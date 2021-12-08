using System.Collections.Generic;
using SadPumpkin.Game.Pitstop.Core.Code.RTS.Pawns;
using SadPumpkin.Game.Pitstop.Core.Code.UI.Gameplay;
using SadPumpkin.Game.Pitstop.Core.Code.Util;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SadPumpkin.Game.Pitstop.Core.Code.RTS.Props
{
    public class ResourceNode : MonoBehaviour, IPointerClickHandler
    {
        public ResourceInteractionUI InteractionUI;
        public ResourceNodeType NodeType = ResourceNodeType.Invalid;
        public float CurrentCapacity;
        public uint MaxPawns;
        
        [ReadOnly] public HashSet<PawnComponent> AssignedPawns = new HashSet<PawnComponent>();
        
        public uint CurrentPawns => (uint)AssignedPawns.Count;

        public void OnPointerClick(PointerEventData eventData)
        {
            InteractionUI.UpdateActive(!InteractionUI.gameObject.activeSelf);
        }

        private void LateUpdate()
        {
            if (CurrentCapacity <= 0f)
            {
                foreach (PawnComponent assignedPawn in AssignedPawns)
                {
                    assignedPawn.CurrentGoal = PawnGoal.ReturnToPit;
                    assignedPawn.ActivelyMining = false;
                    assignedPawn.CurrentNodeTarget = null;
                }
                AssignedPawns.Clear();
                
                // TODO something
                
                Destroy(gameObject);
            }
        }
    }
}