using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace DebugMod.MethodHelpers
{
    public static class VisualMaskHelper
    {
        private static bool MasksDisabled = false;
        private static bool VignetteDisabled = false;

        public static void ToggleAllMasks()
        {
            MasksDisabled = !MasksDisabled;
            if (MasksDisabled)
            {
                DeactivateVisualMasks(Object.FindObjectsOfType<GameObject>());
                Console.AddLine("Disabled all visual masks");
            }
            else
            {
                Console.AddLine("No longer disabling all visual masks; reload the room to see changes");
                if (VignetteDisabled)
                {
                    Console.AddLine("Vignette was disabled; re-enabling");
                    VignetteDisabled = false;
                }
            }
        }

        public static void ToggleVignette()
        {
            VignetteDisabled = !VignetteDisabled;
            if (VignetteDisabled)
            {
                DisableVignette(false);
                Console.AddLine("Disabled vignette");
            }
            else
            {
                Console.AddLine("Enabled vignette");
                DebugMod.HC.vignette.enabled = true;
                if (MasksDisabled)
                {
                    Console.AddLine("All visual masks were disabled; re-enabling");
                    MasksDisabled = false;
                }
            }
        }

        public static void OnSceneChange(Scene s)
        {
            if (MasksDisabled)
            {
                // Delaying for 2f seems enough - wait for 3 just to be sure though.
                DelayInvoke(3, () => DeactivateVisualMasks(GetGameObjectsInScene(s)));
                return;
            }
            
            if (VignetteDisabled)
            {
                DelayInvoke(3, () => DisableVignette(false));
            }
        }

        public static void DelayInvoke(int delay, Action method)
        {
            IEnumerator coro()
            {
                for (int i = 0; i < delay; i++)
                {
                    yield return null;
                }
                method();
            }
            GameManager.instance.StartCoroutine(coro());
        }

        public static IEnumerable<GameObject> GetGameObjectsInScene(Scene s)
        {
            if (!s.IsValid())
            {
                yield break;
            }

            foreach (GameObject go in s.GetRootGameObjects())
            {
                yield return go;
                foreach (Transform t in go.GetComponentsInChildren<Transform>(true))
                {
                    yield return t.gameObject;
                }
            }
        }


        /// <summary>
        /// Deactivate all renderers attached to objects that DebugMod recongises as being a visual mask.
        /// </summary>
        /// <param name="gos">The game objects to iterate over.</param>
        /// <returns>The number of renderers disabled.</returns>
        public static int DeactivateVisualMasks(IEnumerable<GameObject> gos)
        {
            int ctr = 0;

            void disableMask(GameObject go)
            {
                foreach (Renderer r in go.GetComponentsInChildren<Renderer>())
                {
                    if (r.enabled)
                    {
                        ctr++;
                        r.enabled = false;
                    }
                }
            }

            float knightZ = HeroController.instance.transform.position.z;
            foreach (GameObject go in gos)
            {
                if (go.transform.position.z > knightZ) continue;

                // A collection of ways to identify masks. It's possible some slip through the cracks I guess
                if (go.name.StartsWith("msk_"))
                    disableMask(go);
                else if (go.name.StartsWith("Tut_msk"))
                    disableMask(go);
                else if (go.name.StartsWith("black_solid"))
                    disableMask(go);
                else if (go.name.ToLower().Contains("vignette"))
                    disableMask(go);
                else if (go.name.ToLower().Contains("secret mask"))
                    disableMask(go);
                else if (go.LocateMyFSM("unmasker") is PlayMakerFSM)
                    disableMask(go);
                else if (go.LocateMyFSM("remasker_inverse") is PlayMakerFSM)
                    disableMask(go);
                else if (go.LocateMyFSM("remasker") is PlayMakerFSM)
                    disableMask(go);
            }

            Console.AddLine($"Deactivated {ctr} masks" + (HeroController.instance.vignette.enabled ? " and toggling vignette off" : string.Empty));

            // The vignette counts as a visual mask :)
            DisableVignette(true);

            return ctr;
        }

        /// <summary>
        /// Disable the Vignette, as well as all renderers in its children.
        /// </summary>
        /// <param name="includeChildren">If this is false, do not disable renderer's in the vignette's children.</param>
        public static void DisableVignette(bool includeChildren = true)
        {
            DebugMod.HC.vignette.enabled = false;
            
            if (!includeChildren)
            {
                return;
            }
            
            // Not suitable for toggle vignette because not easily reversible
            foreach (Renderer r in DebugMod.HC.vignette.GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }
        }
    }
}
