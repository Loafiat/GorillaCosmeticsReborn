using UnityEngine;

namespace GorillaCosmeticsReborn
{
    public class FurDescriptor : Descriptor
    {
        public Material bodyMat;
        public Material chestMat;

#if Runtime
        public override GorillaTag.CosmeticSystem.GTHardCodedBones.EBone boneToParentTo => GorillaTag.CosmeticSystem.GTHardCodedBones.EBone.None;
        
        public override bool IsSpecialCosmetic => true;
        
        public override GorillaNetworking.CosmeticsController.CosmeticCategory Category => GorillaNetworking.CosmeticsController.CosmeticCategory.Fur;
#endif
    }
}