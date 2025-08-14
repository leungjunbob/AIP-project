using UnityEngine;

namespace SplendorUnity.Tests
{
    /// <summary>
    /// 测试编译是否成功的简单脚本
    /// </summary>
    public class TestCompilation : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("TestCompilation: 开始测试编译...");
            
            // 测试是否可以创建BaseAgent引用
            SplendorUnity.AI.BaseAgent agent = null;
            Debug.Log("TestCompilation: BaseAgent引用创建成功");
            
            Debug.Log("TestCompilation: 编译成功！所有脚本都没有语法错误。");
        }
    }
}

