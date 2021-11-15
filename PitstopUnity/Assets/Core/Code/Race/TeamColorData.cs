using UnityEngine;

namespace SadPumpkin.Game.Pitstop.Core.Code.Race
{
    [CreateAssetMenu]
    public class TeamColorData : ScriptableObject
    {
        public Material PawnMaterial;
        public Color PawnColor;
        public Texture2D PawnTexture;
        
        [Space(15f)]
        public Material CarPrimaryMaterial;
        public Color CarPrimaryColor;
        public Texture2D CarPrimaryTexture;
        public Material CarSecondaryMaterial;
        public Color CarSecondaryColor;
        public Texture2D CarSecondaryTexture;

        public void ApplyToPawn(PawnComponent pawn)
        {
            foreach (Renderer renderer in pawn.GetComponentsInChildren<Renderer>())
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (PawnMaterial != null &&
                        materials[i].name.Contains(PawnMaterial.name))
                    {
                        materials[i] = new Material(PawnMaterial)
                        {
                            mainTexture = PawnTexture,
                            color = PawnColor,
                        };
                    }
                }
                renderer.materials = materials;
            }
        }

        public void ApplyToCar(CarComponent car)
        {
            foreach (Renderer renderer in car.GetComponentsInChildren<Renderer>())
            {
                Material[] materials = renderer.sharedMaterials;
                for (int i = 0; i < materials.Length; i++)
                {
                    if (CarPrimaryMaterial != null &&
                        materials[i].name.Contains(CarPrimaryMaterial.name))
                    {
                        materials[i] = new Material(CarPrimaryMaterial)
                        {
                            mainTexture = CarPrimaryTexture,
                            color = CarPrimaryColor,
                        };
                    }
                    else if (CarSecondaryMaterial != null &&
                             materials[i].name.Contains(CarSecondaryMaterial.name))
                    {
                        materials[i] = new Material(CarSecondaryMaterial)
                        {
                            mainTexture = CarSecondaryTexture,
                            color = CarSecondaryColor,
                        };
                    }
                }
                renderer.materials = materials;
            }
        }
    }
}