using UnityEngine;

namespace SplendorUnity.Utils
{
    /// <summary>
    /// 常量定义类
    /// </summary>
    public static class Constants
    {
        // 游戏常量
        public const bool FREEDOM = false; // 是否惩罚超时/非法动作，调试用
        public const int WARMUP = 15;      // 首次行动的预热时间
        
        // 游戏配置
        public const int DEFAULT_SEED = 1;
        public const double DEFAULT_TIME_LIMIT = 1.0;
        public const int DEFAULT_WARNING_LIMIT = 3;
        public const int MAX_SEED_COUNT = 1000;
        
        // 默认玩家名称
        public static readonly string[] DEFAULT_PLAYER_NAMES = { "Alice", "Bob" };
        
        // 错误类型
        public const string TIMEOUT_ACTION = "timeout";
        public const string ILLEGAL_ACTION = "illegal";
    }
}