using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ProcessingModuleAsynchronous : ProcessingModule
{
    public override void StartProcessing()
    {
        base.StartProcessing();
        Task.Run(ProcessLoop); // 启动后台任务
    }

    private async Task ProcessLoop()
    {
        while (isProcessing)
        {
            TryProcess();
            await Task.Delay(50); // 控制频率，避免资源占用过高
        }
    }
}
