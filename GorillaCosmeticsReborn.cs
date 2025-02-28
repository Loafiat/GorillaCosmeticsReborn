#if Runtime
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BoingKit;
using GorillaCosmeticsReborn.CosmeticSystem;
using GorillaNetworking;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace GorillaCosmeticsReborn
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class GorillaCosmeticsReborn : BaseUnityPlugin
    {
        public void Awake()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), PluginInfo.GUID);
            GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);
        }

        void OnPlayerSpawned()
        {
            ThreadingHelper.Instance.StartAsyncInvoke(() => LoadCosmetics);
        }

        void LoadCosmetics()
        {
            string cosmeticsPath = Path.Join(Assembly.GetExecutingAssembly().Location.Replace("GorillaCosmeticsReborn.dll", ""), "Cosmetics");
            foreach (string bundle in Directory.GetFiles(cosmeticsPath))
            {
                try
                {
                    AssetBundle? assetBundle = null;
                    try
                    {
                        assetBundle = AssetBundle.LoadFromFile(bundle);
                    }
                    catch
                    {
                        Debug.LogError($"File at {bundle} is not a cosmetic!");
                        continue;
                    }
                    if (assetBundle == null)
                    {
                        Debug.LogError($"Null reference while loading cosmetic at {bundle}!");
                        continue;
                    }

                    Object[] assets = assetBundle.LoadAllAssets();
                    Descriptor newHat = ((GameObject)assets.First(
                            x => x is GameObject && ((GameObject)x).GetComponent<Descriptor>()))
                        .GetComponent<Descriptor>();
                    switch (newHat.Category)
                    {
                        case CosmeticsController.CosmeticCategory.Hat:
                            newHat.wardrobePositionOffset = new Vector3(((HatDescriptor)newHat).positionOffset.x, -((HatDescriptor)newHat).positionOffset.z, ((HatDescriptor)newHat).positionOffset.y + 0.15f);
                            newHat.wardrobeRotationOffset = ((HatDescriptor)newHat).rotationOffset;
                            break;
                        case CosmeticsController.CosmeticCategory.Face:
                            newHat.wardrobePositionOffset = new Vector3(((FaceDescriptor)newHat).positionOffset.x, -((FaceDescriptor)newHat).positionOffset.z, ((FaceDescriptor)newHat).positionOffset.y + 0.15f);
                            newHat.wardrobeRotationOffset = ((FaceDescriptor)newHat).rotationOffset;
                            break;
                        case CosmeticsController.CosmeticCategory.Badge:
                            newHat.wardrobePositionOffset = new Vector3(((BadgeDescriptor)newHat).positionOffset.x, -((BadgeDescriptor)newHat).positionOffset.z, ((BadgeDescriptor)newHat).positionOffset.y - 0.25f);
                            newHat.wardrobeRotationOffset = Quaternion.Euler(((BadgeDescriptor)newHat).rotationOffset.eulerAngles.x + 90, ((BadgeDescriptor)newHat).rotationOffset.eulerAngles.y, ((BadgeDescriptor)newHat).rotationOffset.z);
                            break;
                        case CosmeticsController.CosmeticCategory.Pants:
                            newHat.wardrobePositionOffset = new Vector3(((PantsDescriptor)newHat).positionOffset.x, -((PantsDescriptor)newHat).positionOffset.z, ((PantsDescriptor)newHat).positionOffset.y - 0.25f);
                            newHat.wardrobeRotationOffset = Quaternion.Euler(((PantsDescriptor)newHat).rotationOffset.eulerAngles.x + 90, ((PantsDescriptor)newHat).rotationOffset.eulerAngles.y, ((PantsDescriptor)newHat).rotationOffset.z);
                            break;
                        default:
                            newHat.wardrobePositionOffset = Vector3.zero;
                            newHat.wardrobeRotationOffset = Quaternion.identity;
                            break;
                    }
                    PruneCosmeticWithWhitelist(newHat);
                    CustomCosmeticController.CosmeticCategories[newHat.Category].Add(newHat);
                }
                catch {}
            }
        }

        // It's late and I have to make a beta release, if this causes issues then uh, sorry?
        void PruneCosmeticWithWhitelist(Descriptor cosmetic)
        {
            foreach (Component behaviour in cosmetic.gameObject.GetComponents<Component>())
            {
                if (!_componentWhitelist.Contains(behaviour.GetType()))
                {
                    Destroy(behaviour);
                }
                else
                {
                    if (behaviour.GetType() == typeof(SphereCollider) ||
                        behaviour.GetType() == typeof(CapsuleCollider) ||
                        behaviour.GetType() == typeof(BoxCollider) ||
                        behaviour.GetType() == typeof(MeshCollider))
                    {
                        behaviour.gameObject.layer = (int)UnityLayer.Prop;
                        ((Collider)behaviour).includeLayers = LayerMask.GetMask("Prop", "Gorilla Collider");
                        ((Collider)behaviour).excludeLayers = LayerMask.GetMask("Bake");
                    }
                }
            }
        }

        private readonly Type[] _componentWhitelist =
        {
            //Defualt Unity Components
            typeof(Transform),
            typeof(MeshFilter),
            typeof(MeshRenderer),
            typeof(RectTransform),
            typeof(TextMeshPro),
            typeof(SkinnedMeshRenderer),
            typeof(SpriteRenderer),
            typeof(BillboardRenderer),
            
            // Audio and visual
            typeof(ParticleSystemRenderer),
            typeof(ParticleSystem),
            typeof(AudioSource),
            typeof(VideoPlayer),
            
            // Animation components
            typeof(Animator),
            typeof(Avatar),
            
            // BoingKit components (Not sure which of these are actually required, but I don't think it'll harm anything to include these.)
            typeof(BoingBones),
            typeof(BoingEffector),
            typeof(BoingReactor),
            typeof(BoingBoneCollider),
            typeof(BoingReactorField),
            
            // Physics components, useful for interactive cosmetics.
            typeof(Rigidbody),
            typeof(SphereCollider),
            typeof(BoxCollider),
            typeof(CapsuleCollider),
            typeof(MeshCollider),
            
            // Joints for rigidbodies.
            typeof(SpringJoint),
            typeof(CharacterJoint),
            typeof(ConfigurableJoint),
            typeof(FixedJoint),
            typeof(HingeJoint),
            
            //Descriptors, these are required to fetch cosmetic data in game.
            typeof(BadgeDescriptor),
            typeof(FaceDescriptor),
            typeof(FurDescriptor),
            typeof(HatDescriptor),
            typeof(PantsDescriptor)
        };
    }
}
#endif