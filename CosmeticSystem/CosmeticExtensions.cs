#if Runtime
using System.Collections.Generic;
using System.Linq;
using BuildSafe;
using GorillaNetworking;
using GorillaTag;
using GorillaTag.CosmeticSystem;
using UnityEngine;

namespace GorillaCosmeticsReborn.CosmeticSystem
{
    public static class CosmeticExtensions
    {
        public static CosmeticsController.CosmeticItem GetItemOfCategory(this CosmeticsController.CosmeticSet set,
            CosmeticsController.CosmeticCategory category, bool isHoldable = false, bool isLeft = false)
        {
            foreach (CosmeticsController.CosmeticItem item in set.items)
            {
                if (!isHoldable && item.itemCategory == category)
                {
                    return item;
                }
                if (item.itemCategory == category && CosmeticsController.CosmeticSet.IsSlotLeftHanded((CosmeticsController.CosmeticSlots)set.items.ToList().IndexOf(item)) == isLeft)
                {
                    return item;
                }
            }
            return CosmeticsController.instance.nullItem;
        }

        public static void AddCustomModelToHead(this HeadModel head, GameObject model, Vector3 positionOffset, Quaternion rotationOffset)
        {
            Transform newModel = Object.Instantiate(model).transform;
            if (model.TryGetComponent(out Descriptor cosmeticDescriptor) && cosmeticDescriptor.IsSpecialCosmetic)
            {
                switch (cosmeticDescriptor.Category)
                {
                    case CosmeticsController.CosmeticCategory.Fur:
                        Debug.Log("wat");
                        MeshRenderer meshRenderer = head.gameObject.GetComponent<MeshRenderer>();
                        if (!fallbackCustomMats.ContainsKey(head))
                            fallbackCustomMats.Add(head, new [] { meshRenderer.sharedMaterials[0], meshRenderer.sharedMaterials[1] });
                        Material[] mats = {
                            ((FurDescriptor)cosmeticDescriptor).bodyMat,
                            ((FurDescriptor)cosmeticDescriptor).chestMat,
                            meshRenderer.sharedMaterials[2]
                        };
                        meshRenderer.SetSharedMaterials(mats.ToList());
                        if (!activeCustomModels.ContainsKey(head))
                            activeCustomModels[head] = new List<GameObject> { newModel.gameObject };
                        else
                            activeCustomModels[head].Add(newModel.gameObject);
                        return;
                }
            }
            newModel.SetParent(head.transform, false);
            newModel.localPosition = positionOffset;
            newModel.localRotation = rotationOffset;
            if (!activeCustomModels.ContainsKey(head))
                activeCustomModels[head] = new List<GameObject> { newModel.gameObject };
            else
                activeCustomModels[head].Add(newModel.gameObject);
        }

        public static void ClearCustomModelsFromHead(this HeadModel head)
        {
            if (activeCustomModels.TryGetValue(head, out var customModel))
            {
                foreach (GameObject model in customModel)
                {
                    if (model.TryGetComponent(out Descriptor cosmeticDescriptor) && cosmeticDescriptor.IsSpecialCosmetic)
                    {
                        switch (cosmeticDescriptor.Category)
                        {
                            case CosmeticsController.CosmeticCategory.Fur:
                                MeshRenderer meshRenderer = head.gameObject.GetComponent<MeshRenderer>();
                                Material[] mats = {
                                    fallbackCustomMats[head][0],
                                    fallbackCustomMats[head][1],
                                    meshRenderer.sharedMaterials[2]
                                };
                                meshRenderer.SetSharedMaterials(mats.ToList());
                                break;
                        }
                    }
                    Object.Destroy(model);
                }
                activeCustomModels.Remove(head);
            }
        }

        private static Dictionary<HeadModel, List<GameObject>> activeCustomModels = new Dictionary<HeadModel, List<GameObject>>();

        private static Dictionary<HeadModel, Material[]> fallbackCustomMats = new Dictionary<HeadModel, Material[]>();
    }
}
#endif