using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using JetBrains.Annotations;
using m2d;
using MapMagic.Nodes;
using nel;
using nel.gm;
using Novell.Directory.Ldap.Utilclass;
using PixelLiner;
using Spine;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using UnityEngine;
using UnityEngine.NVIDIA;
using XX;
using static nel.QuestTracker;
using static nel.UiHkdsChat;
using static NetworkDebugStart;

namespace AliceInGradleDemosaicMod
{
    [BepInPlugin("com.wolfitdm.AliceInGradleDemosaicMod", "AliceInGradleDemosaicMod Plugin", "1.0.0.0")]
    public class AliceInGradleDemosaicMod : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private static ConfigEntry<bool> configEnableMe;
        private static ConfigEntry<bool> configMenuDefaultOpen;
        private static ConfigEntry<KeyCode> configKeyCodeToOpenCheatMenu;
        private static ConfigEntry<bool> saveSettingsInConfig;
        private static Dictionary<string, ConfigEntry<bool>> configEntries = new Dictionary<string, ConfigEntry<bool>>();
        private static Dictionary<string, bool> configEntriesFirstSet = new Dictionary<string, bool>();
        private static Dictionary<string, ConfigEntry<float>> configEntriesFloat = new Dictionary<string, ConfigEntry<float>>();
        private static Dictionary<string, float> configEntriesFirstSetFloat = new Dictionary<string, float>();
        private static AliceInGradleDemosaicMod Instance = null;

        private static ConfigEntry<bool> getConfigEntry(string section, string var, bool defaultValue = false)
        {
            if (Instance == null)
            {
                return null;
            }

            if (!configEntries.ContainsKey(var))
            {
                ConfigEntry<bool> setC = Instance.Config.Bind(section, var, defaultValue, $"{var}, default {defaultValue}");
                configEntries.Add(var, setC);
            }

            return configEntries[var];
        }

        private static bool updateVarFirst(string section, string var, bool defaultValue = false)
        {
            ConfigEntry<bool> setC = getConfigEntry(section, var, defaultValue);

            if (setC == null)
            {
                return false;
            }

            if (!configEntriesFirstSet.ContainsKey(var))
            {

                configEntriesFirstSet.Add(var, saveSettingsInConfig.Value ? setC.Value : defaultValue);
            }

            bool set = defaultValue;

            if (saveSettingsInConfig.Value)
            {
                set = setC.Value = configEntriesFirstSet[var];
            }

            return set;
        }

        private static void updateVarSecond(string section, string var, bool set)
        {
            ConfigEntry<bool> setC = getConfigEntry(section, var);

            if (setC == null)
            {
                return;
            }

            configEntriesFirstSet[var] = set;

            if (saveSettingsInConfig.Value)
            {
                setC.Value = configEntriesFirstSet[var];
            }

            return;
        }
        private static bool updateVarFirstForce(string section, string var, bool defaultValue = false)
        {
            ConfigEntry<bool> setC = getConfigEntry(section, var, defaultValue);

            if (setC == null)
            {
                return false;
            }

            if (!configEntriesFirstSet.ContainsKey(var))
            {

                configEntriesFirstSet.Add(var, setC.Value);
            }

            bool set = defaultValue;

            set = setC.Value = configEntriesFirstSet[var];

            return set;
        }

        private static void updateVarSecondForce(string section, string var, bool set)
        {
            ConfigEntry<bool> setC = getConfigEntry(section, var);

            if (setC == null)
            {
                return;
            }

            configEntriesFirstSet[var] = set;
            setC.Value = configEntriesFirstSet[var];

            return;
        }

        private static ConfigEntry<float> getConfigEntryFloat(string section, string var, float defaultValue = 0)
        {
            if (Instance == null)
            {
                return null;
            }

            if (!configEntriesFloat.ContainsKey(var))
            {
                ConfigEntry<float> setC = Instance.Config.Bind(section, var, 0f, $"{var}, default {defaultValue}");
                configEntriesFloat.Add(var, setC);
            }

            return configEntriesFloat[var];
        }

        private static float updateVarFirstFloat(string section, string var, float defaultValue = 0)
        {
            ConfigEntry<float> setC = getConfigEntryFloat(section, var, defaultValue);

            if (setC == null)
            {
                return 0;
            }

            if (!configEntriesFirstSetFloat.ContainsKey(var))
            {
                configEntriesFirstSetFloat.Add(var, saveSettingsInConfig.Value ? setC.Value : defaultValue);
            }

            float set = defaultValue;

            if (saveSettingsInConfig.Value)
            {
                set = setC.Value = configEntriesFirstSetFloat[var];
            }

            return set;
        }

        private static void updateVarSecondFloat(string section, string var, float set)
        {
            ConfigEntry<float> setC = getConfigEntryFloat(section, var);

            if (setC == null)
            {
                return;
            }

            configEntriesFirstSetFloat[var] = set;

            if (saveSettingsInConfig.Value)
            {
                setC.Value = configEntriesFirstSetFloat[var];
            }

            return;
        }

        public static bool showMenu = false;

        private Vector2 scrollPos;     // Scroll-Position
        private bool foldoutPlayer = false;
        private bool foldoutSuperNoel = false;
        private bool foldoutPervertFuncs = false;

        public static KeyCode keyCodeToOpenCloseTheCheatsMenu = 0;

        public static Rect windowRect = new Rect(40, 40, 600, 600);

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

            saveSettingsInConfig = Config.Bind(pluginKey,
                                              "SaveSettingsInConfig",
                                              false,
                                             "Whether or not you want save settings in config (default false also no, you do not want it, and true = yes)");

            configKeyCodeToOpenCheatMenu = Config.Bind(pluginKey,
                                             "KeyCodeToOpenCloseTheCheatsMenu",
                                             KeyCode.R,
                                            "KeyCode to open/close the cheats menu, default R");

            Instance = this;

            enableMe = configEnableMe.Value;
            showMenu = configMenuDefaultOpen.Value;

            keyCodeToOpenCloseTheCheatsMenu = configKeyCodeToOpenCheatMenu.Value;

            SetGameValues.initSetGameValuesVars();
            Debug.initDebugVars();
            SuperNoel.initSuperNoelVars();
            NoelPervert.initNoelPervertVars();
            DebugMe.initDebugMeVars();

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

            UnityEngine.Event e = UnityEngine.Event.current;

            // Block Enter/Return key presses
            if (e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter))
            {
                e.Use(); // Prevents the event from reaching the TextField
            }

            // Fenster zentriert anzeigen
            windowRect = GUI.Window(0, windowRect, DrawCheatWindow, "!!! Alice In Gradle Cheat Menu !!!");
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

        public static class SetGameValues
        {
            private static NelM2DBase m2d;
            private static PRNoel noel;
            private static bool isInit = false;

            public static bool USE_UNSAFE_FUNCS = true;

            public static void initSetGameValuesVars()
            {
                USE_UNSAFE_FUNCS = updateVarFirstForce("SetGameValues", "USE_UNSAFE_FUNCS", true);
            }

            public static void updateSetGameValuesVars()
            {
                updateVarSecondForce("SetGameValues", "USE_UNSAFE_FUNCS", USE_UNSAFE_FUNCS);
            }
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

            private static bool use_unsafe_func()
            {
                if (USE_UNSAFE_FUNCS)
                {
                    return true;
                }

                GUILayout.Label("You can not use unsafe func!");
                return false;
            }

            private static Texture2D MakeReadable(Texture2D source)
            {
                if (source == null)
                {
                    Logger.LogError("Source texture is null.");
                    return null;
                }

                // Create a temporary RenderTexture
                RenderTexture rt = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

                try
                {
                    // Copy source texture to RenderTexture
                    UnityEngine.Graphics.Blit(source, rt);

                    // Backup the currently active RenderTexture
                    //RenderTexture previous = RenderTexture.active;
                    //RenderTexture.active = rt;

                    // Create a new readable Texture2D
                    Texture2D readableTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
                    readableTex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                    readableTex.Apply();

                    // Restore the active RenderTexture
                    //RenderTexture.active = previous;

                    return readableTex;
                }
                finally
                {
                    try
                    {
                        // Release the temporary RenderTexture
                        RenderTexture.ReleaseTemporary(rt);
                    }
                    catch { }
                }
            }
            public static void FindAndLogTextures()
            {
                if (!use_unsafe_func())
                {
                    return;
                }

                try
                {
                    // Find all loaded textures, including inactive and hidden ones
                    var textures = Resources.FindObjectsOfTypeAll<Texture2D>();

                    Logger.LogInfo($"Found {textures.Length} textures.");

                    foreach (var tex in textures.OrderBy(t => t.name))
                    {
                        if (tex != null)
                        {
                            Logger.LogInfo($"Texture: {tex.name} | Size: {tex.width}x{tex.height} | Format: {tex.format}");

                            Texture2D texx = MakeReadable(tex);
                            byte[] bytes = null;

                            try
                            {
                                bytes = texx.EncodeToPNG();
                            }
                            catch (Exception e)
                            {
                                continue;
                            }

                            if (bytes == null)
                            {
                                continue;
                            }

                            File.WriteAllBytes(tex.name + ".png", bytes);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    Logger.LogError($"Error while finding textures: {ex}");
                }
            }
            public static void tornClothes(int i)
            {
                if (!use_unsafe_func())
                {
                    return;
                }

                init();
                if (!isInit)
                {
                    return;
                }

                if (noel == null)
                {
                    Logger.LogInfo("noel == null");
                    return;
                }

                noel.Ser.Add(SER.CLT_BROKEN, i, 9999, false);
                noel.UP.applyTorn();
                noel.BetoMng.torned_count = 99;
                noel.UP.recheck();
                noel.BetoMng.addTorned(noel, i, -1000);
                noel.UP.recheck();

                M2SerItem m2SerItem = noel.Ser.Get(SER.CLT_BROKEN);
                noel.Ser.resetFlags();
                noel.setOutfitType(PRNoel.OUTFIT.TORNED, true, true);
                noel.setDefaultPose("damage_thunder");
                noel.SpSetPose("damage_thunder");

               BetoInfo info = noel.BetoMng.getInfo(noel.BetoMng.get_current_dirt());
               Logger.LogInfo(info.level);
               info.level = i;
               MeshDrawer mdTemp = (MeshDrawer)noel.BetoMng.GetType().GetField("MdTemp", BindingFlags.NonPublic | BindingFlags.Static).GetValue(noel.BetoMng);

                GL.Begin(4);
                int n = i <= 0 ? 0 : i;
                long ran = Math.Min(MTR.ANoelBreakCloth.Length-1, n);
                PxlFrame pxlFrame = MTR.ANoelBreakCloth[ran];
                mdTemp.initForImg(pxlFrame.getLayer(0).Img);
                BLIT.RenderToGLOneTask(mdTemp, mdTemp.getTriMax());
                GL.End();
            }
            public static void changeOutFit(string outfit)
            {
                init();
                if (!isInit)
                {
                    return;
                }
                PRNoel.OUTFIT outfitS = noel.outfit_type;
                switch(outfit)
                {
                    case "TORNED":
                        break;

                    case "BABYDOLL":
                        outfitS = PRNoel.OUTFIT.BABYDOLL;
                        break;

                    case "DOJO":
                        outfitS = PRNoel.OUTFIT.DOJO;
                        break;

                    case "NORMAL":
                        outfitS = PRNoel.OUTFIT.NORMAL;
                        break;
                }
                noel.setOutfitType(outfitS, true, true);
            }

            public static void unlockBenchMenu()
            {
                FieldInfo CmdBenchPeeField = typeof(UiBenchMenu).GetField("CmdBenchPee", BindingFlags.NonPublic | BindingFlags.Static);
                FieldInfo ACmdField = typeof(UiBenchMenu).GetField("ACmd", BindingFlags.NonPublic | BindingFlags.Static);

                object ACmdO = ACmdField != null ? ACmdField.GetValue(null) : null;
                object CmdBenchPeeO = CmdBenchPeeField != null ? CmdBenchPeeField.GetValue(null) : null;

                object[] ACmd = ACmdO != null ? (object[])ACmdO : null;
                object CmdBenchPee = CmdBenchPeeO != null ? (object)CmdBenchPeeO : null;

                if (ACmd == null)
                {
                    Logger.LogInfo("ACmd == null");
                    return;
                }

                if (CmdBenchPee == null)
                {
                    Logger.LogInfo("CmdBenchPee == null");
                    return;
                }

                for (int index = 0; index < ACmd.Length; ++index)
                {
                    object obj = ACmd[index];

                    if (obj == null)
                    {
                        Logger.LogInfo(index + " is null");
                        continue;
                    }

                    try
                    {
                        Traverse.Create(obj).Field("FnCanUse").SetValue(null);
                    } catch (Exception ex) {
                        Logger.LogInfo("FnCanUse == null here 1");
                        Logger.LogError(ex.ToString());
                    }

                    try
                    {
                        Traverse.Create(obj).Field("scn_enable").SetValue(true);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogInfo("scn_enable == null here 2");
                        Logger.LogError(ex.ToString());
                    }
                }

                try
                {
                    Traverse.Create(CmdBenchPee).Field("FnCanUse").SetValue(null);
                }
                catch (Exception ex)
                {
                    Logger.LogInfo("FnCanUse == null here 3");
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    Traverse.Create(CmdBenchPee).Field("scn_enable").SetValue(true);
                }
                catch (Exception ex)
                {
                    Logger.LogInfo("scn_enable == null here 4");
                    Logger.LogError(ex.ToString());
                }
            }

            public static void unlockNoelCanPee()
            {
                init();
                if (!isInit)
                {
                    return;
                }
                noel.Ser.Add(SER.NEAR_PEE);
            }

            public static void unlockNoelHaveSex()
            {
                init();
                if (!isInit)
                {
                    return;
                }
                noel.Ser.Add(SER.FORBIDDEN_ORGASM);
                noel.Ser.Add(SER.SEXERCISE);
            }
            public static void SetMoney(int value, uint max)
            {
                init();
                if (!isInit)
                {
                    return;
                }
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
                init();
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
                if (!use_unsafe_func())
                {
                    return;
                }

                init();
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
                if (!use_unsafe_func())
                {
                    return;
                }
                init();
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
                if (!use_unsafe_func())
                {
                    return;
                }
                init();
                if (!isInit)
                {
                    return;
                }
                noel.GaugeBrk.reset();
                UIStatus.Instance.quitCrack();
            }
            public static void PlantEggs()
            {
                if (!use_unsafe_func())
                {
                    return;
                }
                init();
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

            public static void uncensorRestRoomPee(bool uncensor = true)
            {
                uncensorEvImgFile("__events_restroom.pxls.bytes.texture_0.dat", uncensor);
            }

            public static void uncensor2WeekAttack(bool uncensor = true)
            {
                uncensorEvImgFile("__events_2weekattack.pxls.bytes.texture_0.dat", uncensor);
            }

            public static void uncensorEggRemoveAttack(bool uncensor = true)
            {
                uncensorEvImgFile("__events_eggremove.pxls.bytes.texture_0.dat", uncensor);
            }
            public static void uncensorDamageFdownAttackVersion1(bool uncensor = true)
            {
                uncensorSpineAnimFile("damage_fdown.dat", uncensor);
            }

            public static void uncensorDamageFdownAttackVersion2(bool uncensor = true)
            {
                uncensorSpineAnimFile("damage_fdown.dat", uncensor, "damage_fdown_ver2.dat");
            }

            public static void uncensorStandNormal(bool uncensor = true)
            {
                uncensorSpineAnimFile("stand_normal.dat", uncensor);
            }
            public static void uncensorStandNormalVersion2(bool uncensor = true)
            {
                uncensorSpineAnimFile("stand_normal.dat", uncensor, "stand_normal_ver2.dat");
            }
            public static void uncensorStandNormalVersion3(bool uncensor = true)
            {
                uncensorSpineAnimFile("stand_normal.dat", uncensor, "stand_normal_ver3.dat");
            }
            public static void uncensorStandWeak(bool uncensor = true)
            {
                uncensorSpineAnimFile("stand_weak.dat", uncensor);
            }
            public static void uncensorStandWeakVersion2(bool uncensor = true)
            {
                uncensorSpineAnimFile("stand_weak.dat", uncensor, "stand_weak_ver2.dat");
            }
            public static void uncensorDamageA(bool uncensor = true)
            {
                uncensorSpineAnimFile("damage_a", uncensor);
            }
            public static void uncensorDamageB(bool uncensor = true)
            {
                uncensorSpineAnimFile("damage_b", uncensor);
            }
            public static void uncensorEvImgFile(string file, bool uncensor = true)
            {
                string path1 = "/EvImg";
                string path2 = $"{file}";
                string path3 = $"/EvImg/{file}";
                string path4 = path2;
                uncensorFile(path1, path2, path3, path4, uncensor);
            }
            public static void uncensorSpineAnimFile(string file, bool uncensor = true, string path4file = "")
            {
                string path1 = "/SpineAnim";
                string path2 = $"{file}";
                string path3 = $"/SpineAnim/{file}";
                string path4 = path2;

                if (path4file != "")
                {
                    path4 = path4file;
                }

                uncensorFile(path1, path2, path3, path4, uncensor);
            }
            public static void uncensorFile(string path1, string path2, string path3, string path4, bool uncensor = true)
            {
                Directory.CreateDirectory("BepInEx/textures/original");
                Directory.CreateDirectory("BepInEx/textures/mod");
                Directory.CreateDirectory($"AliceInCradle_Data/StreamingAssets{path1}");

                if (!File.Exists($"BepInEx/textures/original/{path2}"))
                {
                    File.Copy($"AliceInCradle_Data/StreamingAssets{path3}", $"BepInEx/textures/original/{path2}");
                }

                string targetDir = uncensor ? "mod" : "original";
                if (File.Exists($"BepInEx/textures/{targetDir}/{path4}"))
                {
                    if (File.Exists($"AliceInCradle_Data/StreamingAssets{path3}"))
                    {
                        File.Delete($"AliceInCradle_Data/StreamingAssets{path3}");
                    }
                    File.Copy($"BepInEx/textures/{targetDir}/{path4}", $"AliceInCradle_Data/StreamingAssets{path3}");
                }

                GUILayout.Label("Please restart Alice In Gradle, in order to see a effect!");
            }
        }

        public static void PatchHarmonyMethodsUnityPrefix(Type uniqueType, Type[] types, string original, string patched)
        {
            
            foreach (Type type in types)
            {
                string patch = patched;
                
                if (type == typeof(NelNPuppy) && original == "initAbsorb" &&  patched == "SuperNoelNoPervertDisableHpAndAbsorbDamage2")
                {
                    patch = "SuperNoelNoPervertDisableHpAndAbsorbDamage3";
                }

                try
                {
                    PatchHarmonyMethodUnityClass(uniqueType, type, original, patch, true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
            }
        }

        public static class Debug
        {
            public static bool UNCENSOR = true;

            public static bool UNCENSOR_RESTROOM = true;

            public static bool UNCENSOR_2WEEK_ATTACK = true;

            public static bool UNCENSOR_EGG_REMOVE_ATTACK = true;

            public static bool UNCENSOR_DAMAGE_FDOWN_VERSION1 = true;

            public static bool UNCENSOR_DAMAGE_FDOWN_VERSION2 = true;

            public static bool UNCENSOR_STAND_NORMAL = true;

            public static bool UNCENSOR_STAND_NORMAL_VERSION2 = true;

            public static bool UNCENSOR_STAND_NORMAL_VERSION3 = true;

            public static bool UNCENSOR_STAND_WEAK = true;

            public static bool UNCENSOR_STAND_WEAK_VERSION2 = true;

            public static bool UNCENSOR_DAMAGE_A = true;

            public static bool UNCENSOR_DAMAGE_B = true;

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

            public static void initDebugVars()
            {
                UNCENSOR = updateVarFirst("Debug", "UNCENSOR", true);

                UNCENSOR_RESTROOM = updateVarFirstForce("Debug", "UNCENSOR_RESTROOM");

                UNCENSOR_2WEEK_ATTACK = updateVarFirstForce("Debug", "UNCENSOR_2WEEK_ATTACK");

                UNCENSOR_EGG_REMOVE_ATTACK = updateVarFirstForce("Debug", "UNCENSOR_EGG_REMOVE_ATTACK");

                UNCENSOR_DAMAGE_FDOWN_VERSION1 = updateVarFirstForce("Debug", "UNCENSOR_DAMAGE_FDOWN_VERSION1");

                UNCENSOR_DAMAGE_FDOWN_VERSION2 = updateVarFirstForce("Debug", "UNCENSOR_DAMAGE_FDOWN_VERSION2");

                UNCENSOR_STAND_NORMAL = updateVarFirstForce("Debug", "UNCENSOR_STAND_NORMAL");

                UNCENSOR_STAND_NORMAL_VERSION2 = updateVarFirstForce("Debug", "UNCENSOR_STAND_NORMAL_VERSION2");

                UNCENSOR_STAND_NORMAL_VERSION3 = updateVarFirstForce("Debug", "UNCENSOR_STAND_NORMAL_VERSION3");

                UNCENSOR_STAND_WEAK = updateVarFirstForce("Debug", "UNCENSOR_STAND_WEAK");

                UNCENSOR_STAND_WEAK_VERSION2 = updateVarFirstForce("Debug", "UNCENSOR_STAND_WEAK_VERSION2");

                UNCENSOR_DAMAGE_A = updateVarFirstForce("Debug", "UNCENSOR_DAMAGE_A");

                UNCENSOR_DAMAGE_B = updateVarFirstForce("Debug", "UNCENSOR_DAMAGE_B");

                DEBUG = updateVarFirst("Debug", "DEBUG", true);

                DEBUGNOCFG = updateVarFirst("Debug", "DEBUGNOCFG");

                DEBUGSUPERSENSITIVE = updateVarFirst("Debug", "DEBUGSUPERSENSITIVE");

                DEBUGANNOUNCE = updateVarFirst("Debug", "DEBUGANNOUNCE");

                DEBUGNOSND = updateVarFirst("Debug", "DEBUGNOSND");

                DEBUG_PLAYER = updateVarFirst("Debug", "DEBUGPLAYER");

                DEBUGALLSKILL = updateVarFirst("Debug", "DEBUGALLSKILL");

                DEBUGNOEVENT = updateVarFirst("Debug", "DEBUGNOEVENT");

                DEBUGNOVOICE = updateVarFirst("Debug", "DEBUGNOVOICE");

                DEBUGRELOADMTR = updateVarFirst("Debug", "DEBUGRELOADMTR");

                DEBUGTIMESTAMP = updateVarFirst("Debug", "DEBUGTIMESTAMP");

                DEBUGALBUMUNLOCK = updateVarFirst("Debug", "DEBUGALBUMUNLOCK");

                DEBUGSTABILIZE_DRAW = updateVarFirst("Debug", "DEBUGSTABILIZE_DRAW");

                DEBUGMIGHTY = updateVarFirst("Debug", "DEBUGMIGHTY");

                DEBUGNODAMAGE = updateVarFirst("Debug", "DEBUGNODAMAGE");

                DEBUGBENCHMARK = updateVarFirst("Debug", "DEBUGBENCHMARK");

                DEBUGSUPERCYCLONE = updateVarFirst("Debug", "DEBUGSUPERCYCLONE");

                ENG_MODE = updateVarFirst("Debug", "ENG_MODE");

                DEBUGSPEFFECT = updateVarFirst("Debug", "DEBUGSPEFFECT");
            }

            public static void updateDebugVars()
            {
                if (!saveSettingsInConfig.Value)
                {
                    return;
                }

                updateVarSecond("Debug", "UNCENSOR", UNCENSOR);

                updateVarSecondForce("Debug", "UNCENSOR_RESTROOM", UNCENSOR_RESTROOM);

                updateVarSecondForce("Debug", "UNCENSOR_2WEEK_ATTACK", UNCENSOR_2WEEK_ATTACK);

                updateVarSecondForce("Debug", "UNCENSOR_EGG_REMOVE_ATTACK", UNCENSOR_EGG_REMOVE_ATTACK);
 
                updateVarSecondForce("Debug", "UNCENSOR_DAMAGE_FDOWN_VERSION1", UNCENSOR_DAMAGE_FDOWN_VERSION1);

                updateVarSecondForce("Debug", "UNCENSOR_DAMAGE_FDOWN_VERSION2", UNCENSOR_DAMAGE_FDOWN_VERSION2);

                updateVarSecondForce("Debug", "UNCENSOR_STAND_NORMAL", UNCENSOR_STAND_NORMAL);

                updateVarSecondForce("Debug", "UNCENSOR_STAND_NORMAL_VERSION2", UNCENSOR_STAND_NORMAL_VERSION2);

                updateVarSecondForce("Debug", "UNCENSOR_STAND_NORMAL_VERSION3", UNCENSOR_STAND_NORMAL_VERSION3);

                updateVarSecondForce("Debug", "UNCENSOR_STAND_WEAK", UNCENSOR_STAND_WEAK);

                updateVarSecondForce("Debug", "UNCENSOR_STAND_WEAK_VERSION2", UNCENSOR_STAND_WEAK_VERSION2);

                updateVarSecondForce("Debug", "UNCENSOR_DAMAGE_A", UNCENSOR_DAMAGE_A);

                updateVarSecondForce("Debug", "UNCENSOR_DAMAGE_B", UNCENSOR_DAMAGE_B);

                updateVarSecond("Debug", "DEBUG", DEBUG);

                updateVarSecond("Debug", "DEBUGNOCFG", DEBUGNOCFG);

                updateVarSecond("Debug", "DEBUGSUPERSENSITIVE", DEBUGSUPERSENSITIVE);

                updateVarSecond("Debug", "DEBUGANNOUNCE", DEBUGANNOUNCE);

                updateVarSecond("Debug", "DEBUGNOSND", DEBUGNOSND);

                updateVarSecond("Debug", "DEBUGPLAYER", DEBUG_PLAYER);

                updateVarSecond("Debug", "DEBUGALLSKILL", DEBUGALLSKILL);

                updateVarSecond("Debug", "DEBUGNOEVENT", DEBUGNOEVENT);

                updateVarSecond("Debug", "DEBUGNOVOICE", DEBUGNOVOICE);

                updateVarSecond("Debug", "DEBUGRELOADMTR", DEBUGRELOADMTR);

                updateVarSecond("Debug", "DEBUGTIMESTAMP", DEBUGTIMESTAMP);

                updateVarSecond("Debug", "DEBUGALBUMUNLOCK", DEBUGALBUMUNLOCK);

                updateVarSecond("Debug", "DEBUGSTABILIZE_DRAW", DEBUGSTABILIZE_DRAW);

                updateVarSecond("Debug", "DEBUGMIGHTY", DEBUGMIGHTY);

                updateVarSecond("Debug", "DEBUGNODAMAGE", DEBUGNODAMAGE);

                updateVarSecond("Debug", "DEBUGBENCHMARK", DEBUGBENCHMARK);

                updateVarSecond("Debug", "DEBUGSUPERCYCLONE", DEBUGSUPERCYCLONE);

                updateVarSecond("Debug", "ENG_MODE", ENG_MODE);

                updateVarSecond("Debug", "DEBUGSPEFFECT", DEBUGSPEFFECT);
            }
            public static string st(string text)
            {
                bool set = false;
                try
                {
                    switch (text)
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
                }
                catch (Exception ex)
                {
                }

                string ret = text.Replace("DEBUG", "DEBUG: ") + ": " + (set ? "ON" : "OFF");

                return ret;
            }

            public static void updateVar(string text)
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

            public static bool ImmuneToSleep = false;
            public static bool ImmuneToConfuse = false;
            public static bool ImmuneToParalysis = false;
            public static bool ImmuneToBurn = false;
            public static bool ImmuneToFrozen = false;
            public static bool ImmuneToJamming = false;

            public static bool NoPervertDisableGrabAttack = false;
            public static bool NoPervertDisableEpDamage = false;
            public static bool NoPervertSkipGameOverPlay = false;
            public static bool NoPervertDisableWormTrap = false;
            public static bool NoPervertDisableHpAndAbsorbDamage = false;

            public static void initSuperNoelVars()
            {
                Invincible = updateVarFirst("SuperNoel", "Invincible");
                DamageMultiplier = updateVarFirst("SuperNoel", "DamageMultiplier");
                InfinteBomb = updateVarFirst("SuperNoel", "InfiniteBomb");
                InfiniteJump = updateVarFirst("SuperNoel", "InfiniteJump");
                DurableShield = updateVarFirst("SuperNoel", "DurableShield");
                DisableGasDamage = updateVarFirst("SuperNoel", "DisableGasDamage");
                ImmuneToMapThorn = updateVarFirst("SuperNoel", "ImmuneToMapThorn");
                ImmuneToLava = updateVarFirst("SuperNoel", "ImmuneToLava");

                ImmuneToSleep = updateVarFirst("SuperNoel", "ImmuneToSleep");
                ImmuneToConfuse = updateVarFirst("SuperNoel", "ImmuneToConfuse");
                ImmuneToParalysis = updateVarFirst("SuperNoel", "ImmuneToParalysis");
                ImmuneToBurn = updateVarFirst("SuperNoel", "ImmuneToBurn");
                ImmuneToFrozen = updateVarFirst("SuperNoel", "ImmuneToFrozen");
                ImmuneToJamming = updateVarFirst("SuperNoel", "ImmuneToJamming");

                NoPervertDisableGrabAttack = updateVarFirst("SuperNoel", "NoPervertDisableGrabAttack");
                NoPervertDisableEpDamage = updateVarFirst("SuperNoel", "NoPervertDisableEpDamage");
                NoPervertSkipGameOverPlay = updateVarFirst("SuperNoel", "NoPervertSkipGameOverPlay");
                NoPervertDisableWormTrap = updateVarFirst("SuperNoel", "NoPervertDisableWormTrap");
                NoPervertDisableHpAndAbsorbDamage = updateVarFirst("SuperNoel", "NoPervertDisableHpAndAbsorbDamage");

                hp_dmg_def = updateVarFirstFloat("SuperNoel", "hp_dmg_def", 0);
                mp_dmg_def = updateVarFirstFloat("SuperNoel", "mp_dmg_def", 0);
            }

            public static void updateSuperNoelVars()
            {
                if (!saveSettingsInConfig.Value)
                {
                    return;
                }

                updateVarSecond("SuperNoel", "Invincible", Invincible);
                updateVarSecond("SuperNoel", "DamageMultiplier", DamageMultiplier);
                updateVarSecond("SuperNoel", "InfiniteBomb", InfinteBomb);
                updateVarSecond("SuperNoel", "InfiniteJump", InfiniteJump);
                updateVarSecond("SuperNoel", "DurableShield", DurableShield);
                updateVarSecond("SuperNoel", "DisableGasDamage", DisableGasDamage);
                updateVarSecond("SuperNoel", "ImmuneToMapThorn", ImmuneToMapThorn);
                updateVarSecond("SuperNoel", "ImmuneToLava", ImmuneToLava);

                updateVarSecond("SuperNoel", "ImmuneToSleep", ImmuneToSleep);
                updateVarSecond("SuperNoel", "ImmuneToConfuse", ImmuneToConfuse);
                updateVarSecond("SuperNoel", "ImmuneToParalysis", ImmuneToParalysis);
                updateVarSecond("SuperNoel", "ImmuneToBurn", ImmuneToBurn);
                updateVarSecond("SuperNoel", "ImmuneToFrozen", ImmuneToFrozen);
                updateVarSecond("SuperNoel", "ImmuneToJamming", ImmuneToJamming);

                updateVarSecond("SuperNoel", "NoPervertDisableGrabAttack", NoPervertDisableGrabAttack);
                updateVarSecond("SuperNoel", "NoPervertDisableEpDamage", NoPervertDisableEpDamage);
                updateVarSecond("SuperNoel", "NoPervertSkipGameOverPlay", NoPervertSkipGameOverPlay);
                updateVarSecond("SuperNoel", "NoPervertDisableWormTrap", NoPervertDisableWormTrap);
                updateVarSecond("SuperNoel", "NoPervertDisableHpAndAbsorbDamage", NoPervertDisableHpAndAbsorbDamage);

                updateVarSecondFloat("SuperNoel", "hp_dmg_def", hp_dmg_def);
                updateVarSecondFloat("SuperNoel", "mp_dmg_def", mp_dmg_def);
            }

            public static string currentItemName = string.Empty;
            public static int currentItemGrade = -1;
            public static int currentItemAmount = 1;

            public static bool addItemCmd = false;

            public static float hp_dmg_def = 0;
            public static float mp_dmg_def = 0;
            public static void ExecuteHarmonyPatches()
            {
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(M2PrSkill), "AtkMul", "SuperNoelDamageMultiplier", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(PR), "applyDamage", "SuperNoelDamageMultiplier2", true, false, new Type[] { typeof(NelAttackInfo), typeof(bool) });
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(PR), "runPhysics", "SuperNoelInfiniteJump", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(M2PrSkill), "explodeMagic", "SuperNoelInfiniteBomb", false, true);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(M2Shield), "run", "SuperNoelDuralableShield", true, false);
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
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(M2Shield), "checkShield", "SuperNoelDuralableShield2", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(PR), "canApplyGasDamage", "SuperNoelDisableGasDamage", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(PR), "applyDamageFromMap", "SuperNoelImmuneToMapThornOrLava", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(NelNGolem), "readTicketOd", "SuperNoelNoPervertDisableGrabAttack", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(PR), "initAbsorb", "SuperNoelNoPervertDisableGrabAttack2", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(EpManager), "applyEpDamage", "SuperNoelNoPervertDisableEpDamage", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(UiGO), "run", "SuperNoelNoPervertSkipGameOverPlay", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(PR), "canPullByWorm", "SuperNoelNoPervertDisableWormTrap", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(NelChipWormHead), "initAction", "SuperNoelNoPervertDisableWormTrap2", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                PatchHarmonyMethodsUnityPrefix(typeof(AliceInGradleDemosaicMod.SuperNoel), [typeof(NelNFox), typeof(NelNGolem), typeof(NelNMush),
                    typeof(NelNPuppy), typeof(NelNSlime),typeof(NelNSnake), typeof(NelNUni)], "runAbsorb", "SuperNoelNoPervertDisableHpAndAbsorbDamage");

                PatchHarmonyMethodsUnityPrefix(typeof(AliceInGradleDemosaicMod.SuperNoel), [typeof(NelNFox), typeof(NelNGolem), typeof(NelNMush),
                    typeof(NelNPuppy), typeof(NelNSlime),typeof(NelNSnake), typeof(NelNUni)], "initAbsorb", "SuperNoelNoPervertDisableHpAndAbsorbDamage2");

                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(NelNSlime), "runAbsorbOverDrive", "SuperNoelNoPervertDisableHpAndAbsorbDamage", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(M2Ser), "Add", "SuperNoelImmuneToOther", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(UiItemManageBox), "fnClickItemCmd", "SuperNoelAddItem", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.SuperNoel), typeof(UiItemManageBox), "fnClickItemRow", "SuperNoelAddItem2", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
            }
            private static bool SuperNoelDamageMultiplier(MagicItem Mg, NelAttackInfo Atk, ref float hpdmg, ref float mpdmg, int minhpdmg0)
            {
                if (!DamageMultiplier)
                {
                    return true;
                }

                hpdmg *= hp_dmg_def;
                mpdmg *= mp_dmg_def;
                return true;
            }

            private static bool SuperNoelDamageMultiplier2(NelAttackInfo Atk, bool force)
            {
                if (!DamageMultiplier)
                {
                    return true;
                }

                if (Atk.AttackFrom is PR)
                {
                    Atk.mpdmg0 *= (int)hp_dmg_def;
                    Atk.hpdmg0 *= (int)mp_dmg_def;
                }

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
            private static bool SuperNoelInfiniteJump(object __instance, float fcnt)
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
            private static void SuperNoelInfiniteBomb(ref object __instance, MagicItem TargetMg)
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
            private static bool SuperNoelDuralableShield(float fcnt, ref float power_progress_level, float fcnt_for_lock_time_progress)
            {
                if (!DurableShield) { return true; }

                power_progress_level = 0;

                return true;
            }

            private static bool SuperNoelDuralableShield2(AttackInfo Atk, ref float val, bool from_away)
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
            private static bool SuperNoelImmuneToMapThornOrLava(M2MapDamageContainer.M2MapDamageItem MDI, AttackInfo _Atk, float efx, float efy, bool apply_execute, ref AttackInfo __result)
            {
                if (ImmuneToLava)
                {
                    if (MDI.kind == MAPDMG.LAVA)
                    {
                        __result = null;
                        return false;
                    }
                }
                else if (ImmuneToMapThorn)
                {
                    __result = null;
                    return false;
                }
                else
                {
                    return true;
                }

                return true;
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
                    object x = Traverse.Create(__instance).Field("state").GetValue();
                    int y = Convert.ToInt32(x);

                    if (y == 4 || y == 5 || y == 6)
                    {
                        Traverse.Create(__instance).Field("t").SetValue(90);
                        //__instance.stop_gameover_run = true;
                        return false;
                    }
                    return true;
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

            public static bool SuperNoelNoPervertDisableHpAndAbsorbDamage2(NelAttackInfo Atk, NelM2Attacker MvTarget, AbsorbManager Abm, bool penetrate, ref bool __result)
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

            public static bool SuperNoelNoPervertDisableHpAndAbsorbDamage3(nel.NelAttackInfo Atk, nel.NelM2Attacker MvTarget, nel.AbsorbManager Absorb, bool penetrate, ref bool __result)
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
            private static bool SuperNoelImmuneToOther(ref M2SerItem __result, SER ser, [HarmonyArgument("__maxt")] int maxt, int max_level, bool add_to_pre_bits)
            {
                bool cure_flag = false;
                if (ser == SER.SLEEP && ImmuneToSleep)
                {
                    cure_flag = true;
                }
                else if (ser == SER.CONFUSE && ImmuneToConfuse)
                {
                    cure_flag = true;
                }
                else if (ser == SER.PARALYSIS && ImmuneToParalysis)
                {
                    cure_flag = true;
                }
                else if (ser == SER.BURNED && ImmuneToBurn)
                {
                    cure_flag = true;
                }
                else if (ser == SER.FROZEN && ImmuneToFrozen)
                {
                    cure_flag = true;
                }
                else if (ser == SER.JAMMING && ImmuneToJamming)
                {
                    cure_flag = true;
                }
                if (cure_flag)
                {
                    __result = null;
                    return false;
                }
                else
                {
                    return true;
                }
            }

            public static List<string> NameToKeyList = new List<string>();
            private static Dictionary<string, string> NameToKey = new Dictionary<string, string>();
            private static Dictionary<string, string> NameToKeyLower = new Dictionary<string, string>();
            private static bool NameToKeyInit = false;
            public static void initNameToKey()
            {
                if (NameToKeyList.Count == 0)
                {
                    NameToKeyInit = false;
                }
                if (NameToKeyInit)
                {
                    return;
                }
                NameToKeyInit = true;
                NameToKey.Clear();
                foreach (string key in NelItem.OData.Keys)
                {
                    NameToKeyList.Add(key);
                    if (!NameToKey.ContainsKey(key))
                    {
                        NameToKey.Add(key, key);
                    }

                    string keyLower = key.ToLower();
                    if (!NameToKeyLower.ContainsKey(keyLower)) {
                        NameToKeyLower.Add(keyLower, key);
                    }
                }
            }

            public static NelItemManager IMNG = null;
            private static UiItemManageBox uiItemManageBox = null;

            public static void AddItem(string item_name, int count)
            {
                initNameToKey();

                if (IMNG == null)
                {
                    return;
                }

                if (uiItemManageBox == null)
                {
                    return;
                }

                NelItem Itm = null;
                
                if (NelItem.OData.ContainsKey(item_name))
                {
                    Itm = NelItem.OData[item_name];
                }
                else if (NameToKey != null && NameToKey.ContainsKey(item_name) &&
                    NelItem.OData.ContainsKey(NameToKey[item_name]))
                {
                    Itm = NelItem.OData[NameToKey[item_name]];
                } else
                {
                    return;
                }

                int grade;
                int new_grade = 5;
                bool retain_grade_flag;
                
                if (currentItemGrade != -1)
                {
                    new_grade = currentItemGrade;
                    retain_grade_flag = false;
                }
                else
                {
                    retain_grade_flag = true;
                }

                grade = Itm.individual_grade ? 0 : retain_grade_flag ? uiItemManageBox.get_grade_cursor() : new_grade - 1;
                
                if (count > 0)
                {
                    NelItemManager.NelItemDrop nelItemDrop = IMNG.dropManual(Itm, count, grade,
                       uiItemManageBox.Pr.x, uiItemManageBox.Pr.y, X.NIXP(-0.003f, -0.07f) * (float)CAim._XD(uiItemManageBox.Pr.aim, 1),
                        X.NIXP(-0.01f, -0.04f), null, false);
                    nelItemDrop.discarded = true;
                }
            }
            private static bool SuperNoelAddItem2(aBtn B, UiItemManageBox __instance)
            {
                if (IMNG == null)
                    IMNG = __instance.IMNG;
                if (uiItemManageBox == null)
                    uiItemManageBox = __instance;
                initNameToKey();

                return true;
            }

            private static bool SuperNoelAddItem(aBtn B, ref UiItemManageBox __instance)
            {
                if  (IMNG == null)
                    IMNG = __instance.IMNG;
                if (uiItemManageBox == null)
                    uiItemManageBox = __instance;
                initNameToKey();

                if (!addItemCmd)
                {
                    return true;
                }
                int count = currentItemAmount;
                if ((B.title != "drop" && B.title != "discard_row" && B.title != "discard_water") || count == 0) { return true; }
                string item_name = currentItemName;
                NelItem Itm;
                if (NelItem.OData.ContainsKey(item_name))
                {
                    Itm = NelItem.OData[item_name];
                }
                else if (NameToKey != null && NameToKey.ContainsKey(item_name) &&
                    NelItem.OData.ContainsKey(NameToKey[item_name]))
                {
                    Itm = NelItem.OData[NameToKey[item_name]];
                }
                else
                {
                    Itm = __instance.UsingTarget;
                }
                if ((Itm.is_precious && Itm.key != "enhancer_slot" && Itm.key != "oc_slot") || Itm.is_cache_item || Itm.is_enhancer)
                {
                    Itm = __instance.UsingTarget;
                }
                int grade;
                int new_grade = 5;
                bool retain_grade_flag;
                if (currentItemGrade != -1)
                {
                    new_grade = currentItemGrade;
                    retain_grade_flag = false;
                }
                else
                {
                    retain_grade_flag = true;
                }
                grade = Itm.individual_grade ? 0 : retain_grade_flag ? __instance.get_grade_cursor() : new_grade - 1;
                if (count > 0)
                {
                    NelItemManager.NelItemDrop nelItemDrop = __instance.IMNG.dropManual(Itm, count, grade,
                        __instance.Pr.x, __instance.Pr.y, X.NIXP(-0.003f, -0.07f) * (float)CAim._XD(__instance.Pr.aim, 1),
                        X.NIXP(-0.01f, -0.04f), null, false);
                    nelItemDrop.discarded = true;
                }
                return true;
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

            public static void initNoelPervertVars()
            {
                PervertEpItemEffect = updateVarFirst("NoelPervert", "PervertEpItemEffect");
                PervertEPDamageMultiplier = updateVarFirst("NoelPervert", "PervertEPDamageMultiplier");
                PervertEnableMultipleOrgasmForAll = updateVarFirst("NoelPervert", "PervertEnableMultipleOrgasmForAll");
                PervertEasierOrgasmWithHighEP = updateVarFirst("NoelPervert", "PervertEasierOrgasmWithHighEP");
                PervertSadismMode = updateVarFirst("NoelPervert", "PervertSadismMode");
                PervertMasochismMode = updateVarFirst("NoelPervert", "PervertMasochismMode");
                PervertEroBow = updateVarFirst("NoelPervert", "PervertEroBow");

                EPDamageMultiplier = updateVarFirstFloat("NoelPervert", "EPDamageMultiplier");
            }

            public static void updateNoelPervertVars()
            {
                if (!saveSettingsInConfig.Value)
                {
                    return;
                }

                updateVarSecond("NoelPervert", "PervertEpItemEffect", PervertEpItemEffect);
                updateVarSecond("NoelPervert", "PervertEPDamageMultiplier", PervertEPDamageMultiplier);
                updateVarSecond("NoelPervert", "PervertEnableMultipleOrgasmForAll", PervertEnableMultipleOrgasmForAll);
                updateVarSecond("NoelPervert", "PervertEasierOrgasmWithHighEP", PervertEasierOrgasmWithHighEP);
                updateVarSecond("NoelPervert", "PervertSadismMode", PervertSadismMode);
                updateVarSecond("NoelPervert", "PervertMasochismMode", PervertMasochismMode);
                updateVarSecond("NoelPervert", "PervertEroBow", PervertEroBow);

                updateVarSecondFloat("NoelPervert", "EPDamageMultiplier", EPDamageMultiplier);
            }
            public static void ExecuteHarmonyPatches()
            {
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.NoelPervert), typeof(NelItem), "Use", "NoelPervertEpItemEffect", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.NoelPervert), typeof(EpManager), "calcNormalEpErection", "NoelPervertEPDamageMultiplier", false, true);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.NoelPervert), typeof(EpManager), "checkMultipleOrgasm", "NoelPervertEnableMultipleOrgasmForAll2", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.NoelPervert), typeof(EpManager), "getLeadToOrgasmRatio", "NoelPervertEnableMultipleOrgasmForAll", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.NoelPervert), typeof(NelEnemy), "applyDamage", "NoelPervertSadismMode", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.NoelPervert), typeof(M2PrADmg), "applyHpDamageSimple", "NoelPervertMasochismMode", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.NoelPervert), typeof(EpManager), "addMasturbateCountImmediate", "NoelPervertRemoveRecord", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.NoelPervert), typeof(NelNGolemToyBow), "createBeam", "NoelPervertEroBow", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.NoelPervert), typeof(MgNGeneralBeam), "mgattr2col", "NoelPervertEroBow2", true, false);
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
            private static bool NoelPervertEnableMultipleOrgasmForAll(ref EpManager __instance, ref float __result, EPCATEG target)
            {
                PR Pr = __instance.Pr;
                Pr.Ser.Add(SER.FORBIDDEN_ORGASM);
                Pr.Ser.Add(SER.SEXERCISE);
                EpSuppressor Suppressor = Traverse.Create(__instance).Field("Suppressor").GetValue<EpSuppressor>();
                for (int i = 0; i < 30; i++)
                {
                    Suppressor.Orgasmed(target, 0);
                }
                float[] Ases_orgasmable = Traverse.Create(__instance).Field("Ases_orgasmable").GetValue<float[]>();
                float t_sage = Traverse.Create(__instance).Field("t_sage").GetValue<float>();
                float num = Ases_orgasmable[(int)target];
                int num2 = Suppressor.Sum();
                if (num2 != 0 || t_sage > 0f)
                {
                    num = X.NI(num, Ases_orgasmable[11], X.ZLINE(num2 - 2, 8f));
                }

                num += X.NI(0.08f, 0.2f, num);
                num += 0.4f;
                float orgasmableRatio = Suppressor.getOrgasmableRatio(target);
                __result = num * ((orgasmableRatio == 1f) ? 1f : (Suppressor.getOrgasmableRatio(target) - 0.004f));

                Logger.LogInfo("orgasm_result: " + __result);
                if (PervertEnableMultipleOrgasmForAll)
                {
                    __result = 4f;
                    return false;
                }
                return true;
            }

            private static bool NoelPervertEnableMultipleOrgasmForAll2(ref EpManager __instance, ref bool __result, EpAtk Atk)
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
            private static bool NoelPervertSadismMode(NelAttackInfo Atk, ref HITTYPE add_hittype, bool force)
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

            private static bool NoelPervertMasochismMode(NelAttackInfoBase Atk, out bool force, int val, bool show_damage_counter)
            {
                force = val > 0;
                //if (Atk == null || Atk.AttackFrom is PR)
                if (Atk == null || Atk.AttackFrom is PR)
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
            private static bool NoelPervertRemoveRecord(ref EpManager __instance, ref EPCATEG __result, string situation_key, int multiple_orgasm)
            {
                if (situation_key == "sadism" || situation_key == "masochism")
                {
                    __result = EPCATEG.OTHER;
                    Traverse lmc = Traverse.Create(__instance).Field("lead_to_orgasm");
                    if ((int)lmc.GetValue<EPCATEG_BITS>() < multiple_orgasm)
                    {
                        lmc.SetValue((EPCATEG_BITS)0);
                    }
                    return false;
                }
                return true;
            }

            private static bool NoelPervertEroBow(ref NelNGolemToyBow __instance)
            {
                
                if (PervertEroBow)
                {
                    MagicItem coolMg = null;
                    try
                    {
                        MgNGeneralBeam.BeamHandlerS Mh = Traverse.Create(__instance).Field("Mh").GetValue<MgNGeneralBeam.BeamHandlerS>();
                        if (Mh.isActive(out MagicItem Mg) && Mg.Atk0 != null)
                        {
                            coolMg = Mg;
                        }
                    }
                    catch (Exception ex)
                    {
                        return true;
                    }
                    if (coolMg != null && coolMg.Atk0 != null)
                    {
                        coolMg.Atk0.attr = MGATTR.ACME;
                        coolMg.Ray.HitLock(13f, null);
                    }
                    return true;
                }
                else
                {
                    return true;
                }
            }

            private static bool NoelPervertEroBow2(object __instance, MGATTR attr, out Color32 col0, out Color32 col1, out Color32 colsub)
            {
                MgNGeneralBeam _this = (MgNGeneralBeam)__instance;

                switch (attr)
                {
                    case MGATTR.FIRE:
                        col0 = C32.d2c(4294942222u);
                        col1 = C32.d2c(2004157701u);
                        colsub = C32.d2c(4282020220u);
                        break;
                    case MGATTR.ICE:
                        col0 = C32.d2c(4280340989u);
                        col1 = C32.d2c(1996823413u);
                        colsub = C32.d2c(4288439086u);
                        break;
                    case MGATTR.THUNDER:
                        col0 = C32.d2c(4293524018u);
                        col1 = C32.d2c(2001889584u);
                        colsub = C32.d2c(4281532888u);
                        break;
                    case MGATTR.ACME:
                        if (!CFG.ep_fear_mode)
                        {
                            col0 = C32.d2c(4294138793u);
                            col1 = C32.d2c(2863860369u);
                            colsub = C32.d2c(4282483037u);
                        }
                        else
                        {
                            col0 = C32.d2c(4287450239u);
                            col1 = C32.d2c(2855737444u);
                            colsub = C32.d2c(4282483037u);
                        }

                        break;
                    case MGATTR.STONE:
                        col0 = C32.d2c(4284374622u);
                        col1 = C32.d2c(2859429743u);
                        colsub = C32.d2c(4286805614u);
                        break;
                    default:
                        col0 = C32.d2c(4292927712u);
                        col1 = C32.d2c(2859429743u);
                        colsub = C32.d2c(4286805614u);
                        break;
                }

                if (PervertEroBow)
                {
                    if (attr == MGATTR.ACME)
                    {
                        col0 = new UnityEngine.Color32(243, 91, 169, 255);
                        col1 = new UnityEngine.Color(179, 10, 145, 170);
                        colsub = C32.d2c(4282483037u);
                    }
                    return false;
                }

                return true;
            }
        }

        public static class DebugMe
        {
            public static bool DebugThis = false;

            public static void initDebugMeVars()
            {
                updateVarFirst("DebugMe", "DebugThis");
            }

            public static void updateDebugMeVars()
            {
                if (!saveSettingsInConfig.Value)
                {
                    return;
                }

                updateVarSecond("DebugMe", "DebugThis", DebugThis);
            }

            public static void ExecuteHarmonyPatches()
            {
                try
                {
                    PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod.DebugMe), typeof(nel.BetobetoManager.SvTexture), "runBetobeto", "runBetobeto", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
            }

            public static bool runBetobeto(object __instance, ref bool __result, int cur_dirt, BetobetoManager BCon)
            {
                if (!DebugThis)
                {
                    return true;
                }
                Logger.LogInfo("execute runBetobeto");
                nel.BetobetoManager.SvTexture _this = (nel.BetobetoManager.SvTexture) __instance;
                bool flag1 = _this.dirt_index < 0;
                if ((UnityEngine.Object)_this.prepareTexture() == (UnityEngine.Object)null)
                {
                    __result = false;
                    return false;
                }
                if (cur_dirt <= _this.dirt_index)
                {
                    __result = !flag1;
                    return false;
                }
                if (BCon == null)
                {
                    __result = false;
                    return false;
                }
                MeshDrawer mdTemp = (MeshDrawer)BCon.GetType().GetField("MdTemp", BindingFlags.NonPublic | BindingFlags.Static).GetValue(BCon);
                RenderTexture Base = (RenderTexture)_this.GetType().GetField("Base", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_this);
                Material MtrFrozenGdtA = (Material)BCon.GetType().GetField("MtrFrozenGdtA", BindingFlags.NonPublic | BindingFlags.Static).GetValue(BCon);
                Material MtrFrozenGdtS = (Material)BCon.GetType().GetField("MtrFrozenGdtS", BindingFlags.NonPublic | BindingFlags.Static).GetValue(BCon);
                Material MtrFrozen = (Material)BCon.GetType().GetField("MtrFrozen", BindingFlags.NonPublic | BindingFlags.Static).GetValue(BCon);
                Material MtrBetoImg = (Material)BCon.GetType().GetField("MtrBetoImg", BindingFlags.NonPublic | BindingFlags.Static).GetValue(BCon);
                Material MtrStone = (Material)BCon.GetType().GetField("MtrStone", BindingFlags.NonPublic | BindingFlags.Static).GetValue(BCon);

                BetoInfo info = BCon.getInfo(_this.dirt_index);
                BetoInfo.TYPE type = info.type;
                bool flag2 = (type & BetoInfo.TYPE._SPERM) != 0;
                if (flag2)
                    type &= BetoInfo.TYPE._CATEGORY | BetoInfo.TYPE._MARUNOMI;
                if ((type & BetoInfo.TYPE._MARUNOMI) != 0)
                    type &= BetoInfo.TYPE._CATEGORY | BetoInfo.TYPE._SPERM;
                if (CFG.ui_effect_dirty == (byte)0 && !BetobetoManager.is_special_ser_type(type))
                {
                    __result = !flag1;
                    return false;
                }
                float num1 = info.level * X.ZLINE((float)CFG.ui_effect_dirty, 7f);
                float num2 = X.NI(0.66f, 1f, X.ZLINE((float)CFG.ui_effect_dirty, 7f));
                float num3 = info.scale * num2;
                float num4 = info.scale * num2;
                int frozenStoneIndex = BCon.frozen_stone_index;
                mdTemp.base_z = frozenStoneIndex < 0 || _this.dirt_index < frozenStoneIndex ? 0.125f : 0.625f;
                uint ran = BCon.GETRAN2((info.fill_id & (int)byte.MaxValue) + _this.id * 27, 1 + info.fill_id % 5 + _this.id % 4);
                UnityEngine.Graphics.SetRenderTarget(Base);
                GL.PushMatrix();
                GL.LoadOrtho();
                GL.MultMatrix(Matrix4x4.Scale(new Vector3(64f / (float)Base.width, 64f / (float)Base.height, 1f)));
                int num5 = 0;
                int num6 = 0;
                switch (type)
                {
                    case BetoInfo.TYPE.FROZEN:
                        Material _Mtr = (Material)null;
                        float num7 = (float)Base.width / 512f / info.scale;
                        float num8 = (float)Base.height / 512f / info.scale;
                        for (int index = 2; index >= 0; --index)
                        {
                            Material material;
                            switch (index)
                            {
                                case 1:
                                    material = MtrFrozenGdtA;
                                    break;
                                case 2:
                                    material = MtrFrozenGdtS;
                                    break;
                                default:
                                    material = MtrFrozen;
                                    break;
                            }
                            _Mtr = material;
                            _Mtr.SetFloat("_Level", info.level);
                            _Mtr.SetFloat("_ScaleX", num7);
                            _Mtr.SetFloat("_ScaleY", num8);
                            _Mtr.SetFloat("_ZTest", 4f);
                            if (index >= 1)
                                _Mtr.mainTexture = MTR.MIiconL.Tx;
                        }
                        _Mtr.SetColor("_Color", (UnityEngine.Color)C32.d2c(4288008150U));
                        _Mtr.SetColor("_BColor", (UnityEngine.Color)C32.d2c(4279786863U));
                        _Mtr.SetColor("_WColor", (UnityEngine.Color)C32.d2c(4292867578U));
                        RenderTexture temporary1 = RenderTexture.GetTemporary(Base.descriptor);
                        UnityEngine.Graphics.Blit((Texture)Base, temporary1, BLIT.MtrJustPaste);
                        UnityEngine.Graphics.SetRenderTarget(Base);
                        mdTemp.activate("frozen", _Mtr, true, info.Col);
                        mdTemp.initForImgAndTexture((Texture)temporary1);
                        _Mtr.SetPass(0);
                        GL.Begin(4);
                        mdTemp.Col = MTRX.ColWhite;
                        mdTemp.RectBL(0.0f, 0.0f, (float)temporary1.width, (float)temporary1.height);
                        BLIT.RenderToGLOneTask(mdTemp, mdTemp.getTriMax());
                        GL.End();
                        GL.Flush();
                        RenderTexture.ReleaseTemporary(temporary1);
                        int num9 = X.IntC((float)((double)(Base.width * Base.height) * (double)X.ZPOW(info.level - 0.4f, 0.6f) / 6000.0));
                        int num10 = X.Mx(1, (int)((double)num9 * 0.44999998807907104));
                        mdTemp.activate("frozen", MtrFrozenGdtS, true, info.Col);
                        mdTemp.Col = C32.d2c(1714631435U);
                        MtrFrozenGdtS.SetPass(0);
                        GL.Begin(4);
                        while (--num9 >= 0)
                        {
                            ran = BCon.GETRAN2(num9 * 13 + ((int)ran & (int)byte.MaxValue), num9 % 7 + ((int)ran & 7));
                            Logger.LogInfo("ran: " + ran.ToString());
                            PxlFrame pxlFrame = MTR.ANoelBreakCloth[(long)ran % (long)MTR.ANoelBreakCloth.Length];
                            pxlFrame = MTR.ANoelBreakCloth[MTR.ANoelBreakCloth.Length - 1];
                            mdTemp.initForImg(pxlFrame.getLayer(0).Img);
                            mdTemp.RotaGraph(X.RAN(ran, 453) * (float)Base.width, X.RAN(ran, 1280 /*0x0500*/) * (float)Base.height, X.NI(2.2f, 2.7f, X.RAN(ran, 1143)), X.RAN(ran, 1212) * 6.28318548f, flip: (double)X.RAN(ran, 567) < 0.5);
                            BLIT.RenderToGLOneTask(mdTemp, mdTemp.getTriMax());
                            if (--num10 == 0)
                            {
                                GL.End();
                                GL.Flush();
                                mdTemp.activate("frozen",MtrFrozenGdtA, true, info.Col);
                                MtrFrozenGdtA.SetPass(0);
                                mdTemp.Col = C32.d2c(4282104531U);
                                GL.Begin(4);
                            }
                        }
                        GL.End();
                        break;
                    case BetoInfo.TYPE.STONE_WHOLE:
                        float num11 = (float)Base.width / 512f / info.scale * 4f;
                        float num12 = (float)Base.height / 512f / info.scale * 4f;
                        Material mtrStone = MtrStone;
                        mtrStone.SetColor("_Color", (UnityEngine.Color)info.Col);
                        mtrStone.SetColor("_BColor", (UnityEngine.Color)info.Col2);
                        mtrStone.SetFloat("_ZTest", 4f);
                        mdTemp.activate("stone", mtrStone, true, MTRX.ColWhite);
                        mtrStone.SetFloat("_ScaleX", num11);
                        mtrStone.SetFloat("_ScaleY", num12);
                        mtrStone.SetFloat("_AlphaSrc", 0.0f);
                        mtrStone.SetFloat("_AlphaDest", 1f);
                        mtrStone.SetFloat("_Level", info.level);
                        RenderTexture temporary2 = RenderTexture.GetTemporary(Base.descriptor);
                        UnityEngine.Graphics.Blit((Texture)Base, temporary2, BLIT.MtrJustPaste);
                        UnityEngine.Graphics.SetRenderTarget(Base);
                        mdTemp.initForImgAndTexture((Texture)temporary2);
                        mtrStone.SetPass(0);
                        GL.Begin(4);
                        mdTemp.allocUv2(4);
                        mdTemp.Uv2(info.level, 0.0f);
                        mdTemp.RectBL(0.0f, 0.0f, (float)temporary2.width, (float)temporary2.height);
                        mdTemp.allocUv2(0, true);
                        BLIT.RenderToGLOneTask(mdTemp, mdTemp.getTriMax());
                        GL.End();
                        GL.Flush();
                        RenderTexture.ReleaseTemporary(temporary2);
                        break;
                    default:
                        float num13 = 0.0f;
                        Material mtrBetoImg = MtrBetoImg;
                        Color32 color32 = info.Col;
                        Color32 _C = info.Col2;
                        float scale = info.scale;
                        if (type == BetoInfo.TYPE.POISON_LIQUID || type == BetoInfo.TYPE.POISON_SMOKE)
                        {
                            float level = X.RAN(ran, 831) * 1.3f;
                            if ((double)level >= 0.30000001192092896)
                                level -= 0.3f;
                            float a1 = (float)_C.a;
                            float a2 = (float)color32.a;
                            _C = mdTemp.ColGrd.Set(_C).blend(4286389383U, level).setA(a1 * (float)(1.0 - (double)level * 0.6600000262260437)).C;
                            color32 = mdTemp.ColGrd.Set(color32).blend(4282072413U, level).setA(a2 * (float)(1.0 - (double)level * 0.40000000596046448)).C;
                            float num14 = (float)(1.0 + (double)level * 0.699999988079071);
                            scale *= num14;
                            num3 *= num14;
                            num4 *= num14;
                        }
                        if (flag2 && X.sensitive_level >= (byte)2)
                        {
                            _C = mdTemp.ColGrd.Set(_C).blend(2287160179U, 0.8f).C;
                            color32 = mdTemp.ColGrd.Set(color32).blend(2896201391U, 0.8f).C;
                        }
                        int num15 = (int)ran & 3;
                        float num16 = (float)((double)(Base.width * Base.height) * (double)num1 * 0.75) * X.NI(0.4f, 1f, X.ZLINE(scale));
                        float num17 = num16 * 0.35f;
                        mdTemp.activate("svnel_betobeto", mtrBetoImg, true, color32);
                        if (info.BloodReplaceCol.a > (byte)0 && CFG.blood_weaken > (byte)0)
                        {
                            num13 = X.Mx(0.125f, num16 * (float)(1.0 - (double)X.ZSINV((float)(1.0 - (double)CFG.blood_weaken / (double)CFG.BLOOD_WEAKEN_MAX)) * (double)X.NI(0.75f, 1f, X.RAN(ran, 1953))));
                            mdTemp.Col = info.BloodReplaceCol;
                        }
                        mtrBetoImg.SetColor("_BColor", (UnityEngine.Color)_C);
                        mtrBetoImg.SetFloat("_TextureScale", scale);
                        mtrBetoImg.SetFloat("_Density", (float)CFG.ui_effect_density / 10f);
                        mtrBetoImg.SetFloat("_ZTest", 4f);
                        mtrBetoImg.SetPass(0);
                        GL.Begin(4);
                        float num18 = 0.0f;
                        float num19 = 0.0f;
                        float num20 = 0.0f;
                        float num21 = 0.0f;
                        float num22 = X.RAN(ran, 2896) * (float)Base.width;
                        float num23 = X.RAN(ran, 526) * (float)Base.height;
                        float num24 = info.jumprate * X.NI(0.6f, 1f, X.RAN(ran, 1700));
                        PxlSequence pxlSequence = (PxlSequence)null;
                        switch (type)
                        {
                            case BetoInfo.TYPE.LIQUID:
                            case BetoInfo.TYPE.POISON_LIQUID:
                                pxlSequence = MTR.SqParticleSperm;
                                break;
                            case BetoInfo.TYPE.STAIN:
                                pxlSequence = MTR.SqEfSabi;
                                break;
                            case BetoInfo.TYPE.WEB_TRAPPED:
                                pxlSequence = MTR.SqParticleBetoWebTrapped;
                                num16 *= 9f;
                                mdTemp.base_z = 0.625f;
                                break;
                        }
                        while ((double)num18 < (double)num16)
                        {
                            ran = BCon.GETRAN2(num5 * 13 + ((int)ran & (int)byte.MaxValue), num5 % 44 + ((int)ran & 31 /*0x1F*/));
                            PxlImage Img = (PxlImage)null;
                            switch (type)
                            {
                                case BetoInfo.TYPE.SMOKE:
                                case BetoInfo.TYPE.POISON_SMOKE:
                                    pxlSequence = (double)num17 > (double)num18 ? MTR.SqParticleSperm : MTR.SqParticleSplash;
                                    break;
                                case BetoInfo.TYPE.CUTTED:
                                    PxlSequence sqBezierCutted = MTR.SqBezierCutted;
                                    Img = sqBezierCutted.getFrame((int)((long)ran % (long)sqBezierCutted.countFrames())).getLayer(0).Img;
                                    break;
                            }
                            PxlFrame F = (PxlFrame)null;
                            if (pxlSequence != null)
                            {
                                F = pxlSequence.getFrame((int)((long)ran % (long)pxlSequence.countFrames()));
                                Img = F.getLayer(0).Img;
                            }
                            float num25 = 1f;
                            float num26 = 1f;
                            if (num6 > 0 && (double)X.RAN(ran, 493) < (double)num24)
                                num6 = 0;
                            float num27;
                            float num28;
                            if (num6 == 0)
                            {
                                float num29 = X.RAN(ran, 1494);
                                float num30 = X.RAN(ran, 670);
                                if (((double)num29 >= 0.5 ? 1 : 0) + ((double)num30 >= 0.5 ? 2 : 0) == num15)
                                {
                                    ++num5;
                                    continue;
                                }
                                num27 = (float)Base.width * num29;
                                num28 = (float)Base.height * num30;
                                num21 = 3.14159274f * X.NI(-0.15f, 0.15f, X.RAN(ran, 510));
                            }
                            else
                            {
                                float x = X.RAN(ran, 2100) * 6.28318548f;
                                float num31 = (X.NI(12, 30, X.RAN(ran, 728)) + (num6 == 1 ? 25f : 0.0f)) * num3;
                                float num32 = X.NI(0.06f, 0.3f, X.ZPOW(X.RAN(ran, 782)));
                                num27 = num19 + num31 * X.Cos(x);
                                num28 = num20 + num31 * X.Sin(x);
                                num25 *= num32;
                                num26 *= num32;
                            }
                            float num33 = X.NI(0.75f, 2.25f, X.RAN(ran, 1530));
                            float scalex = num25 * (num3 * num33);
                            float scaley = num26 * (num4 * (num33 + X.NI(-0.1f, 0.1f, X.RAN(ran, 1478))));
                            float num34 = (float)Img.width;
                            if (type == BetoInfo.TYPE.CUTTED)
                            {
                                BetobetoManager.BzPict.PtCenterPx((float)sbyte.MinValue, 0.0f, 0.0f, (float)(20.0 + 14.0 * (double)X.RAN(ran, 2754)) * (float)X.MPF((double)X.RAN(ran, 2851) < 0.5), X.NI(8, 11, X.RAN(ran, 1629)) * 4f, X.NI(-0.06f, 0.06f, X.RAN(ran, 2989)) * 3.14159274f, 128f, 0.0f);
                                mdTemp.initForImg(Img);
                                Matrix4x4 currentMatrix = mdTemp.getCurrentMatrix();
                                float num35 = X.NI(Img.width, 140, 0.6f);
                                mdTemp.TranslateP((num27 + num22) % (float)Base.width, (num28 + num23) % (float)Base.height, true).Rotate(num21 + X.NI(-0.03f, 0.03f, X.RAN(ran, 2252)) * 3.14159274f, true).Scale((float)((double)num35 * (double)scalex * (1.0 / 64.0) / 4.0), 1f, true);
                                BetobetoManager.BzPict.drawTo(mdTemp, 0.0f, 0.0f, (float)((double)scaley * (double)Img.height * 2.0 * (1.0 / 64.0)), true);
                                mdTemp.setCurrentMatrix(currentMatrix);
                                num34 = num35 * 1.5f;
                            }
                            else
                                mdTemp.RotaPF((num27 + num22) % (float)Base.width, (num28 + num23) % (float)Base.height, scalex, scaley, X.RAN(ran, 2342) * 6.28318548f, F, (ran & 1U) > 0U);
                            num19 = num27;
                            num20 = num28;
                            ++num5;
                            ++num6;
                            float num36 = num34 * scalex * (float)Img.height * scaley;
                            num18 += num36;
                            if ((double)num13 > 0.0)
                            {
                                num13 -= num36;
                                if ((double)num13 <= 0.0)
                                    mdTemp.Col = color32;
                            }
                            BLIT.RenderToGLOneTask(mdTemp, mdTemp.getTriMax());
                        }
                        BLIT.RenderToGLOneTask(mdTemp, mdTemp.getTriMax());
                        GL.End();
                        break;
                }
                UnityEngine.Graphics.SetRenderTarget((RenderTexture)null);
                GL.PopMatrix();
                GL.Flush();
                __result = ++_this.dirt_index == cur_dirt && !flag1;
                return false;
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
        bool foldoutTornClothes = false;
        bool foldoutChangeOutfit = false;
        bool foldoutUnlockables = false;
        bool foldoutAddItem = false;
        bool foldoutAddItemList = false;
        string weatherText = "0110111";
        int money = 0;
        int level = 0;
        int dangerLevel = 0;
        int grade = 0;

        private void DrawCheatWindow(int windowID)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUILayout.Label($"KeyCode to open / close the cheat menu: {keyCodeToOpenCloseTheCheatsMenu}");

            GUILayout.Label("To change the key code, please look in");

            GUILayout.Label("BepInEx/config/com.wolfitdm.AliceInGradleDemosaicMod.cfg");

            toggleButton("Save Settings In Config", saveSettingsInConfig.Value, () =>
            {
                saveSettingsInConfig.Value = !saveSettingsInConfig.Value;
            });

            toggleButton("Is Menu Default Opened", configMenuDefaultOpen.Value, () =>
            {
                configMenuDefaultOpen.Value = !configMenuDefaultOpen.Value;
            });

            toggleButton("Use unsafe functions", SetGameValues.USE_UNSAFE_FUNCS, () =>
            {
                SetGameValues.USE_UNSAFE_FUNCS = !SetGameValues.USE_UNSAFE_FUNCS;
            });

            SetGameValues.updateSetGameValuesVars();

            actionButton("Find and Export all textures", () =>
            {
                SetGameValues.FindAndLogTextures();
            });

            // Spieler-Cheats
            foldoutPlayer = EditorLikeFoldout(foldoutPlayer, "Uncensor + Debug + Money Cheat");

            if (foldoutPlayer)
            {
                string[] vars = ["UNCENSOR", "DEBUG", "DEBUGNOCFG", "DEBUGSUPERSENSITIVE", "DEBUGANNOUNCE", "DEBUGNOSND", "DEBUGPLAYER", "DEBUGALLSKILL",
                                 "DEBUGNOEVENT", "DEBUGNOVOICE", "DEBUGRELOADMTR", "DEBUGTIMESTAMP", "DEBUGALBUMUNLOCK", "DEBUGSTABILIZE_DRAW",
                                 "DEBUGMIGHTY", "DEBUGNODAMAGE", "DEBUGWEAK", "DEBUGBENCHMARK", "DEBUGSUPERCYCLONE", "DEBUGENG_MODE", "DEBUGSPEFFECT"
                                ];

                toggleButton("UNCENSOR RESTROOM", Debug.UNCENSOR_RESTROOM, () =>
                {
                    Debug.UNCENSOR_RESTROOM = !Debug.UNCENSOR_RESTROOM;

                    SetGameValues.uncensorRestRoomPee(Debug.UNCENSOR_RESTROOM);
                });

                toggleButton("UNCENSOR 2WEEK ATTACK", Debug.UNCENSOR_2WEEK_ATTACK, () =>
                {
                    Debug.UNCENSOR_2WEEK_ATTACK = !Debug.UNCENSOR_2WEEK_ATTACK;

                    SetGameValues.uncensor2WeekAttack(Debug.UNCENSOR_2WEEK_ATTACK);
                });

                toggleButton("UNCENSOR EGG REMOVE ATTACK", Debug.UNCENSOR_EGG_REMOVE_ATTACK, () =>
                {
                    Debug.UNCENSOR_EGG_REMOVE_ATTACK = !Debug.UNCENSOR_EGG_REMOVE_ATTACK;

                    SetGameValues.uncensorEggRemoveAttack(Debug.UNCENSOR_EGG_REMOVE_ATTACK);
                });

                toggleButton("UNCENSOR DAMAGE FDOWN ATTACK VERSION 1", Debug.UNCENSOR_DAMAGE_FDOWN_VERSION1, () =>
                {
                    Debug.UNCENSOR_DAMAGE_FDOWN_VERSION1 = !Debug.UNCENSOR_DAMAGE_FDOWN_VERSION1;

                    SetGameValues.uncensorDamageFdownAttackVersion1(Debug.UNCENSOR_DAMAGE_FDOWN_VERSION1);
                });

                toggleButton("UNCENSOR DAMAGE FDOWN ATTACK VERSION 2", Debug.UNCENSOR_DAMAGE_FDOWN_VERSION2, () =>
                {
                    Debug.UNCENSOR_DAMAGE_FDOWN_VERSION2 = !Debug.UNCENSOR_DAMAGE_FDOWN_VERSION2;

                    SetGameValues.uncensorDamageFdownAttackVersion2(Debug.UNCENSOR_DAMAGE_FDOWN_VERSION2);
                });

                toggleButton("UNCENSOR STAND NORMAL (MINIROCK VERSION)", Debug.UNCENSOR_STAND_NORMAL, () =>
                {
                    Debug.UNCENSOR_STAND_NORMAL = !Debug.UNCENSOR_STAND_NORMAL;

                    SetGameValues.uncensorStandNormal(Debug.UNCENSOR_STAND_NORMAL);
                });

                toggleButton("UNCENSOR STAND NORMAL VERSION 2 (MINIROCK VERSION NUDE)", Debug.UNCENSOR_STAND_NORMAL_VERSION2, () =>
                {
                    Debug.UNCENSOR_STAND_NORMAL_VERSION2 = !Debug.UNCENSOR_STAND_NORMAL_VERSION2;

                    SetGameValues.uncensorStandNormalVersion2(Debug.UNCENSOR_STAND_NORMAL_VERSION2);
                });

                toggleButton("UNCENSOR STAND NORMAL VERSION 3 (MINIROCK VERSION FULL NUDE)", Debug.UNCENSOR_STAND_NORMAL_VERSION3, () =>
                {
                    Debug.UNCENSOR_STAND_NORMAL_VERSION3 = !Debug.UNCENSOR_STAND_NORMAL_VERSION3;

                    SetGameValues.uncensorStandNormalVersion3(Debug.UNCENSOR_STAND_NORMAL_VERSION3);
                });

                toggleButton("UNCENSOR STAND WEAK (MORE BOOBS)", Debug.UNCENSOR_STAND_WEAK, () =>
                {
                    Debug.UNCENSOR_STAND_WEAK = !Debug.UNCENSOR_STAND_WEAK;

                    SetGameValues.uncensorStandWeak(Debug.UNCENSOR_STAND_WEAK);
                });

                toggleButton("UNCENSOR STAND WEAK VERSION 2 (MORE BOOBS AND VAGINA)", Debug.UNCENSOR_STAND_WEAK_VERSION2, () =>
                {
                    Debug.UNCENSOR_STAND_WEAK_VERSION2 = !Debug.UNCENSOR_STAND_WEAK_VERSION2;

                    SetGameValues.uncensorStandWeakVersion2(Debug.UNCENSOR_STAND_WEAK_VERSION2);
                });

                toggleButton("UNCENSOR DAMAGE A", Debug.UNCENSOR_DAMAGE_A, () =>
                {
                    Debug.UNCENSOR_DAMAGE_A = !Debug.UNCENSOR_DAMAGE_A;

                    SetGameValues.uncensorDamageA(Debug.UNCENSOR_DAMAGE_A);
                });

                toggleButton("UNCENSOR DAMAGE B", Debug.UNCENSOR_DAMAGE_B, () =>
                {
                    Debug.UNCENSOR_DAMAGE_B = !Debug.UNCENSOR_DAMAGE_B;

                    SetGameValues.uncensorDamageB(Debug.UNCENSOR_DAMAGE_B);
                });

                foreach (string var in vars)
                {
                    if (GUILayout.Button(Debug.st(var)))
                    {
                        Debug.updateVar(var);
                    }
                }

                Debug.updateDebugVars();

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

                toggleButton("ImmuneToSleep", SuperNoel.ImmuneToSleep, () =>
                {
                    SuperNoel.ImmuneToSleep = !SuperNoel.ImmuneToSleep;
                });

                toggleButton("ImmuneToConfuse", SuperNoel.ImmuneToConfuse, () =>
                {
                    SuperNoel.ImmuneToConfuse = !SuperNoel.ImmuneToConfuse;
                });

                toggleButton("ImmuneToParalysis", SuperNoel.ImmuneToParalysis, () =>
                {
                    SuperNoel.ImmuneToParalysis = !SuperNoel.ImmuneToParalysis;
                });

                toggleButton("ImmuneToBurn", SuperNoel.ImmuneToBurn, () =>
                {
                    SuperNoel.ImmuneToBurn = !SuperNoel.ImmuneToBurn;
                });

                toggleButton("ImmuneToFrozen", SuperNoel.ImmuneToFrozen, () =>
                {
                    SuperNoel.ImmuneToFrozen = !SuperNoel.ImmuneToFrozen;
                });

                toggleButton("ImmuneToJamming", SuperNoel.ImmuneToJamming, () =>
                {
                    SuperNoel.ImmuneToJamming = !SuperNoel.ImmuneToJamming;
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

                SuperNoel.updateSuperNoelVars();
            }

            foldoutAddItemList = EditorLikeFoldout(foldoutAddItemList, "Super Noel Add Item List");

            if (foldoutAddItemList)
            {
                SuperNoel.currentItemAmount = (int)slider("Item Amount", (float)SuperNoel.currentItemAmount, 1, 1000);
                SuperNoel.currentItemGrade = (int)slider("Item Grade", (float)SuperNoel.currentItemGrade, -1, 100);
                SuperNoel.initNameToKey();
                foreach (string item in SuperNoel.NameToKeyList)
                {
                    actionButton($"Add Item {item} / {SuperNoel.currentItemAmount} / {SuperNoel.currentItemGrade}", () =>
                    {
                        SuperNoel.currentItemName = item;
                        SuperNoel.addItemCmd = false;

                        SuperNoel.AddItem(SuperNoel.currentItemName, SuperNoel.currentItemAmount);
                    });
                }
            }

            foldoutAddItem = EditorLikeFoldout(foldoutAddItem, "Super Noel Add Item");

            if (foldoutAddItem)
            {
                SuperNoel.currentItemName = GUILayout.TextField(SuperNoel.currentItemName, 100, GUILayout.Width(100));
                SuperNoel.currentItemAmount = (int)slider("Item Amount", (float)SuperNoel.currentItemAmount, 1, 1000);
                SuperNoel.currentItemGrade = (int)slider("Item Grade", (float)SuperNoel.currentItemGrade, -1, 100);

                actionButton($"Add Item {SuperNoel.currentItemName} / {SuperNoel.currentItemAmount} / {SuperNoel.currentItemGrade}", () =>
                {
                    SuperNoel.addItemCmd = false;

                    SuperNoel.AddItem(SuperNoel.currentItemName, SuperNoel.currentItemAmount);
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

                NoelPervert.updateNoelPervertVars();

                toggleButton("Always Full Break Clothes", DebugMe.DebugThis, () =>
                {
                    DebugMe.DebugThis = !DebugMe.DebugThis;
                });
            }

            foldoutChangeOutfit = EditorLikeFoldout(foldoutChangeOutfit, "Noel Change Outfit");

            if (foldoutChangeOutfit)
            {
                string[] outfits = ["TORNED", "BABYDOLL", "DOJO", "NORMAL"];

                foreach (string f in outfits)
                {
                    actionButton(f, () =>
                    {
                        SetGameValues.changeOutFit(f);
                    });
                }
            }

            foldoutUnlockables = EditorLikeFoldout(foldoutUnlockables, "Noel Unlockables");

            if (foldoutUnlockables)
            {
                actionButton("Unlock Bench Menu", () =>
                {
                    SetGameValues.unlockBenchMenu();
                });

                actionButton("Unlock Noel Can Pee", () =>
                {
                    SetGameValues.unlockNoelCanPee();
                });

                actionButton("Unlock Noel Have Much Sex Experiences", () =>
                {
                    SetGameValues.unlockNoelHaveSex();
                });
            }

            foldoutSetDanger = EditorLikeFoldout(foldoutSetDanger, "Moel Set Danger Level");

            if (foldoutSetDanger)
            {
                dangerLevel = (int)slider("DangerLevel", dangerLevel, 0, 50);
                grade = (int)slider("DangerLevelGrade", grade, 0, 1000);
                actionButton($"Set Danger Level {dangerLevel} {grade}", () =>
                {
                    SetGameValues.SetDangerLevel(dangerLevel, grade);
                });
            }

            foldoutOtherSetMoney = EditorLikeFoldout(foldoutOtherSetMoney, "Noel Set Money");

            if (foldoutOtherSetMoney)
            {
                money = (int)slider("money", money, 0, 999999);
                actionButton($"Set Money {money}", () =>
                {
                    SetGameValues.SetMoney(money, 999999);
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

            foldoutTornClothes = EditorLikeFoldout(foldoutTornClothes, "Torn Clothes (Not Safe)");

            if (foldoutTornClothes)

            {
                level = (int)slider("level", level, -999, 999);
                actionButton($"Torn Clothes {level}", () =>
                {
                    SetGameValues.tornClothes(level);
                });
            }

            GUILayout.EndScrollView();
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
                    PatchHarmonyMethodUnity(type, "drawToMesh", "drawToMesh", true, false, new Type[] { typeof(Camera) });
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

                try
                {
                    PatchHarmonyMethodUnity(ftMosaic, "getSensitiveOrMosaicRect", "getSensitiveOrMosaicRect", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
            }

            try
            {
                Type[] params1 = new Type[] { typeof(Matrix4x4).MakeByRefType(), typeof(int), typeof(MeshAttachment).MakeByRefType(), typeof(Spine.Slot).MakeByRefType() };
                Type[] params2 = new Type[] { typeof(Matrix4x4).MakeByRefType(), typeof(int), typeof(MeshAttachment).MakeByRefType(), typeof(Spine.Slot).MakeByRefType(), typeof(SpineViewer) };
                try
                {
                    PatchHarmonyMethodUnity(typeof(AnimateCutin), "getSensitiveOrMosaicRect", "getSensitiveOrMosaicRect", true, false, params1);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnity(typeof(AnimateCutin), "countMosaic", "countMosaic2", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }

                try
                {
                    PatchHarmonyMethodUnity(typeof(UIPictureBodyData), "countMosaic", "countMosaic", true, false);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(UIPictureBodyData), "getSensitiveOrMosaicRect", "getSensitiveOrMosaicRect", true, false, params1);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
                try
                {
                    PatchHarmonyMethodUnity(typeof(UIPictureBodySpine), "getSensitiveOrMosaicRect", "getSensitiveOrMosaicRect2", true, false, params2);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
            } catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

            SuperNoel.ExecuteHarmonyPatches();
            NoelPervert.ExecuteHarmonyPatches();
            DebugMe.ExecuteHarmonyPatches();
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

        public static bool callDestruct(object __instance, bool set = false)
        {
            try
            {
                Type instanceType = __instance.GetType();
                Type thisType = null;

                if (set)
                {
                    return false;
                }

                if (ftMosaicType != null && instanceType == ftMosaicType)
                {
                    thisType = ftMosaicType;
                    ((MosaicShower)__instance).destruct();
                    return true;
                }
                else if (instanceType == typeof(MosaicShower))
                {
                    thisType = typeof(MosaicShower);
                    ((MosaicShower)__instance).destruct();
                    return true;
                }
                return false;
            } catch
            {
                return false; 
            }
        }

        public static bool drawToMeshEx(object __instance)
        {
            if (!Debug.UNCENSOR)
            {
                setEnabledToFalse(__instance, true);
                setUseMosaicToFalse(__instance, true);
                callDestruct(__instance, true);
                return true;
            }
            setEnabledToFalse(__instance);
            setUseMosaicToFalse(__instance);
            callDestruct(__instance);
            return false;
        }

        public static bool drawToMesh(object __instance, Camera Cam)
        {
            if (!Debug.UNCENSOR)
            {
                setEnabledToFalse(__instance, true);
                setUseMosaicToFalse(__instance, true);
                callDestruct(__instance, true);
                return true;
            }
            setUseMosaicToFalse(__instance);
            setEnabledToFalse(__instance);
            callDestruct(__instance);
            return false;
        }
        public static bool FnDrawMosaic(object __instance, object XCon, ProjectionContainer JCon, Camera Cam, ref bool __result)
        {
            if (!Debug.UNCENSOR)
            {
                setEnabledToFalse(__instance, true);
                setUseMosaicToFalse(__instance, true);
                callDestruct(__instance, true);
                return true;
            }
            setUseMosaicToFalse(__instance);
            setEnabledToFalse(__instance);
            callDestruct(__instance);
            __result = false;
            return false;
        }
        public static bool setTarget(object __instance, IMosaicDescriptor _Targ, bool force)
        {
            if (!Debug.UNCENSOR)
            {
                setEnabledToFalse(__instance, true);
                setUseMosaicToFalse(__instance, true);
                callDestruct(__instance, true);
                return true;
            }
            setEnabledToFalse(__instance);
            setUseMosaicToFalse(__instance);
            callDestruct(__instance);

            return true;
        }

        public static bool countMosaic(object __instance, ref int __result, bool only_on_sensitive)
        {
            if (!Debug.UNCENSOR)
            {
                setEnabledToFalse(__instance, true);
                setUseMosaicToFalse(__instance, true);
                callDestruct(__instance, true);
                return true;
            }
            setUseMosaicToFalse(__instance);
            setEnabledToFalse(__instance);
            callDestruct(__instance);

            __result = 0;
            return false;
        }
        public static bool countMosaic2(object __instance, ref int __result, bool only_sensitive)
        {
            if (!Debug.UNCENSOR)
            {
                setEnabledToFalse(__instance, true);
                setUseMosaicToFalse(__instance, true);
                callDestruct(__instance, true);
                return true;
            }
            setUseMosaicToFalse(__instance);
            setEnabledToFalse(__instance);
            callDestruct(__instance);

            __result = 0;
            return false;
        }
        public static bool getSensitiveOrMosaicRect(
               ref Matrix4x4 Out,
               int id,
               ref MeshAttachment OutMesh,
               ref Spine.Slot BelongSlot, ref bool __result, object __instance)
        {
            if (!Debug.UNCENSOR)
            {
                setEnabledToFalse(__instance, true);
                setUseMosaicToFalse(__instance, true);
                callDestruct(__instance, true);
                return true;
            }
            setEnabledToFalse(__instance);
            setUseMosaicToFalse(__instance);
            callDestruct(__instance);

            __result = false;
            return false;
        }

        public static bool getSensitiveOrMosaicRect2(
            ref Matrix4x4 Out,
            int id,
            ref MeshAttachment OutMesh,
            ref Spine.Slot BelongSlot,
            SpineViewer TargetSpv, ref bool __result, object __instance)
        {
            if (!Debug.UNCENSOR)
            {
                setEnabledToFalse(__instance, true);
                setUseMosaicToFalse(__instance, true);
                callDestruct(__instance, true);
                return true;
            }
            setEnabledToFalse(__instance);
            setUseMosaicToFalse(__instance);
            callDestruct(__instance);

            __result = false;
            return false;
        }

        public static void PatchHarmonyMethodUnity(Type originalClass, string originalMethodName, string patchedMethodName, bool usePrefix, bool usePostfix, Type[] parameters = null)
        {
            PatchHarmonyMethodUnityClass(typeof(AliceInGradleDemosaicMod), originalClass, originalMethodName, patchedMethodName, usePrefix, usePostfix, parameters);
        }

        public static void PatchHarmonyMethodUnityClass(Type patchClass, Type originalClass, string originalMethodName, string patchedMethodName, bool usePrefix, bool usePostfix, Type[] parameters = null)
        {
            string uniqueId = "com.wolfitdm.AliceInGradleDemosaicMod";
            Type uniqueType = patchClass;

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