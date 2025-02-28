using UnityEngine;

namespace GorillaCosmeticsReborn
{
    public class PantsDescriptor : Descriptor
    {
        public Vector3 positionOffset;
        public Quaternion rotationOffset;

#if Runtime
        public override GorillaTag.CosmeticSystem.GTHardCodedBones.EBone boneToParentTo => GorillaTag.CosmeticSystem.GTHardCodedBones.EBone.body;
        
        public override GorillaNetworking.CosmeticsController.CosmeticCategory Category => GorillaNetworking.CosmeticsController.CosmeticCategory.Pants;
#endif
    }
}