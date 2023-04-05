namespace DebugMod
{
    public static class PlayerDeathWatcher
    {
        static PlayerDeathWatcher()
        {
            Modding.ModHooks.Instance.BeforePlayerDeadHook += SetPlayerDead;
        }

        public static bool playerDead;
        
        private static void SetPlayerDead()
        {
            playerDead = true;
            LogDeathDetails();
        }

        public static void Reset()
        {
            playerDead = false;
        }

        public static void LogDeathDetails()
        {
            Console.AddLine(string.Concat(new string[]
            {
                "Hero death detected. Game playtime: ",
                PlayerData.instance.playTime.ToString(),
                " Shade Zone: ",
                PlayerData.instance.shadeMapZone.ToString(),
                " Shade Geo: ",
                PlayerData.instance.geoPool.ToString(),
                " Respawn scene: ",
                PlayerData.instance.respawnScene.ToString()
            }));
        }
    }
}
