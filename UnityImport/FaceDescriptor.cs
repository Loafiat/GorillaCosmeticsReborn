using UnityEngine;

namespace GorillaCosmeticsReborn
{
    public class FaceDescriptor : Descriptor
    {
        public Vector3 positionOffset;
        public Quaternion rotationOffset;

#if Runtime
        public override GorillaTag.CosmeticSystem.GTHardCodedBones.EBone boneToParentTo => GorillaTag.CosmeticSystem.GTHardCodedBones.EBone.head;
        
        public override GorillaNetworking.CosmeticsController.CosmeticCategory Category => GorillaNetworking.CosmeticsController.CosmeticCategory.Face;
#endif
    }
}