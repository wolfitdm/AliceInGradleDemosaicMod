using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using UnityEngine;
using XX;

namespace AliceInGradleDemosaicMod
{
    [BepInPlugin("com.wolfitdm.AliceInGradleDemosaicMod", "AliceInGradleDemosaicMod Plugin", "1.0.0.0")]
    public class AliceInGradleDemosaicMod : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger;

        private static ConfigEntry<bool> configEnableMe;

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
            enableMe = configEnableMe.Value;

            PatchAllHarmonyMethods();

            Logger.LogInfo($"Plugin AliceInGradleDemosaicMod BepInEx is loaded!");
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

        public static bool setUseMosaicToFalse(object __instance)
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
                    field.SetValue(__instance, false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
            return false;
        }

        public static bool setEnabledToFalse(object __instance)
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
                ftMosaicTypeEnabled.SetValue(thisType, false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
            return false;
        }

        public static bool drawToMeshEx(object __instance)
        {
            setUseMosaicToFalse(__instance);
            setEnabledToFalse(__instance);
 
            return false;
        }

        public static bool drawToMesh(object __instance, Camera Cam)
        {
            setUseMosaicToFalse(__instance);
            setEnabledToFalse(__instance);
            return false;
        }
        public static bool FnDrawMosaic(object __instance, object XCon, ProjectionContainer JCon, Camera Cam)
        {
            setUseMosaicToFalse (__instance);
            setEnabledToFalse(__instance);
            return false; 
        }
        public static bool setTarget(object __instance, IMosaicDescriptor _Targ, bool force)
        {
            setUseMosaicToFalse(__instance);
            return true;
        }

        public static bool countMosaic(object __instance, ref int __result, bool only_on_sensitive)
        {
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