using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
public class ProcessingModuleSynchronous : ProcessingModule
{
    private void Update()
    {
        if (isProcessing)
        {
            TryProcess();
        }
    }
}
