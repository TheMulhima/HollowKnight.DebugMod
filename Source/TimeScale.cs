using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace DebugMod
{
    public static class TimeScale
    {
         public static void EnableTimeScale()
        {
            DebugMod.TimeScaleActive = true;
            On.GameManager.SetTimeScale_float += GameManager_SetTimeScale_1;
            On.GameManager.FreezeMoment_float_float_float_float += GameManager_FreezeMoment_1;
            On.QuitToMenu.Start += Quit_To_Menu;
            
        }
        
         public static void DisableTimeScale()
        {
            Time.timeScale = 1f;
            DebugMod.CurrentTimeScale = 1f;
            DebugMod.TimeScaleActive = false;
            On.GameManager.SetTimeScale_float -= GameManager_SetTimeScale_1;
            On.GameManager.FreezeMoment_float_float_float_float -= GameManager_FreezeMoment_1;
            On.QuitToMenu.Start -= Quit_To_Menu;
        }
        private static IEnumerator Quit_To_Menu(On.QuitToMenu.orig_Start orig, QuitToMenu self)
        {
            yield return null;
            UIManager ui = UIManager.instance;
            bool flag = ui != null;
            if (flag)
            {
                UIManager.instance.AudioGoToGameplay(0f);
                UnityEngine.Object.Destroy(ui.gameObject);
            }
            HeroController heroController = HeroController.instance;
            bool flag2 = heroController != null;
            if (flag2)
            {
                UnityEngine.Object.Destroy(heroController.gameObject);
            }
            GameCameras gameCameras = GameCameras.instance;
            bool flag3 = gameCameras != null;
            if (flag3)
            {
                UnityEngine.Object.Destroy(gameCameras.gameObject);
            }
            GameManager gameManager = GameManager.instance;
            bool flag4 = gameManager != null;
            if (flag4)
            {
                try
                {
                    ObjectPool.RecycleAll();
                }
                catch (Exception ex)
                {
                    Exception exception = ex;
                    Debug.LogErrorFormat("Error while recycling all as part of quit, attempting to continue regardless.", new object[0]);
                    Debug.LogException(exception);
                }
                gameManager.playerData.Reset();
                gameManager.sceneData.Reset();
                UnityEngine.Object.Destroy(gameManager.gameObject);
            }
            Time.timeScale = DebugMod.CurrentTimeScale;
            yield return null;
            GCManager.Collect();
            UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Menu_Title", 0);
            DisableTimeScale();
        }
        private static IEnumerator GameManager_FreezeMoment_1(On.GameManager.orig_FreezeMoment_float_float_float_float orig, GameManager self, float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
        {
            FieldInfo timeSlowedField = typeof(GameManager).GetField("TimeSlowed", BindingFlags.Instance | BindingFlags.Public);
            bool flag = self.TimeSlowed;
            if (flag)
            {
                yield break;
            }
            timeSlowedField.SetValue(self,true);
            yield return self.StartCoroutine(SetTimeScale(targetSpeed, rampDownTime));
            for (float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime * DebugMod.CurrentTimeScale)
            {
                yield return null;
            }
            yield return self.StartCoroutine(SetTimeScale(1f, rampUpTime));
            timeSlowedField.SetValue(self,false);
            yield break;
        }

        private static IEnumerator SetTimeScale(float newTimeScale, float duration)
        {
            float lastTimeScale = Time.timeScale;
            for (float timer = 0f; timer < duration; timer += Time.unscaledDeltaTime)
            {
                float val = Mathf.Clamp01(timer / duration);
                SetTimeScale(Mathf.Lerp(lastTimeScale, newTimeScale, val));
                yield return null;
            }
            SetTimeScale(newTimeScale);
            yield break;
        }

        private static void SetTimeScale(float newTimeScale)
        {
            Time.timeScale = ((newTimeScale <= 0.01f) ? 0f : newTimeScale) * DebugMod.CurrentTimeScale;
        }

        private static void GameManager_SetTimeScale_1(On.GameManager.orig_SetTimeScale_float orig, GameManager self, float newTimeScale)
        {
            Time.timeScale = ((newTimeScale <= 0.01f) ? 0f : newTimeScale) * DebugMod.CurrentTimeScale;
        }
    }
}