using UnityEngine;

public class TestModuleAsynchronous : ProcessingModuleAsynchronous
{
    public string prefix = "Asynchronous: "; // 额外的文本处理参数

    public override string ProcessMessage(string message)
    {
        // 使用前缀对消息进行处理
        return $"{prefix}{message} with parameter {parameter}";
    }
}