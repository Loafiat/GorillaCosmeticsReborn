#if Runtime
using System.Collections;
using UnityEngine;

namespace GorillaCosmeticsReborn.CosmeticSystem
{
    public class SwitchWardrobeButton : GorillaPressableButton
    {
        public override void Start()
        {
            onText = "DEFAULT";
            offText = "CUSTOM";
        }

        public override void ButtonActivationWithHand(bool isLeftHand)
        {
            CustomCosmeticController.ToggleModdedWardrobe();
            StartCoroutine(ButtonColorUpdate());
        }

        public IEnumerator ButtonColorUpdate()
        {
            isOn = true;
            UpdateColor();
            yield return new WaitForSeconds(debounceTime);
            isOn = false;
            UpdateColor();
        }
    }
}
#endif