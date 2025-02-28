using UnityEngine;

namespace GorillaCosmeticsReborn
{
    public class HoldableDescriptor : Descriptor
    {
        public DockPositions dockPositions;

        public Vector3 holdableDockOffset;
        public Vector3 holdableGrabbedOffset;
        public Quaternion holdableDockRotOffset;
        public Quaternion holdableGrabbedRotOffset;
    }

    public enum DockPositions
    {
        LeftArm = 1,
        RightArm = 2,
        Arms = LeftArm | RightArm,
        Chest = 4,
        LeftBack = 8,
        RightBack = 16,
        Back = LeftBack | RightBack
    } 
}