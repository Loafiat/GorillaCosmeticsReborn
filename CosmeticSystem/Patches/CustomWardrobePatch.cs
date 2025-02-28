#if Runtime
using System.Linq;
using GorillaNetworking;
using HarmonyLib;
using TMPro;
using UnityEngine;
using static GorillaCosmeticsReborn.CosmeticSystem.CustomCosmeticController;

namespace GorillaCosmeticsReborn.CosmeticSystem.Patches
{
    [HarmonyPatch(typeof(CosmeticWardrobe))]
    public class CustomWardrobePatch
    {
        [HarmonyPrefix, HarmonyPatch("Start")]
        public static void StartPrefix(CosmeticWardrobe __instance)
        {
            // Here's the ugly part.
            Transform customButton = Object.Instantiate(__instance.cosmeticCategoryButtons[0].button.gameObject).transform;
            customButton.gameObject.name = "CustomWardrobe_Button";
            customButton.SetParent(__instance.cosmeticCategoryButtons[0].button.transform.parent, false);
            customButton.localPosition = new Vector3(-0.6f, -0.1f, 0f);
            Object.Destroy(customButton.gameObject.GetComponent<CosmeticCategoryButton>());
            SwitchWardrobeButton buttonScript = customButton.AddComponent<SwitchWardrobeButton>();
            buttonScript.buttonRenderer = customButton.gameObject.GetComponent<MeshRenderer>();
            buttonScript.pressedMaterial = __instance.cosmeticCategoryButtons[0].button.pressedMaterial;
            buttonScript.unpressedMaterial = __instance.cosmeticCategoryButtons[0].button.unpressedMaterial;
            buttonScript.UpdateColor();
        }
        
        [HarmonyPrefix, HarmonyPatch("HandlePressedNextSelection")]
        public static bool HandlePressedNextSelectionPrefix(CosmeticWardrobe __instance)
        {
            if (!isModdedWardrobe) return true;
            CosmeticWardrobe.startingDisplayIndex += __instance.cosmeticCollectionDisplays.Length;
            if (CosmeticWardrobe.startingDisplayIndex + __instance.cosmeticCollectionDisplays.Length >
                CosmeticCategories[CosmeticWardrobe.selectedCategory].Count)
                CosmeticWardrobe.startingDisplayIndex = 0;
            __instance.UpdateCosmeticDisplays();
            return false;
        }
        
        [HarmonyPrefix, HarmonyPatch("HandlePressedPrevSelection")]
        public static bool HandlePressedPrevSelectionPrefix(CosmeticWardrobe __instance)
        {
            if (isModdedWardrobe) return true;
            CosmeticWardrobe.startingDisplayIndex -= __instance.cosmeticCollectionDisplays.Length;
            if (CosmeticWardrobe.startingDisplayIndex - __instance.cosmeticCollectionDisplays.Length < 0)
            {
                int categorySize = CosmeticCategories[CosmeticWardrobe.selectedCategory].Count;
                int startOfPage;
                if (categorySize % __instance.cosmeticCollectionDisplays.Length == 0)
                    startOfPage = categorySize - __instance.cosmeticCollectionDisplays.Length;
                else
                {
                    startOfPage = categorySize / __instance.cosmeticCollectionDisplays.Length;
                    startOfPage *= __instance.cosmeticCollectionDisplays.Length;
                }
                CosmeticWardrobe.startingDisplayIndex = startOfPage;
            }
            __instance.UpdateCosmeticDisplays();
            return false;
        }
        
        [HarmonyPrefix, HarmonyPatch("HandlePressedSelectCosmeticButton")]
        public static bool HandlePressedSelectCosmeticButtonPrefix(CosmeticWardrobe __instance, GorillaPressableButton button, bool isLeft)
        {
            if (!isModdedWardrobe)
            {
                DisableCosmetic(CosmeticWardrobe.selectedCategory);
                return true;
            }
            int cosmeticIndexToSelect = CosmeticWardrobe.startingDisplayIndex + __instance.cosmeticCollectionDisplays
                .Select(x => x.selectButton).ToList().IndexOf((CosmeticButton)button);
            if (button.isOn)
                DisableCosmetic(CosmeticWardrobe.selectedCategory);
            else
                EnableCosmetic(CosmeticWardrobe.selectedCategory,
                    CosmeticCategories[CosmeticWardrobe.selectedCategory][cosmeticIndexToSelect]);
            __instance.UpdateCosmeticDisplays();
            return false;
        }
        
        [HarmonyPrefix, HarmonyPatch("HandleChangeCategory")]
        public static bool HandleChangeCategoryPrefix(CosmeticWardrobe __instance, GorillaPressableButton button)
        {
            if (!isModdedWardrobe) return true;
            CosmeticWardrobe.CosmeticWardrobeCategory category = __instance.cosmeticCategoryButtons.First(x => x.button == button);
            if (CosmeticWardrobe.selectedCategory == category.category)
            {
                Descriptor? hatDisabled = GetEquippedCosmeticAtSlot(category.category);
                if (hatDisabled != null)
                {
                    SavedCosmeticForReenable.Add(category.category,
                        CosmeticCategories[category.category].First(x =>
                            x.Name == hatDisabled.Name && x.Author == hatDisabled.Author));
                    DisableCosmetic(category.category);
                }
                else if (SavedCosmeticForReenable.ContainsKey(category.category))
                {
                    EnableCosmetic(category.category,
                        SavedCosmeticForReenable[category.category]);
                    SavedCosmeticForReenable.Remove(category.category);
                }
            }
            CosmeticWardrobe.selectedCategory = category.category;
            __instance.UpdateCategoryButtons();
            __instance.UpdateCosmeticDisplays();
            return false;
        }
        
        [HarmonyPrefix, HarmonyPatch("UpdateCosmeticDisplays")]
        public static bool UpdateCosmeticDisplaysPrefix(CosmeticWardrobe __instance)
        {
            if (!isModdedWardrobe)
            {
                for (int i = 0; i < __instance.cosmeticCollectionDisplays.Length; i++)
                {
                    __instance.cosmeticCollectionDisplays[i].displayHead.ClearCustomModelsFromHead();
                }
                return true;
            }
            for (int i = 0; i < __instance.cosmeticCollectionDisplays.Length; i++)
            {
                Descriptor? cosmetic;
                try
                {
                    cosmetic = CosmeticCategories[CosmeticWardrobe.selectedCategory][
                        CosmeticWardrobe.selectedCategoryIndex + i];
                }
                catch
                {
                    continue;
                }
                __instance.cosmeticCollectionDisplays[i].currentCosmeticItem =
                    CosmeticsController.instance.nullItem;
                __instance.cosmeticCollectionDisplays[i].displayHead._ClearCurrent();
                __instance.cosmeticCollectionDisplays[i].displayHead.ClearCustomModelsFromHead();
                if (cosmetic != null)
                {
                    __instance.cosmeticCollectionDisplays[i].displayHead.AddCustomModelToHead(cosmetic.gameObject,
                        cosmetic.wardrobePositionOffset, cosmetic.wardrobeRotationOffset);
                    Descriptor eqCosmetic = GetEquippedCosmeticAtSlot(CosmeticWardrobe.selectedCategory)!;
                    __instance.cosmeticCollectionDisplays[i].selectButton.isOn = eqCosmetic != null &&
                        eqCosmetic.Name == cosmetic.Name && eqCosmetic.Author == cosmetic.Author;
                }
                __instance.cosmeticCollectionDisplays[i].selectButton.enabled = cosmetic != null;
                __instance.cosmeticCollectionDisplays[i].selectButton.UpdateColor();
            }
            int categorySize = CosmeticCategories[CosmeticWardrobe.selectedCategory].Count;
            __instance.nextSelection.enabled =
                categorySize > __instance.cosmeticCollectionDisplays.Length;
            __instance.nextSelection.UpdateColor();
            __instance.prevSelection.enabled =
                categorySize > __instance.cosmeticCollectionDisplays.Length;
            __instance.prevSelection.UpdateColor();
            return false;
        }
        
        [HarmonyPostfix, HarmonyPatch("HandleCosmeticsUpdated")]
        public static void HandleCosmeticsUpdatedPostfix(CosmeticWardrobe __instance)
        {
            __instance.currentEquippedDisplay.ClearCustomModelsFromHead();
            foreach (Descriptor descriptor in EquippedCosmetics)
            {
                __instance.currentEquippedDisplay.AddCustomModelToHead(descriptor.gameObject, descriptor.wardrobePositionOffset, descriptor.wardrobeRotationOffset);
            }
        }
        
        [HarmonyPrefix, HarmonyPatch("UpdateCategoryButtons")]
        public static bool UpdateCategoryButtonsPrefix(CosmeticWardrobe __instance)
        {
            CosmeticsController.CosmeticSet currentSet = CosmeticsController.instance.currentWornSet;
            foreach (CosmeticWardrobe.CosmeticWardrobeCategory category in __instance.cosmeticCategoryButtons)
            {
                category.button.SetIcon(null);
                bool EquippedCustomCosmeticInCat = GetEquippedCosmeticAtSlot(category.category) != null;
                bool EquippedDefaultCosmeticInCat = currentSet.HasItemOfCategory(category.category);
                if (EquippedDefaultCosmeticInCat || EquippedCustomCosmeticInCat)
                {
                    switch (category.category)
                    {
                        case CosmeticsController.CosmeticCategory.Arms:
                        case CosmeticsController.CosmeticCategory.Back:
                        case CosmeticsController.CosmeticCategory.Paw:
                            if (EquippedDefaultCosmeticInCat)
                            {
                                category.button.SetDualIcon(
                                    currentSet.GetItemOfCategory(category.category, true, true).itemPicture,
                                    currentSet.GetItemOfCategory(category.category, true, false).itemPicture);
                            }
                            // CUSTOM IS UNFINISHED!
                            break;
                        default:
                            if (EquippedDefaultCosmeticInCat)
                            {
                                category.button.SetIcon(currentSet.GetItemOfCategory(category.category).itemPicture);
                                break;
                            }
                            category.button.SetIcon(GetEquippedCosmeticAtSlot(category.category)!.CosmeticIcon);
                            break;
                    }
                }

                category.button.enabled = isModdedWardrobe
                    ? CosmeticCategories.ContainsKey(category.category) &&
                      CosmeticCategories[category.category].Count > 0
                    : CosmeticsController.instance.GetCategorySize(category.category) > 0;
                category.button.isOn = category.category == CosmeticWardrobe.selectedCategory;
                category.button.UpdateColor();
            }
            return false;
        }
    }
}
#endif