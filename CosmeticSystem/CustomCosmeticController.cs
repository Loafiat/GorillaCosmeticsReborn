#if Runtime
using System.Collections.Generic;
using System.Linq;
using GorillaNetworking;
using GorillaTag.CosmeticSystem;
using UnityEngine;

namespace GorillaCosmeticsReborn.CosmeticSystem
{
    public class CustomCosmeticController
    {
        public static bool isModdedWardrobe;

        public static Dictionary<CosmeticsController.CosmeticCategory, List<Descriptor>> CosmeticCategories =
            new Dictionary<CosmeticsController.CosmeticCategory, List<Descriptor>>
            {
                { CosmeticsController.CosmeticCategory.Hat, new List<Descriptor>() },
                { CosmeticsController.CosmeticCategory.Face, new List<Descriptor>() },
                { CosmeticsController.CosmeticCategory.Badge, new List<Descriptor>() },
                { CosmeticsController.CosmeticCategory.Shirt, new List<Descriptor>() },
                { CosmeticsController.CosmeticCategory.Pants, new List<Descriptor>() },
                { CosmeticsController.CosmeticCategory.Fur, new List<Descriptor>() }
            };

        public static Dictionary<CosmeticsController.CosmeticCategory, Descriptor> SavedCosmeticForReenable =
            new Dictionary<CosmeticsController.CosmeticCategory, Descriptor>();

        public static List<Descriptor> EquippedCosmetics = new List<Descriptor>();

        public static void ToggleModdedWardrobe()
        {
            isModdedWardrobe = !isModdedWardrobe;
            foreach (CosmeticWardrobe wardrobe in Object.FindObjectsOfType<CosmeticWardrobe>(true))
            {
                foreach (CosmeticWardrobe.CosmeticWardrobeSelection selection in wardrobe.cosmeticCollectionDisplays)
                {
                    selection.displayHead._ClearCurrent();
                }
                wardrobe.UpdateCategoryButtons();
                wardrobe.UpdateCosmeticDisplays();
            }
        }
        
        public static Descriptor? GetEquippedCosmeticAtSlot(CosmeticsController.CosmeticCategory category)
        {
            if (EquippedCosmetics.Select(x => x.Category).Contains(category))
            {
                return EquippedCosmetics.FirstOrDefault(x => x.Category == category);
            }
            return null;
        }

        public static void EnableCosmetic(CosmeticsController.CosmeticCategory category, Descriptor cosmetic, bool leftHand = false)
        {
            bool isHoldable = category == CosmeticsController.CosmeticCategory.Arms ||
                              category == CosmeticsController.CosmeticCategory.Back ||
                              category == CosmeticsController.CosmeticCategory.Paw;
            if (CosmeticsController.instance.currentWornSet.HasItemOfCategory(category))
            {
                CosmeticsController.instance.currentWornSet.items[
                    CosmeticsController.instance.currentWornSet.items.ToList().IndexOf(
                        CosmeticsController.instance.currentWornSet.GetItemOfCategory(category, isHoldable,
                            leftHand))] = CosmeticsController.instance.nullItem;
            }

            if (GetEquippedCosmeticAtSlot(category) != null)
                DisableCosmetic(category, leftHand);
            Descriptor newHat = Object.Instantiate(cosmetic.gameObject).GetComponent<Descriptor>();
            EquippedCosmetics.Add(newHat);
            if (newHat.IsSpecialCosmetic)
            {
                switch (newHat.Category)
                {
                    case CosmeticsController.CosmeticCategory.Fur:
                        Material bodyMatToEq = ((FurDescriptor)newHat).bodyMat != null
                            ? ((FurDescriptor)newHat).bodyMat
                            : GorillaTagger.Instance.offlineVRRig.CurrentCosmeticSkin.bodyMaterial;
                        Material chestMatToEq = ((FurDescriptor)newHat).chestMat != null
                            ? ((FurDescriptor)newHat).chestMat
                            : GorillaTagger.Instance.offlineVRRig.CurrentCosmeticSkin.chestMaterial;
                        GorillaTagger.Instance.offlineVRRig.bodyRenderer.SetSkinMaterials(bodyMatToEq, chestMatToEq);
                        break;
                }
                return;
            }
            if (!GTHardCodedBones.TryGetBoneXforms(GorillaTagger.Instance.offlineVRRig, out Transform[] bones,
                    out string errorMsg))
                Debug.LogError(errorMsg);
            GTHardCodedBones.TryGetBoneXform(bones, newHat.boneToParentTo, out Transform boneXform);
            newHat.transform.SetParent(boneXform);
            newHat.transform.localPosition = ((HatDescriptor)newHat).positionOffset;
            newHat.transform.localRotation = ((HatDescriptor)newHat).rotationOffset;
            CosmeticsController.instance.OnCosmeticsUpdated();
            CosmeticsController.instance.UpdateWornCosmetics(NetworkSystem.Instance.InRoom);
        }

        public static void DisableCosmetic(CosmeticsController.CosmeticCategory category, bool leftHand = false)
        {
            Descriptor? cosmeticAtCategory = GetEquippedCosmeticAtSlot(category);
            if (cosmeticAtCategory == null)
                return;
            if (cosmeticAtCategory.IsSpecialCosmetic)
            {
                switch (category)
                {
                    case CosmeticsController.CosmeticCategory.Fur:
                        GorillaSkin.ShowActiveSkin(GorillaTagger.Instance.offlineVRRig);
                        break;
                }
            }
            EquippedCosmetics.Remove(cosmeticAtCategory);
            Object.Destroy(cosmeticAtCategory.gameObject);
            CosmeticsController.instance.OnCosmeticsUpdated();
            CosmeticsController.instance.UpdateWornCosmetics(NetworkSystem.Instance.InRoom);
        }
    }
}
#endif