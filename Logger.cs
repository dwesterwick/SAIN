using BepInEx.Logging;
using EFT.Communications;
using EFT.UI;
using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace SAIN
{
    public static class Logger
    {
        private static float _nextNotification;
        private static readonly ManualLogSource SAINLogger = BepInEx.Logging.Logger.CreateLogSource("SAIN");

        public static void LogDebug(object data, string methodName = null)
            => createLogMessage(LogLevel.Debug, data, methodName);

        public static void LogInfo(object data, string methodName = null)
            => createLogMessage(LogLevel.Info, data, methodName);

        public static void LogWarning(object data, string methodName = null)
            => createLogMessage(LogLevel.Warning, data, methodName);

        public static void LogError(object data, string methodName = null)
            => createLogMessage(LogLevel.Error, data, methodName);

        public static void NotifyInfo(object data, ENotificationDurationType duration = ENotificationDurationType.Default)
            => NotifyMessage(data, duration, ENotificationIconType.Note);

        public static void NotifyDebug(object data, ENotificationDurationType duration = ENotificationDurationType.Default)
            => NotifyMessage(data, duration, ENotificationIconType.Note, Color.gray);

        public static void NotifyWarning(object data, ENotificationDurationType duration = ENotificationDurationType.Default)
            => NotifyMessage(data, duration, ENotificationIconType.Alert, Color.yellow);

        public static void NotifyError(object data, ENotificationDurationType duration = ENotificationDurationType.Long)
            => NotifyMessage(data, duration, ENotificationIconType.Alert, Color.red, true);

        public static void LogAndNotifyInfo(object data, ENotificationDurationType duration = ENotificationDurationType.Default)
        {
            createLogMessage(LogLevel.Info, data);
            NotifyMessage(data, duration, ENotificationIconType.Note);
        }

        public static void LogAndNotifyDebug(object data, ENotificationDurationType duration = ENotificationDurationType.Default)
        {
            createLogMessage(LogLevel.Debug, data);
            NotifyMessage(data, duration, ENotificationIconType.Note, Color.gray);
        }

        public static void LogAndNotifyWarning(object data, ENotificationDurationType duration = ENotificationDurationType.Default)
        {
            createLogMessage(LogLevel.Warning, data);
            NotifyMessage(data, duration, ENotificationIconType.Alert, Color.yellow);
        }

        public static void LogAndNotifyError(object data, ENotificationDurationType duration = ENotificationDurationType.Long)
        {
            createLogMessage(LogLevel.Error, data);
            string message = CreateErrorMessage(data);
            NotificationManagerClass.DisplayMessageNotification(message, duration, ENotificationIconType.Alert, Color.red);
        }

        public static void NotifyMessage(object data,
            ENotificationDurationType durationType = ENotificationDurationType.Default,
            ENotificationIconType iconType = ENotificationIconType.Default,
            UnityEngine.Color? textColor = null, bool Error = false)
        {
            if (_nextNotification < Time.time && SAINPlugin.DebugMode) {
                _nextNotification = Time.time + 0.1f;
                string message = Error ? CreateErrorMessage(data) : data.ToString();
                NotificationManagerClass.DisplayMessageNotification(message, durationType, iconType, textColor);
            }
        }

        private static string CreateErrorMessage(object data)
        {
            StackTrace stackTrace = new StackTrace(2);
            int max = Mathf.Clamp(stackTrace.FrameCount, 0, 10);
            for (int i = 0; i < max; i++) {
                MethodBase method = stackTrace.GetFrame(i)?.GetMethod();
                Type type = method?.DeclaringType;
                if (type != null && type.DeclaringType != typeof(Logger)) {
                    string errorString = $"[{type} : {method}]: ERROR: {data}";
                    return errorString;
                }
            }
            return data.ToString();
        }

        private static void createLogMessage(LogLevel level, object data, string methodName = null)
        {
            string result = createLogString(level, data, methodName);
            sendLog(level, result);
        }

        private static string createLogString(LogLevel level, object data, string methodName)
        {
            if (!methodName.IsNullOrEmpty()) {
                return $"[{methodName}]: {data}";
            }
            Type declaringType = null;
            string methodsString = string.Empty;
            if (level != LogLevel.Debug) {
                int max = GetMaxFrames(level);
                StackTrace stackTrace = new StackTrace(2);
                int count = 0;
                for (int i = 0; i < stackTrace.FrameCount; i++) {
                    if (count >= max) {
                        break;
                    }
                    var method = stackTrace.GetFrame(i).GetMethod();

                    if (method.DeclaringType == typeof(Logger)) continue;

                    if (declaringType == null) {
                        declaringType = method.DeclaringType;
                    }

                    if (!methodsString.IsNullOrEmpty()) {
                        methodsString = "." + methodsString;
                    }

                    methodsString = $"{method.Name}()" + methodsString;
                    count++;
                }
                methodsString = $"[{methodsString}]:";
            }

            if (methodsString.IsNullOrEmpty()) {
                return data.ToString();
            }
            else if (declaringType != null) {
                return $"[{declaringType}] : [{methodsString}] : [{data}]";
            }
            else {
                return $"[{methodsString}] : [{data}]";
            }
        }

        private static void sendLog(LogLevel level, string result)
        {
            switch (level) {
                case LogLevel.Debug:
                case LogLevel.Fatal:
                    //UnityEngine.Debug.LogError(result);
                    if (MonoBehaviourSingleton<PreloaderUI>.Instance?.Console != null) {
                        //ConsoleScreen.LogError(result);
                    }
                    break;

                default:
                    //UnityEngine.Debug.Log(result);
                    break;
            }
            SAINLogger.Log(level, result);
        }

        private static int GetMaxFrames(LogLevel level)
        {
            switch (level) {
                case LogLevel.Debug:
                    return 1;

                case LogLevel.Info:
                    return 2;

                case LogLevel.Warning:
                    return 3;

                case LogLevel.Error:
                    return 4;

                case LogLevel.Fatal:
                    return 5;

                default:
                    return 1;
            }
        }
    }
}