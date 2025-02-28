using UnityEngine;

namespace GorillaCosmeticsReborn
{
    public class Descriptor : MonoBehaviour
    {
        public Sprite? CosmeticIcon;
        public string Name = "Cosmetic Name";
        public string Author = "Your Name";
            
        public Vector3 wardrobePositionOffset;
        public Quaternion wardrobeRotationOffset;

#if Runtime
        public virtual GorillaTag.CosmeticSystem.GTHardCodedBones.EBone boneToParentTo => GorillaTag.CosmeticSystem.GTHardCodedBones.EBone.body;
        
        public virtual bool IsSpecialCosmetic => false;
        
        public virtual GorillaNetworking.CosmeticsController.CosmeticCategory Category => GorillaNetworking.CosmeticsController.CosmeticCategory.None;
#endif
    }
}