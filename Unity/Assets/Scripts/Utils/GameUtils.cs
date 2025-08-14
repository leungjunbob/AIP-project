using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace SplendorUnity.Utils
{
    /// <summary>
    /// 游戏工具类，提供各种游戏相关的工具方法
    /// </summary>
    public static class GameUtils
    {
        /// <summary>
        /// 测试模式标志
        /// </summary>
        public static bool IsTestMode { get; set; } = false;

        /// <summary>
        /// 抛出未定义方法异常
        /// </summary>
        public static void RaiseNotDefined()
        {
            var stackFrame = new StackFrame(1);
            var method = stackFrame.GetMethod();
            var fileName = stackFrame.GetFileName();
            var lineNumber = stackFrame.GetFileLineNumber();

            string message = $"*** Method not implemented: {method?.Name} at line {lineNumber} of {fileName}";
            
            if (!IsTestMode)
            {
                UnityEngine.Debug.LogError(message);
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
            }
            else
            {
                throw new NotImplementedException($"Method {method?.Name} not implemented");
            }
        }
        /// <summary>
        /// 深度复制对象
        /// </summary>
        public static object DeepCopy(object obj)
        {
            if (obj == null)
                return null;

            var type = obj.GetType();

            // 基本类型和字符串直接返回
            if (type.IsPrimitive || type == typeof(string))
                return obj;

            // List
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var newList = (System.Collections.IList)Activator.CreateInstance(type);
                foreach (var item in (System.Collections.IEnumerable)obj)
                {
                    newList.Add(DeepCopy(item));
                }
                return newList;
            }

            // Dictionary
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var newDict = (System.Collections.IDictionary)Activator.CreateInstance(type);
                foreach (System.Collections.DictionaryEntry kvp in (System.Collections.IDictionary)obj)
                {
                    newDict.Add(DeepCopy(kvp.Key), DeepCopy(kvp.Value));
                }
                return newDict;
            }

            // Array
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                if (elementType != null)
                {
                    var array = (Array)obj;
                    var newArray = Array.CreateInstance(elementType, array.Length);
                    for (int i = 0; i < array.Length; i++)
                    {
                        var value = array.GetValue(i);
                        if (value != null)
                        {
                            newArray.SetValue(DeepCopy(value), i);
                        }
                    }
                    return newArray;
                }
            }

            // 对于实现了IDeepCopyable接口的对象
            if (obj is SplendorUnity.Core.IDeepCopyable deepCopyable)
            {
                return deepCopyable.DeepCopy();
            }

            // 对于其他复杂对象，暂时返回原对象（浅复制）
            return obj;
        }
        
        /// <summary>
        /// 验证动作是否在合法动作列表中
        /// </summary>
        public static bool IsValidAction(object action, List<object> validActions)
        {
            if (action == null || validActions == null)
                return false;
                
            return validActions.Any(validAction => validAction.Equals(action));
        }
        
        /// <summary>
        /// 生成随机种子列表
        /// </summary>
        public static List<int> GenerateSeedList(int seed, int count)
        {
            var rand = new System.Random(seed);
            var seedList = new List<int>();
            
            for (int i = 0; i < count; i++)
            {
                seedList.Add(rand.Next(0, unchecked((int)1e10)));
            }
            
            return seedList;
        }
        
        /// <summary>
        /// 计算玩家分数
        /// </summary>
        public static double CalculateScore(object gameState, int playerId)
        {
            // 这里需要根据具体的游戏状态类型来实现
            // 暂时返回0
            return 0.0;
        }
        
        /// <summary>
        /// 检查游戏是否结束
        /// </summary>
        public static bool IsGameEnded(object gameState)
        {
            // 这里需要根据具体的游戏状态类型来实现
            // 暂时返回false
            return false;
        }
        
        /// <summary>
        /// 获取当前玩家索引
        /// </summary>
        public static int GetCurrentPlayerIndex(object gameState)
        {
            // 这里需要根据具体的游戏状态类型来实现
            // 暂时返回0
            return 0;
        }
        
        /// <summary>
        /// 获取下一个玩家索引
        /// </summary>
        public static int GetNextPlayerIndex(int currentIndex, int totalPlayers)
        {
            return (currentIndex + 1) % totalPlayers;
        }
        
        /// <summary>
        /// 格式化时间显示
        /// </summary>
        public static string FormatTime(float seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }
        
        /// <summary>
        /// 格式化分数显示
        /// </summary>
        public static string FormatScore(double score)
        {
            return score.ToString("F1");
        }
        
        /// <summary>
        /// 检查两个对象是否相等
        /// </summary>
        public static bool AreEqual(object obj1, object obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;
            if (obj1 == null || obj2 == null)
                return false;
                
            return obj1.Equals(obj2);
        }
        
        /// <summary>
        /// 安全地获取对象属性值
        /// </summary>
        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return null;
                
            try
            {
                var property = obj.GetType().GetProperty(propertyName);
                return property?.GetValue(obj);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"获取属性 {propertyName} 失败: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 安全地设置对象属性值
        /// </summary>
        public static bool SetPropertyValue(object obj, string propertyName, object value)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
                return false;
                
            try
            {
                var property = obj.GetType().GetProperty(propertyName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(obj, value);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"设置属性 {propertyName} 失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 记录游戏日志
        /// </summary>
        public static void LogGameEvent(string eventType, string message, object data = null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logMessage = $"[{timestamp}] [{eventType}] {message}";
            
            if (data != null)
            {
                logMessage += $" - Data: {data}";
            }
            
            UnityEngine.Debug.Log(logMessage);
        }
        
        /// <summary>
        /// 记录游戏警告
        /// </summary>
        public static void LogGameWarning(string eventType, string message, object data = null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logMessage = $"[{timestamp}] [{eventType}] WARNING: {message}";
            
            if (data != null)
            {
                logMessage += $" - Data: {data}";
            }
            
            UnityEngine.Debug.LogWarning(logMessage);
        }
        
        /// <summary>
        /// 记录游戏错误
        /// </summary>
        public static void LogGameError(string eventType, string message, Exception exception = null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logMessage = $"[{timestamp}] [{eventType}] ERROR: {message}";
            
            if (exception != null)
            {
                logMessage += $" - Exception: {exception.Message}";
            }
            
            UnityEngine.Debug.LogError(logMessage);
        }
    }
}