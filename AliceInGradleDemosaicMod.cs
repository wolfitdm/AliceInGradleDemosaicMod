using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using m2d;
using nel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Web;
using UnityEngine;
using UnityEngine.NVIDIA;
using XX;

namespace AliceInGradleDemosaicMod
{
    [BepInPlugin("com.wolfitdm.AliceInGradleDemosaicMod", "AliceInGradleDemosaicMod Plugin", "1.0.0.0")]
    public class AliceInGradleDemosaicMod : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private static ConfigEntry<bool> configEnableMe;
        private static ConfigEntry<bool> configMenuDefaultOpen;
        private static ConfigEntry<KeyCode> configKeyCodeToOpenCheatMenu;

        public static bool showMenu = false;

        private Vector2 scrollPos;     // Scroll-Position
        private bool foldoutPlayer = false;
        private bool foldoutSuperNoel = false;
        private bool foldoutPervertFuncs = false;

        public static KeyCode keyCodeToOpenCloseTheCheatsMenu = 0;

        public static Rect windowRect = new Rect(20, 20, 400, 600);

        public AliceInGradleDemosaicMod()
        {
        }

        public static Type MyGetType(string originalClassName)
        { 
            return Type.GetType(originalClassName + ",Assembly-CSharp");
        }

        public static Type MyGetType(Type type, string originalClassName)
        {
            if (type == null)
            {
                return null;
            }

            Assembly ass = type.Assembly;

            if (ass == null)
            {
                return null;
            }

            Type[] typesx = ass.GetTypes();

            foreach (Type typex in typesx)
            {
                if (typex.Name == originalClassName)
                {
                    return typex;
                }
            }

            return null;
        }

        public static Type MyGetTypeUnityEngine(string originalClassName)
        {
            return Type.GetType(originalClassName + ",UnityEngine");
        }

        private static string pluginKey = "General.Toggles";

        public static bool enableMe = false;

        private static Type ftMosaicType = null;
        private static PropertyInfo ftMosaicTypeEnabled = null;


        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;

            configEnableMe = Config.Bind(pluginKey,
                                              "EnableMe",
                                              true,
                                             "Whether or not you want enable this mod (default true also yes, you want it, and false = no)");

            configMenuDefaultOpen = Config.Bind(pluginKey,
                      "IsMenuDefaultOpened",
                      true,
                     "Is menu default opened, default true");

            configKeyCodeToOpenCheatMenu = Config.Bind(pluginKey,
                                             "KeyCodeToOpenCloseTheCheatsMenu",
                                             KeyCode.R,
                                            "KeyCode to open/close the cheats menu, default R");

            enableMe = configEnableMe.Value;
            showMenu = configMenuDefaultOpen.Value;

            keyCodeToOpenCloseTheCheatsMenu = configKeyCodeToOpenCheatMenu.Value;

            PatchAllHarmonyMethods();

            Logger.LogInfo($"Plugin AliceInGradleDemosaicMod BepInEx is loaded!");
        }

        private void Update()
        {
            // Menü mit F1 ein-/ausblenden
            if (Input.GetKeyUp(keyCodeToOpenCloseTheCheatsMenu))
            {
                showMenu = !showMenu;
            }
        }

        private CursorLockMode defaultCursorLockMode;
        private bool cursorIsVisible = false;
        private bool cursorIsInit = false;
        private void OnGUI()
        {
            if (!cursorIsInit)
            {
                defaultCursorLockMode = Cursor.lockState;
                cursorIsVisible = Cursor.visible;
                cursorIsInit = true;
            }

            if (!enableMe)
            {
                Cursor.lockState = defaultCursorLockMode;
                Cursor.visible = cursorIsVisible;
                return;
            }

            if (!showMenu)
            {
                Cursor.lockState = defaultCursorLockMode;
                Cursor.visible = cursorIsVisible;
                return;
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Event e = Event.current;

            // Block Enter/Return key presses
            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                e.Use(); // Prevents the event from reaching the TextField
            }

            // Fenster zentriert anzeigen
            windowRect = GUI.Window(0, windowRect, DrawCheatWindow, "Cheat Menu");
        }
        private bool EditorLikeFoldout(bool foldout, string title)
        {
            GUILayout.BeginHorizontal();
            string arrow = foldout ? "▼" : "▶";
            if (GUILayout.Button(arrow + " " + title, GUI.skin.label))
            {
                foldout = !foldout;
            }
            GUILayout.EndHorizontal();
            return foldout;
        }


        public static bool UNCENSOR = true;

        public static bool DEBUG = true;

        public static bool DEBUGNOCFG = false;

        public static bool DEBUGSUPERSENSITIVE = false;

        public static bool DEBUGANNOUNCE = false;

        public static bool DEBUGNOSND = false;

        public static bool DEBUG_PLAYER = false;

        public static bool DEBUGALLSKILL = false;

        public static bool DEBUGNOEVENT = false;

        public static bool DEBUGNOVOICE = false;

        public static bool DEBUGRELOADMTR = false;

        public static bool DEBUGTIMESTAMP = false;

        public static bool DEBUGALBUMUNLOCK = false;

        public static bool DEBUGSTABILIZE_DRAW = false;

        public static bool DEBUGMIGHTY = false;

        public static bool DEBUGNODAMAGE = false;

        public static bool DEBUGWEAK = false;

        public static bool DEBUGBENCHMARK = false;

        public static bool DEBUGSUPERCYCLONE = false;

        public static bool ENG_MODE = false;

        public static bool DEBUGSPEFFECT = false;

        private static string st(string text)
        {
             bool set = false;
             try
             {
                switch(text)
                {
                    case "UNCENSOR":
                        {
                            set = UNCENSOR;
                        }
                        break;

                    case "DEBUG":
                        {
                            set = DEBUG;
                        }
                        break;

                    case "DEBUGNOCFG":
                        {
                            set = DEBUGNOCFG;
                        }
                        break;

                    case "DEBUGSUPERSENSITIVE":
                        {
                            set = DEBUGSUPERSENSITIVE;
                        }
                        break;

                    case "DEBUGANNOUNCE":
                        {
                            set = DEBUGANNOUNCE;
                        }
                        break;

                    case "DEBUGNOSND":
                        {
                            set = DEBUGNOSND;
                        }
                        break;

                    case "DEBUGPLAYER":
                        {
                            set = DEBUG_PLAYER;
                        }
                        break;

                    case "DEBUGALLSKILL":
                        {
                            set = DEBUGALLSKILL;
                        }
                        break;

                    case "DEBUGNOEVENT":
                        {
                            set = DEBUGNOEVENT;
                        }
                        break;

                    case "DEBUGNOVOICE":
                        {
                            set = DEBUGNOVOICE;
                        }
                        break;

                    case "DEBUGRELOADMTR":
                        {
                            set = DEBUGRELOADMTR;
                        }
                        break;

                    case "DEBUGTIMESTAMP":
                        {
                            set = DEBUGTIMESTAMP;
                        }
                        break;

                    case "DEBUGALBUMUNLOCK":
                        {
                            set = DEBUGALBUMUNLOCK;
                        }
                        break;

                    case "DEBUGSTABILIZE_DRAW":
                        {
                            set = DEBUGSTABILIZE_DRAW;
                        }
                        break;

                    case "DEBUGMIGHTY":
                        {
                            set = DEBUGMIGHTY;
                        }
                        break;

                    case "DEBUGNODAMAGE":
                        {
                            set = DEBUGNODAMAGE;
                        }
                        break;

                    case "DEBUGWEAK":
                        {
                            set = DEBUGWEAK;
                        }
                        break;

                    case "DEBUGBENCHMARK":
                        {
                            set = DEBUGBENCHMARK;
                        }
                        break;

                    case "DEBUGSUPERCYCLONE":
                        {
                            set = DEBUGSUPERCYCLONE;
                        }
                        break;

                    case "DEBUGENG_MODE":
                        {
                            set = ENG_MODE;
                        }
                        break;

                    case "DEBUGSPEFFECT":
                        {
                            set = DEBUGSPEFFECT;
                        }
                        break;
                }
             } catch(Exception ex)
             {
             }

            string ret = text.Replace("DEBUG","DEBUG: ") + ": " + (set ? "ON" : "OFF");

            return ret;
        }

        private static void updateVar(string text)
        {
            try
            {
                switch (text)
                {
                    case "UNCENSOR":
                        {
                            UNCENSOR = !UNCENSOR;
                        }
                        break;

                    case "DEBUG":
                        {
                            X.DEBUG = DEBUG = !DEBUG;
                        }
                        break;

                    case "DEBUGNOCFG":
                        {
                            X.DEBUGNOCFG = DEBUGNOCFG = !DEBUGNOCFG;
                        }
                        break;

                    case "DEBUGSUPERSENSITIVE":
                        {
                            X.DEBUGSUPERSENSITIVE = DEBUGSUPERSENSITIVE = !DEBUGSUPERSENSITIVE;
                        }
                        break;

                    case "DEBUGANNOUNCE":
                        {
                            X.DEBUGANNOUNCE = DEBUGANNOUNCE = !DEBUGANNOUNCE;
                        }
                        break;

                    case "DEBUGNOSND":
                        {
                            X.DEBUGNOSND = DEBUGNOSND = !DEBUGNOSND;
                        }
                        break;

                    case "DEBUGPLAYER":
                        {
                            X.DEBUG_PLAYER = DEBUG_PLAYER = !DEBUG_PLAYER;
                        }
                        break;

                    case "DEBUGALLSKILL":
                        {
                            X.DEBUGALLSKILL = DEBUGALLSKILL = !DEBUGALLSKILL;
                        }
                        break;

                    case "DEBUGNOEVENT":
                        {
                            X.DEBUGNOEVENT = DEBUGNOEVENT = !DEBUGNOEVENT;
                        }
                        break;

                    case "DEBUGNOVOICE":
                        {
                            X.DEBUGNOVOICE = DEBUGNOVOICE = !DEBUGNOVOICE;
                        }
                        break;

                    case "DEBUGRELOADMTR":
                        {
                            X.DEBUGRELOADMTR = DEBUGRELOADMTR = !DEBUGRELOADMTR;
                        }
                        break;

                    case "DEBUGTIMESTAMP":
                        {
                            X.DEBUGTIMESTAMP = DEBUGTIMESTAMP = !DEBUGTIMESTAMP;
                        }
                        break;

                    case "DEBUGALBUMUNLOCK":
                        {
                            X.DEBUGALBUMUNLOCK = DEBUGALBUMUNLOCK = !DEBUGALBUMUNLOCK;
                        }
                        break;

                    case "DEBUGSTABILIZE_DRAW":
                        {
                            X.DEBUGSTABILIZE_DRAW = DEBUGSTABILIZE_DRAW = !DEBUGSTABILIZE_DRAW;
                        }
                        break;

                    case "DEBUGMIGHTY":
                        {
                            X.DEBUGMIGHTY = DEBUGMIGHTY = !DEBUGMIGHTY;
                        }
                        break;

                    case "DEBUGNODAMAGE":
                        {
                            X.DEBUGNODAMAGE = DEBUGNODAMAGE = !DEBUGNODAMAGE;
                        }
                        break;

                    case "DEBUGWEAK":
                        {
                            X.DEBUGWEAK = DEBUGWEAK = !DEBUGWEAK;
                        }
                        break;

                    case "DEBUGBENCHMARK":
                        {
                            X.DEBUGBENCHMARK = DEBUGBENCHMARK = !DEBUGBENCHMARK;
                        }
                        break;

                    case "DEBUGSUPERCYCLONE":
                        {
                            X.DEBUGSUPERCYCLONE = DEBUGSUPERCYCLONE = !DEBUGSUPERCYCLONE;
                        }
                        break;

                    case "DEBUGENG_MODE":
                        {
                            X.ENG_MODE = ENG_MODE = !ENG_MODE;
                        }
                        break;

                    case "DEBUGSPEFFECT":
                        {
                            X.DEBUGSPEFFECT = DEBUGSPEFFECT = !DEBUGSPEFFECT;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public static class SetGameValues
        {
            private static NelM2DBase m2d;
            private static PRNoel noel;
            private static bool isInit = false;
            public static void init()
            {
                if (isInit)
                    return;

                if (!(M2DBase.Instance is NelM2DBase))
                    return;
                isInit = true;
                m2d = M2DBase.Instance as NelM2DBase;
                noel = m2d.getPrNoel();
            }

            public static PRNoel getNoel()
            {
                init();
                return noel; 
            }

            public static NelM2DBase getM2D()
            {
                init();
                return m2d;
            }

            public static void SetMoney(int value, uint max)
            {
                max = CoinStorage.getCount();
                int diff_money = (int)(value - max);
                if (diff_money >= 0)
                {
                    CoinStorage.addCount(diff_money);
                }
                else
                {
                    CoinStorage.reduceCount(-diff_money);
                }
            }
            public static void SetDangerLevel(int danger_level, int grade)
            {
                if (!isInit)
                {
                    return;
                }
                NightController nc = m2d.NightCon;
                if (nc.M2D.isSafeArea()) { return; }
                int dlevel = Traverse.Create(nc).Field("dlevel").GetValue<int>();
                int dlevel_add = Traverse.Create(nc).Field("dlevel_add").GetValue<int>();
                int new_dlevel = danger_level;
                int diff_level = new_dlevel - dlevel - dlevel_add;
                if (diff_level == 0) { return; }
                if (diff_level > 0)
                {
                    Traverse.Create(nc).Field("dlevel").SetValue(new_dlevel);
                }
                else if (diff_level < 0)
                {
                    // Reduce additional danger level first
                    if (new_dlevel >= dlevel)
                    {
                        m2d.NightCon.addAdditionalDangerLevel(ref diff_level, grade, false);
                    }
                    else
                    {
                        int sub = -dlevel_add;
                        m2d.NightCon.addAdditionalDangerLevel(ref sub, grade, false);
                        Traverse.Create(nc).Field("dlevel").SetValue(new_dlevel);
                    }
                }
                nc.showNightLevelAdditionUI();
            }
            public static void SetWeather(string wea)
            {
                if (!isInit)
                {
                    return;
                }
                NightController nc = m2d.NightCon;
                string weather_string = wea;
                WeatherItem.WEATHER[] available_weather = (WeatherItem.WEATHER[])Enum.GetValues(typeof(WeatherItem.WEATHER));
                List<WeatherItem.WEATHER> add_weather_list = new();
                for (int i = 0; i < weather_string.Length && i < available_weather.Length - 1; i++)
                {
                    if (int.TryParse(weather_string[i].ToString(), out int num))
                    {
                        for (int j = 0; j < num; j++)
                        {
                            add_weather_list.Add(available_weather[i]);
                        }
                    }
                }
                // destruction of old weathers
                WeatherItem[] AWeather = Traverse.Create(nc).Field("AWeather").GetValue<WeatherItem[]>();
                foreach (WeatherItem weather in AWeather)
                {
                    weather.destruct();
                }
                // add new weathers
                List<WeatherItem> AList = new();
                foreach (WeatherItem.WEATHER i in add_weather_list)
                {
                    if (i == WeatherItem.WEATHER.MIST && add_weather_list.Contains(WeatherItem.WEATHER.MIST_DENSE))
                    {
                        continue;
                    }
                    if (i == WeatherItem.WEATHER.NORMAL && add_weather_list.Count > 1)
                    {
                        continue;
                    }
                    AList.Add(new WeatherItem(i, Traverse.Create(nc).Field("dlevel").GetValue<int>()));
                }
                int cur_weather = Traverse.Create(nc).Field("cur_weather_").GetValue<int>();
                for (int j = AList.Count - 1; j >= 0; j--)
                {
                    cur_weather |= 1 << (int)AList[j].weather;
                }
                Traverse.Create(nc).Field("cur_weather_").SetValue(cur_weather);
                Traverse.Create(nc).Field("AWeather").SetValue(AList.ToArray());
            }
            public static void ResetHExp()
            {
                if (!isInit)
                {
                    return;
                }
                noel.Ser.clear();
                noel.EpCon.newGame();
                noel.EggCon.clear();
                noel.GaugeBrk.reset();
                UIStatus.Instance.fineMpRatio(true, false);
                UIStatus.Instance.quitCrack();
                noel.recheck_emot = true;
            }
            public static void resetMpBrk()
            {
                if (!isInit)
                {
                    return;
                }
                noel.GaugeBrk.reset();
                UIStatus.Instance.quitCrack();
            }
            public static void PlantEggs()
            {
                if (!isInit)
                {
                    return;
                }
                PrEggManager EggCon = noel.EggCon;
                int type_num = Enum.GetNames(typeof(PrEggManager.CATEG)).Length;
                int val = (int)(noel.get_maxmp() / type_num / 2) * 2;
                Traverse.Create(noel).Field("mp").SetValue((int)(noel.get_maxmp() - (val * (type_num - 1))));
                int len = Traverse.Create(EggCon).Field("LEN").GetValue<int>();
                bool any_flag = len > 0;
                foreach (PrEggManager.CATEG categ in Enum.GetValues(typeof(PrEggManager.CATEG)))
                {
                    if (categ == PrEggManager.CATEG._ALL)
                    {
                        continue;
                    }
                    PrEggManager.PrEggItem prEggItem = EggCon.Get(categ);
                    if (prEggItem == null)
                    {
                        PrEggManager.PrEggItem[] aitm = Traverse.Create(EggCon).Field("AItm").GetValue<PrEggManager.PrEggItem[]>();
                        prEggItem = aitm[len] = new PrEggManager.PrEggItem(EggCon, categ);
                        len += 1;
                    }
                    prEggItem.val = val;
                    prEggItem.add_cushion = 0;
                    prEggItem.t_cushion = 0;
                    prEggItem.val_absorbed = 0;
                    prEggItem.t_effect_egg = -1;
                    prEggItem.effect_egg_count = 0;
                    prEggItem.effect_egg_count_layed = 0;
                }
                Traverse.Create(EggCon).Field("LEN").SetValue(len);
                Traverse.Create(EggCon).Field("total_").SetValue(-1);
                Traverse.Create(EggCon).Field("status_reduce_max_").SetValue(-1);
                if (!any_flag)
                {
                    EggCon.need_fine_mp = true;
                    if (!Traverse.Create(EggCon).Field("lock_fine").GetValue<bool>() && UIStatus.PrIs(EggCon.Pr))
                    {
                        UIStatus.Instance.finePlantedEgg();
                    }
                    noel.NM2D.IMNG.fineSpecialNoelRow(EggCon.Pr);
                }
                if (noel.UP != null && noel.UP.isActive())
                {
                    UIStatus.Instance.fineMpRatio(true, false);
                }
                noel.recheck_emot = true;
            }
        }

        public static void PatchHarmonyMethodsUnityPrefix(Type[] types, string original, string patched)
        {
            foreach (Type type in types)
            {
                try
                {
                    PatchHarmonyMethodUnity(type, original, patched, true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
            }
        }
        public static class SuperNoel
        {
            public static bool Invincible = false;
            public static bool DamageMultiplier = false;
            public static bool InfinteBomb = false;
            public static bool InfiniteJump = false;
            public static bool DurableShield = false;
            public static bool DisableGasDamage = false;
            public static bool ImmuneToMapThorn = false;
            public static bool ImmuneToLava = false;

            public static bool NoPervertDisableGrabAttack = false;
            public static bool NoPervertDisableEpDamage = false;
            public static bool NoPervertSkipGameOverPlay = false;
            public static bool NoPervertDisableWormTrap = false;
            public static bool NoPervertDisableHpAndAbsorbDamage = false;

            public static float hp_dmg_def = 0;
            public static float mp_dmg_def = 0;
            public static void ExecuteHarmonyPatches()
            {
                try
                {
                    PatchHarmonyMethodUnity(typeof(M2PrSkill), "AtkMul", "SuperNoelDamageMultiplier", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(PR), "applyDamage", "SuperNoelDamageMultiplier", true, false, new Type[] { typeof(NelAttackInfo), typeof(bool) });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(PR), "runPhysics", "SuperNoelInfiniteJump", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(M2PrSkill), "explodeMagic", "SuperNoelInfiniteBomb", false, true);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(M2Shield), "run", "DuralableShield", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {

                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(M2Shield), "checkShield", "DuralableShield2", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(PR), "canApplyGasDamage", "SuperNoelDisableGasDamage", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(PR), "applyDamageFromMap", "SuperNoelImmuneToMapThorn", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(PR), "checkLavaExecute", "SuperNoelImmuneToLava", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(NelNGolem), "readTicketOd", "SuperNoelNoPervertDisableGrabAttack", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(PR), "initAbsorb", "SuperNoelNoPervertDisableGrabAttack2", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(EpManager), "applyEpDamage", "SuperNoelNoPervertDisableEpDamage", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(UiGO), "runGiveup", "SuperNoelNoPervertSkipGameOverPlay", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(PR), "canPullByWorm", "SuperNoelNoPervertDisableWormTrap", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnity(typeof(NelChipWormHead), "initAction", "SuperNoelNoPervertDisableWormTrap2", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                PatchHarmonyMethodsUnityPrefix([typeof(NelNFox), typeof(NelNGolem), typeof(NelNMush),
                    typeof(NelNPuppy), typeof(NelNSlime),typeof(NelNSnake), typeof(NelNUni)], "runAbsorb", "SuperNoelDisableHpAndAbsorbDamage");

                PatchHarmonyMethodsUnityPrefix([typeof(NelNFox), typeof(NelNGolem), typeof(NelNMush),
                    typeof(NelNPuppy), typeof(NelNSlime),typeof(NelNSnake), typeof(NelNUni)], "initAbsorb", "SuperNoelDisableHpAndAbsorbDamage");

                try
                {
                    PatchHarmonyMethodUnity(typeof(NelNSlime), "runAbsorbOverDrive", "SuperNoelDisableHpAndAbsorbDamage", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
            }
            private static bool SuperNoelDamageMultiplier(ref float hpdmg, ref float mpdmg)
            {
                if (!DamageMultiplier)
                {
                    return true;
                }

                hpdmg *= hp_dmg_def;
                mpdmg *= mp_dmg_def;
                return true;
            }
           
            private static bool SuperNoelInvincible(ref int __result)
            {
                if (Invincible)
                {
                    __result = 0;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            private static bool SuperNoelInfiniteJump(object __instance)
            {
                if (!InfiniteJump)
                {
                    return true; 
                }

                PR noel = (PR)__instance;
                float jump_pushing = Traverse.Create(noel).Field("jump_pushing").GetValue<float>();
                if (jump_pushing >= -1 && jump_pushing < (20000 - noel.TS) && noel.isJumpO(0))
                {
                    if (jump_pushing < 10)
                    {
                        jump_pushing = 20000;
                    }
                    else
                    {
                        jump_pushing += noel.TS;
                    }
                    Traverse.Create(noel).Field("jump_pushing").SetValue(jump_pushing);
                }

                return true;
            }
            private static void SuperNoelInfiniteBomb(ref object __instance)
            {
                M2PrSkill _this = (M2PrSkill)__instance;
                if (!InfinteBomb) { return; }
                MGContainer mgc = _this.NM2D.MGC;
                MagicItem cur_mg = Traverse.Create(_this).Field("CurMg").GetValue<MagicItem>();
                int bomb_count = 0;
                for (int i = mgc.Length - 1; i >= 0; i--)
                {
                    MagicItem mg = mgc.getMg(i);
                    if (mg != cur_mg && mg.isActive(_this.Pr, MGKIND.DROPBOMB) && ++bomb_count > 4 && mg.phase == 9)
                    {
                        mg.phase = 3;
                    }
                }
            }
            private static bool SuperNoelDuralableShield(ref float power_progress_level)
            {
                if (!DurableShield) { return true; }
                    
                power_progress_level = 0;

                return true;
            }
                
            private static bool SuperNoelDuralableShield2(ref float val)
            {
                if (!DurableShield) { return true; }
                val = 0;
                return true;
            }
            private static bool SuperNoelDisableGasDamage(ref bool __result)
            {
                if (DisableGasDamage)
                {
                    __result = false;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            private static bool SuperNoelImmuneToMapThorn(ref AttackInfo __result)
            {
                 if (ImmuneToMapThorn)
                 {
                     __result = null;
                     return false;
                 }
                 else
                 {
                     return true;
                 }
            }
            private static bool SuperNoelImmuneToLava()
            {
                 if (ImmuneToLava)
                 {
                     return false;
                 }
                 else
                 {
                     return true;
                 }
            }
            private static bool SuperNoelNoPervertDisableGrabAttack(ref NaTicket Tk)
            {
                if (NoPervertDisableGrabAttack && Tk.type == NAI.TYPE.PUNCH_1)
                {
                    Tk.type = NAI.TYPE.PUNCH;
                }
                return true;
            }
            private static bool SuperNoelNoPervertDisableGrabAttack2(ref bool __result, ref AbsorbManager Abm)
            {
                if (NoPervertDisableGrabAttack)
                {
                    __result = false;
                    Abm?.destruct();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            private static bool SuperNoelNoPervertDisableEpDamage(ref bool __result)
            {
                if (NoPervertDisableEpDamage)
                {
                    __result = false;
                    return false;
                }
                else
                {
                    return true;
                }
            }

            private static bool SuperNoelNoPervertSkipGameOverPlay(ref UiGO __instance)
            {
                if (NoPervertSkipGameOverPlay)
                {
                    Traverse.Create(__instance).Field("t").SetValue(90);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            private static bool SuperNoelNoPervertDisableWormTrap(ref bool __result)
            {
                if (NoPervertDisableWormTrap)
                {
                    __result = false;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            private static bool SuperNoelNoPervertDisableWormTrap2()
            {
                if (NoPervertDisableWormTrap)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            public static bool SuperNoelNoPervertDisableHpAndAbsorbDamage(ref bool __result)
            {
                if (NoPervertDisableHpAndAbsorbDamage)
                {
                    __result = false;
                    return false;
                }
                else
                {
                    return true;
                }
            }
           
        }
        public static class NoelPervert
        {
            public static bool PervertEpItemEffect = false;
            public static bool PervertEPDamageMultiplier = false;
            public static bool PervertEnableMultipleOrgasmForAll = false;
            public static bool PervertEasierOrgasmWithHighEP = false;
            public static bool PervertSadismMode = false;
            public static bool PervertMasochismMode = false;
            public static bool PervertEroBow = false;

            public static float EPDamageMultiplier = 0;
            public static void ExecuteHarmonyPatches()
            {
                try
                {
                    PatchHarmonyMethodUnity(typeof(NelItem), "Use", "NoelPervertEpItemEffect", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(EpManager), "calcNormalEpErection", "NoelPervertEPDamageMultiplier", false, true);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(EpManager), "checkMultipleOrgasm", "NoelPervertEnableMultipleOrgasmForAll", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(EpManager), "getLeadToOrgasmRatio", "NoelPervertEnableMultipleOrgasmForAll", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnity(typeof(NelEnemy), "applyDamage", "NoelPervertSadismMode", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnity(typeof(PR), "applyHpDamageSimple", "NoelPervertMasochismMode", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnity(typeof(EpManager), "addMasturbateCountImmediate", "NoelPervertRemoveRecord", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnity(typeof(NelNGolemToyBow), "decideAttr", "NoelPervertEroBow", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
            }
            private static bool NoelPervertEpItemEffect(ref NelItem __instance, ref PR Pr)
            {
                if (PervertEpItemEffect)
                {
                    if (__instance.key == "fruit_epdmg_apple0" && Pr.ep >= 700)
                    {
                        Pr.Ser.Add(SER.FRUSTRATED);
                    }
                }
                return true;
            }
            private static void NoelPervertEPDamageMultiplier(EPCATEG target, float tval, ref float __result)
            {
                if (!PervertEPDamageMultiplier)
                {
                    return;
                }
                __result *= EPDamageMultiplier;
            }
            private static bool NoelPervertEnableMultipleOrgasmForAll(EPCATEG target, ref EpManager __instance, ref bool __result, ref EpAtk Atk)
            {
                if (PervertEnableMultipleOrgasmForAll)
                {
                    float set_mo = 0.4f;
                    int cur_mo = Traverse.Create(__instance).Field("multiple_orgasm").GetValue<int>();
                    if (Atk.situation_key != "masturbate" && Atk.multiple_orgasm < set_mo)
                    {
                        __result = X.XORSP() < set_mo * X.Pow(1f - X.ZLINE(cur_mo - 1, 9f) * 0.5f
                            - X.ZLINE(cur_mo - 10, 40f) * 0.48f, 2);
                    }
                    return false;
                }
                return true;
            }
            private static bool NoelPervertEasierOrgasmWithHighEP(ref float __result)
            {
                if (PervertEasierOrgasmWithHighEP)
                {
                    __result = 1;
                    return false;
                }
                else
                {
                    return true;
                }
            }
            private static bool NoelPervertSadismMode(ref NelAttackInfo Atk)
            {
                PRNoel noel = SetGameValues.getNoel();

                if (noel == null)
                {
                    return true;
                }

                if (PervertSadismMode && Atk != null && Atk.AttackFrom is PR)
                {
                    int value = (int)(Atk.hpdmg0 * 1.5f);
                    value = value > 0 ? value : 1;
                    noel.EpCon.applyEpDamage(new EpAtk(value, "sadism"), noel, EPCATEG_BITS.OTHER);
                }
                return true;
            }

            private static bool NoelPervertMasochismMode(ref NelAttackInfoBase Atk)
            {
                //if (Atk == null || Atk.AttackFrom is PR)
                if (Atk == null)
                {
                    return true;
                }
                if (PervertMasochismMode)
                {
                    if (Atk.EpDmg == null)
                    {
                        int value = (int)(Atk.hpdmg0 * 1.5f);
                        value = value > 0 ? value : 1;
                        Atk.EpDmg = new EpAtk(value, "masochism");
                    }
                }
                else if (Atk.EpDmg != null && Atk.EpDmg.situation_key == "masochism")
                {
                    Atk.EpDmg = null;
                }
                return true;
            }
            private static bool NoelPervertRemoveRecord(ref EpManager __instance, ref EPCATEG __result, ref EpAtk Atk, ref int multiple_orgasm)
            {
                if (Atk.situation_key == "sadism" || Atk.situation_key == "masochism")
                {
                    __result = EPCATEG.OTHER;
                    Traverse lmc = Traverse.Create(__instance).Field("last_ex_multi_count_temp");
                    if (lmc.GetValue<int>() < multiple_orgasm)
                    {
                        lmc.SetValue(multiple_orgasm);
                    }
                    return false;
                }
                return true;
            }

            private static bool NoelPervertEroBow(ref NelNGolemToyBow __instance)
            {
                if (PervertEroBow)
                {
                    MagicItem Mg = Traverse.Create(__instance).Field("Mg").GetValue<MagicItem>();
                    if (Mg != null)
                    {
                        Mg.Atk0 = Traverse.Create(__instance).Field("AtkAcme").GetValue<NelAttackInfo>();
                        Mg.Ray.HitLock(13f, null);
                        Traverse.Create(__instance).Field("Cattr").SetValue(new UnityEngine.Color32(243, 91, 169, 255));
                        Traverse.Create(__instance).Field("Cattr2").SetValue(new UnityEngine.Color(179, 10, 145, 170));
                    }
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        public static float slider(string var, float x, float min, float max)
        {
            GUILayout.Space(5);
            GUILayout.Label($"{var}: / Current {var}: {x}");
            x = (int)GUILayout.HorizontalSlider((float)x, (float)min, (float)max);
            GUILayout.Space(5);
            return x;
        }

        public static void toggleButton(string var, bool set, Action action)
        {
            if (GUILayout.Button(var + ": " + (set ? "ON" : "OFF"))) 
            {
                action.Invoke();
            }
        }

        public static void actionButton(string var, Action action)
        {
            if (GUILayout.Button(var))
            {
                action.Invoke();
            }
        }

        bool foldoutOtherSetMoney = false;
        bool foldoutSetDanger = false;
        bool foldoutSetWeather = false;
        bool foldoutOther = false;
        string weatherText = "";
        int money = 0;
        int dangerLevel = 0;
        int grade = 0;

        private void DrawCheatWindow(int windowID)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            // Spieler-Cheats
            foldoutPlayer = EditorLikeFoldout(foldoutPlayer, "Uncensor + Debug + Money Cheat");
            
            if (foldoutPlayer)
            {
                string[] vars = ["UNCENSOR", "DEBUG", "DEBUGNOCFG", "DEBUGSUPERSENSITIVE", "DEBUGANNOUNCE", "DEBUGNOSND", "DEBUGPLAYER", "DEBUGALLSKILL", 
                                 "DEBUGNOEVENT", "DEBUGNOVOICE", "DEBUGRELOADMTR", "DEBUGTIMESTAMP", "DEBUGALBUMUNLOCK", "DEBUGSTABILIZE_DRAW",
                                 "DEBUGMIGHTY", "DEBUGNODAMAGE", "DEBUGWEAK", "DEBUGBENCHMARK", "DEBUGSUPERCYCLONE", "DEBUGENG_MODE", "DEBUGSPEFFECT"
                                ];

                foreach (string var in vars)
                {
                    if (GUILayout.Button(st(var)))
                    {
                        updateVar(var);
                    }
                }

                if (GUILayout.Button("+1000 Money"))
                {
                    try
                    {
                        CoinStorage.addCount(1000, true);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            foldoutSuperNoel = EditorLikeFoldout(foldoutSuperNoel, "Super Noel");

            if (foldoutSuperNoel)
            {
                toggleButton("Invincible", SuperNoel.Invincible, () =>
                {
                    SuperNoel.Invincible = !SuperNoel.Invincible;
                });
                
                toggleButton("DamageMultiplier", SuperNoel.DamageMultiplier, () =>
                {
                    SuperNoel.DamageMultiplier = !SuperNoel.DamageMultiplier;
                });

                if (SuperNoel.DamageMultiplier)
                {
                    SuperNoel.hp_dmg_def = slider("DamageMultiplierHP", SuperNoel.hp_dmg_def, 0, 1000);
                    SuperNoel.hp_dmg_def = slider("DamageMultiplierMP", SuperNoel.hp_dmg_def, 0, 1000);
                }

                toggleButton("InfinteBomb", SuperNoel.InfinteBomb, () =>
                {
                    SuperNoel.InfinteBomb = !SuperNoel.InfinteBomb;
                });
                
                toggleButton("InfiniteJump", SuperNoel.InfiniteJump, () =>
                {
                    SuperNoel.InfiniteJump = !SuperNoel.InfiniteJump;
                });
                
                toggleButton("DurableShield", SuperNoel.DurableShield, () =>
                {
                    SuperNoel.DurableShield = !SuperNoel.DurableShield;
                });
                
                toggleButton("DisableGasDamage", SuperNoel.DisableGasDamage, () =>
                {
                    SuperNoel.DisableGasDamage = !SuperNoel.DisableGasDamage;
                });

                toggleButton("ImmuneToMapThorn", SuperNoel.ImmuneToMapThorn, () =>
                {
                    SuperNoel.ImmuneToMapThorn = !SuperNoel.ImmuneToMapThorn;
                });
                
                toggleButton("ImmuneToLava", SuperNoel.ImmuneToLava, () =>
                {
                    SuperNoel.ImmuneToLava = !SuperNoel.ImmuneToLava;
                });

                toggleButton("NoPervertDisableGrabAttack", SuperNoel.NoPervertDisableGrabAttack, () =>
                {
                    SuperNoel.NoPervertDisableGrabAttack = !SuperNoel.NoPervertDisableGrabAttack;
                });

                toggleButton("NoPervertDisableEpDamage", SuperNoel.NoPervertDisableEpDamage, () =>
                {
                    SuperNoel.NoPervertDisableEpDamage = !SuperNoel.NoPervertDisableEpDamage;
                });

                toggleButton("NoPervertSkipGameOverPlay", SuperNoel.NoPervertSkipGameOverPlay, () =>
                {
                    SuperNoel.NoPervertSkipGameOverPlay = !SuperNoel.NoPervertSkipGameOverPlay;
                });

                toggleButton("NoPervertDisableWormTrap", SuperNoel.NoPervertDisableWormTrap, () =>
                {
                    SuperNoel.NoPervertDisableWormTrap = !SuperNoel.NoPervertDisableWormTrap;
                });

                toggleButton("NoPervertDisableHpAndAbsorbDamage", SuperNoel.NoPervertDisableHpAndAbsorbDamage, () =>
                {
                    SuperNoel.NoPervertDisableHpAndAbsorbDamage = !SuperNoel.NoPervertDisableHpAndAbsorbDamage;
                });
            }

            foldoutPervertFuncs = EditorLikeFoldout(foldoutPervertFuncs, "Noel Pervert");

            if (foldoutPervertFuncs)
            {
                toggleButton("PervertEpItemEffect", NoelPervert.PervertEpItemEffect, () =>
                {
                    NoelPervert.PervertEpItemEffect = !NoelPervert.PervertEpItemEffect;
                });
                toggleButton("PervertEnableMultipleOrgasmForAll", NoelPervert.PervertEnableMultipleOrgasmForAll, () =>
                {
                    NoelPervert.PervertEnableMultipleOrgasmForAll = !NoelPervert.PervertEnableMultipleOrgasmForAll;
                });
                toggleButton("PervertEasierOrgasmWithHighEP", NoelPervert.PervertEasierOrgasmWithHighEP, () =>
                {
                    NoelPervert.PervertEasierOrgasmWithHighEP = !NoelPervert.PervertEasierOrgasmWithHighEP;
                });
                toggleButton("PervertSadismMode", NoelPervert.PervertSadismMode, () =>
                {
                    NoelPervert.PervertSadismMode = !NoelPervert.PervertSadismMode;
                });
                toggleButton("PervertMasochismMode", NoelPervert.PervertMasochismMode, () =>
                {
                    NoelPervert.PervertMasochismMode = !NoelPervert.PervertMasochismMode;
                });
                toggleButton("PervertEPDamageMultiplier", NoelPervert.PervertEPDamageMultiplier, () =>
                {
                    NoelPervert.PervertEPDamageMultiplier = !NoelPervert.PervertEPDamageMultiplier;
                });
                if (NoelPervert.PervertEPDamageMultiplier)
                {
                    NoelPervert.EPDamageMultiplier = slider("EPDamageMultiplier", NoelPervert.EPDamageMultiplier, 0, 1000000);
                }
            }

            foldoutOtherSetMoney = EditorLikeFoldout(foldoutOtherSetMoney, "Set Money");

            if (foldoutOtherSetMoney)
            {
                money = (int)slider("money", money, 0, 999999);
                actionButton($"Set Money {money}", () =>
                {
                    SetGameValues.SetMoney(money, 999999);
                });
            }

            foldoutSetDanger = EditorLikeFoldout(foldoutSetDanger, "Set Danger Level");

            if (foldoutSetDanger)
            {
                dangerLevel = (int)slider("DangerLevel", dangerLevel, 0, 50);
                grade = (int)slider("DangerLevelGrade", grade, 0, 1000);
                actionButton($"Set Danger Level {dangerLevel} {grade}", () =>
                {
                    SetGameValues.SetDangerLevel(dangerLevel, grade);
                });
            }

            foldoutSetWeather = EditorLikeFoldout(foldoutSetWeather, "Set Weather");
            if (foldoutSetWeather)
            {
                weatherText = GUILayout.TextField(weatherText, 100, GUILayout.Width(100));
                actionButton($"Set Weather {weatherText}", () =>
                {
                    SetGameValues.SetWeather(weatherText);
                });
            }

            foldoutOther = EditorLikeFoldout(foldoutOther, "Misc");

            if (foldoutOther)
            {
                actionButton($"Reset H Exp", () =>
                {
                    SetGameValues.ResetHExp();
                });

                actionButton($"Reset MP Brk", () =>
                {
                    SetGameValues.resetMpBrk();
                });

                actionButton($"Plant Eggs", () =>
                {
                    SetGameValues.PlantEggs();
                });
            }
            // Fenster verschiebbar machen
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }
        public static void PatchAllHarmonyMethods()
        {
            if (!enableMe)
            {
                return;
            }

            Type ftMosaic = MyGetType(typeof(MosaicShower), "FtMosaic");

            if (ftMosaic != null) {
                ftMosaicType = ftMosaic;
                try
                {
                    ftMosaicTypeEnabled = ftMosaicType.GetProperty("enabled", BindingFlags.Public | BindingFlags.Instance);
                }
                catch (Exception e)
                {
                    ftMosaicTypeEnabled = null;
                }
            }

            Type mosaicShower = typeof(MosaicShower);

            Type[] types = new Type[] { ftMosaic, mosaicShower };

            foreach (Type type in types) {
                
                if (type == null)
                {
                    continue;
                }

                try
                {
                    PatchHarmonyMethodUnity(type, "drawToMesh", "drawToMesh", true, false, new Type[] {typeof(Camera)});
                } catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnity(type, "FnDrawMosaic", "FnDrawMosaic", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnity(type, "setTarget", "setTarget", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
            }

            if (ftMosaic != null)
            {
                try
                {
                    PatchHarmonyMethodUnity(ftMosaic, "drawToMesh", "drawToMeshEx", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnity(ftMosaic, "countMosaic", "countMosaic", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
            }
        }

        public static bool setUseMosaicToFalse(object __instance, bool set = false)
        {
            Type instanceType = __instance.GetType();
            Type thisType = null;
            
            if (ftMosaicType != null && instanceType == ftMosaicType)
            {
                thisType = ftMosaicType;
            } else if (instanceType == typeof(MosaicShower))
            {
                thisType = typeof(MosaicShower);
            } else
            {
                thisType = instanceType;
                return false;
            }
                
            try
            {
                FieldInfo field = thisType.GetField("use_mosaic", BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(__instance, set);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
            return false;
        }

        public static bool setEnabledToFalse(object __instance, bool set = false)
        {
            Type instanceType = __instance.GetType();
            Type thisType = null;

            if (ftMosaicType != null && instanceType == ftMosaicType)
            {
                thisType = ftMosaicType;
            }
            else if (instanceType == typeof(MosaicShower))
            {
                thisType = typeof(MosaicShower);
                return false; 
            }
            else
            {
                thisType = instanceType;
                return false;
            }

            if (ftMosaicTypeEnabled == null)
            {
                return false; 
            }

            try
            {
                ftMosaicTypeEnabled.SetValue(thisType, set);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
            return false;
        }

        public static bool drawToMeshEx(object __instance)
        {
            if (!UNCENSOR)
            {
                setEnabledToFalse(true);
                setUseMosaicToFalse(true);
                return true;
            }
            setUseMosaicToFalse(__instance);
            setEnabledToFalse(__instance);
 
            return false;
        }

        public static bool drawToMesh(object __instance, Camera Cam)
        {
            if (!UNCENSOR)
            {
                setEnabledToFalse(true);
                setUseMosaicToFalse(true);
                return true;
            }
            setUseMosaicToFalse(__instance);
            setEnabledToFalse(__instance);
            return false;
        }
        public static bool FnDrawMosaic(object __instance, object XCon, ProjectionContainer JCon, Camera Cam)
        {
            if (!UNCENSOR)
            {
                setEnabledToFalse(true);
                setUseMosaicToFalse(true);
                return true;
            }
            setUseMosaicToFalse(__instance);
            setEnabledToFalse(__instance);
            return false; 
        }
        public static bool setTarget(object __instance, IMosaicDescriptor _Targ, bool force)
        {
            if (!UNCENSOR)
            {
                setEnabledToFalse(true);
                setUseMosaicToFalse(true);
                return true;
            }
            setUseMosaicToFalse(__instance);
            return true;
        }

        public static bool countMosaic(object __instance, ref int __result, bool only_on_sensitive)
        {
            if (!UNCENSOR)
            {
                setEnabledToFalse(true);
                setUseMosaicToFalse(true);
                return true;
            }
            setUseMosaicToFalse(__instance);
            setEnabledToFalse(__instance);
            __result = 0;
            return false;
        }

        public static void PatchHarmonyMethodUnity(Type originalClass, string originalMethodName, string patchedMethodName, bool usePrefix, bool usePostfix, Type[] parameters = null)
        {
            string uniqueId = "com.wolfitdm.AliceInGradleDemosaicMod";
            Type uniqueType = typeof(AliceInGradleDemosaicMod);

            // Create a new Harmony instance with a unique ID
            var harmony = new Harmony(uniqueId);

            if (originalClass == null)
            {
                Logger.LogInfo($"GetType originalClass == null");
                return;
            }

            MethodInfo patched = null;

            try
            {
                patched = AccessTools.Method(uniqueType, patchedMethodName);
            }
            catch (Exception ex)
            {
                patched = null;
            }

            if (patched == null)
            {
                Logger.LogInfo($"AccessTool.Method patched {patchedMethodName} == null");
                return;

            }

            // Or apply patches manually
            MethodInfo original = null;

            try
            {
                if (parameters == null)
                {
                    original = AccessTools.Method(originalClass, originalMethodName);
                }
                else
                {
                    original = AccessTools.Method(originalClass, originalMethodName, parameters);
                }
            }
            catch (AmbiguousMatchException ex)
            {
                Type[] nullParameters = new Type[] { };
                try
                {
                    if (patched == null)
                    {
                        parameters = nullParameters;
                    }

                    ParameterInfo[] parameterInfos = patched.GetParameters();

                    if (parameterInfos == null || parameterInfos.Length == 0)
                    {
                        parameters = nullParameters;
                    }

                    List<Type> parametersN = new List<Type>();

                    for (int i = 0; i < parameterInfos.Length; i++)
                    {
                        ParameterInfo parameterInfo = parameterInfos[i];

                        if (parameterInfo == null)
                        {
                            continue;
                        }

                        if (parameterInfo.Name == null)
                        {
                            continue;
                        }

                        if (parameterInfo.Name.StartsWith("__"))
                        {
                            continue;
                        }

                        Type type = parameterInfos[i].ParameterType;

                        if (type == null)
                        {
                            continue;
                        }

                        parametersN.Add(type);
                    }

                    parameters = parametersN.ToArray();
                }
                catch (Exception ex2)
                {
                    parameters = nullParameters;
                }

                try
                {
                    original = AccessTools.Method(originalClass, originalMethodName, parameters);
                }
                catch (Exception ex2)
                {
                    original = null;
                }
            }
            catch (Exception ex)
            {
                original = null;
            }

            if (original == null)
            {
                Logger.LogInfo($"AccessTool.Method original {originalMethodName} == null");
                return;
            }

            HarmonyMethod patchedMethod = new HarmonyMethod(patched);
            var prefixMethod = usePrefix ? patchedMethod : null;
            var postfixMethod = usePostfix ? patchedMethod : null;

            harmony.Patch(original,
                prefix: prefixMethod,
                postfix: postfixMethod);
        }
    }
 }