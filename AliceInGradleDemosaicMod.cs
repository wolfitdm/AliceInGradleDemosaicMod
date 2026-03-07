using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.Mono;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using System.Reflection;
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

        public static Type MyGetTypeUnityEngine(string originalClassName)
        {
            return Type.GetType(originalClassName + ",UnityEngine");
        }

        private static string pluginKey = "General.Toggles";

        public static bool enableMe = false;


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

            try
            {
                PatchHarmonyMethodUnity(typeof(MosaicShower), "drawToMesh", "drawToMesh", true, false);
            } catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

            try
            {
                PatchHarmonyMethodUnity(typeof(MosaicShower), "drawToMesh", "drawToMeshEx", true, false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

            try
            {
                PatchHarmonyMethodUnity(typeof(MosaicShower), "FnDrawMosaic", "FnDrawMosaic", true, false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

            try
            {
                PatchHarmonyMethodUnity(typeof(MosaicShower), "FnDrawMosaic", "FnDrawMosaicEx", true, false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

            try
            {
                PatchHarmonyMethodUnity(typeof(MosaicShower), "setTarget", "setTarget", true, false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }

            try
            {
                PatchHarmonyMethodUnity(typeof(IMosaicDescriptor), "countMosaic", "countMosaic", true, false);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
            }
        }

        public static bool drawToMesh(ref bool __use_mosaic, Camera cam)
        {
            __use_mosaic = false;
            return false;
        }

        public static bool drawToMeshEx(Camera cam)
        {
            return false;
        }

        public static bool FnDrawMosaic(ref bool __use_mosaic, object XCon, ProjectionContainer JCon, Camera Cam)
        {
            __use_mosaic = false;
            return false; 
        }
        public static bool FnDrawMosaicEx(object XCon, ProjectionContainer JCon, Camera Cam)
        {
            return false;
        }

        public static bool setTarget(ref bool __use_mosaic, IMosaicDescriptor _Targ, bool force)
        {
            __use_mosaic = false;
            return true;
        }

        public static bool countMosaic(ref int __result, bool only_on_sensitive)
        {
            __result = 0;
            return true;
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