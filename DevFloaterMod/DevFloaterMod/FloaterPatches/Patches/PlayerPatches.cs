/* 
MIT License

Copyright (c) 2021 legoandmars

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using HarmonyLib;
using GorillaLocomotion;
using System.Collections;
using UnityEngine;

namespace DevFloaterMod.FloaterPatches.Patches
{
    [HarmonyPatch(typeof(Player), "Awake")]
    internal class InitializationPatch
    {
        internal static void Postfix(Player __instance)
        {
            __instance.StartCoroutine(Delay());
        }

        internal static IEnumerator Delay()
        {
            yield return 0;

            Plugin.Instance.OnInitialized();
        }
    }

    [HarmonyPatch(typeof(Player), "GetSlidePercentage")]
    internal class SlipPatch
    {
        internal static void Postfix(ref float __result, RaycastHit raycastHit)
        {
            if ((Plugin.Instance.underwater || Plugin.Instance.handUnderwater) && Plugin.Instance.InModdedRoom)
            {
                GorillaSurfaceOverride currentOverride = raycastHit.collider.gameObject.GetComponent<GorillaSurfaceOverride>();
                if (currentOverride != null)
                {
                    int currentMaterialIndex = currentOverride.overrideIndex;
                    if (!Player.Instance.materialData[currentMaterialIndex].overrideSlidePercent) __result = 0.55f;
                    else __result = Player.Instance.materialData[currentMaterialIndex].slidePercent == 0 ? 0.55f : Player.Instance.materialData[currentMaterialIndex].slidePercent;
                }
                else __result = 0.55f;
            }
        }
    }
}
