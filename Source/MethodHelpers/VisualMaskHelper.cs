using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DebugMod.MethodHelpers
{
    public static class VisualMaskHelper
    {
        private static bool DeactivatingOnSceneChange = false;
        public static bool MasksDeactivated = false;

        public static void InvokedBindableFunction()
        {
            if (!MasksDeactivated)
            {
                DeactivateVisualMasks(GameObject.FindObjectsOfType<GameObject>());
                MasksDeactivated = true;
                Console.AddLine("Press again to deactivate masks each scene change");
            }
            else
            {
                DeactivatingOnSceneChange = !DeactivatingOnSceneChange;
                Console.AddLine((DeactivatingOnSceneChange ? "D" : "No longer d") + "eactivating visual masks each scene change");
            }
        }

        public static void OnSceneChange(Scene s)
        {
            if (DeactivatingOnSceneChange)
            {
                DelayInvoke(() => DeactivateVisualMasks(GetGameObjectsInScene(s)));
                MasksDeactivated = true;
            }
            else
            {
                if (MasksDeactivated)
                {
                    // We need to reactivate the renderers attached to the vignette
                    foreach (Renderer r in DebugMod.HC.vignette.GetComponentsInChildren<Renderer>(true))
                    {
                        r.enabled = true;
                    }
                }
                MasksDeactivated = false;
            }
        }

        public static void DelayInvoke(Action method)
        {
            IEnumerator coro()
            {
                // Delaying for 2f seems enough - wait for 3 just to be sure though.
                yield return null;
                yield return null;
                yield return null;
                method();
            }
            GameManager.instance.StartCoroutine(coro());
        }

        public static IEnumerable<GameObject> GetGameObjectsInScene(Scene s)
        {
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
                else if (go.LocateMyFSM("unmasker") is PlayMakerFSM)
                    disableMask(go);
                else if (go.LocateMyFSM("remasker_inverse") is PlayMakerFSM)
                    disableMask(go);
                else if (go.LocateMyFSM("remasker") is PlayMakerFSM)
                    disableMask(go);
            }

            Console.AddLine($"Deactivated {ctr} masks" + (HeroController.instance.vignette.enabled ? " and toggling vignette off" : string.Empty));

            // The vignette counts as a visual mask :)
            ProperlyDisableVignette();

            return ctr;
        }

        /// <summary>
        /// Disable the Vignette, as well as all of the renderers in its children
        /// </summary>
        public static void ProperlyDisableVignette()
        {
            // Not suitable for toggle vignette because not easily reversible
            DebugMod.HC.vignette.enabled = false;
            foreach (Renderer r in DebugMod.HC.vignette.GetComponentsInChildren<Renderer>())
            {
                r.enabled = false;
            }
        }
    }
}
