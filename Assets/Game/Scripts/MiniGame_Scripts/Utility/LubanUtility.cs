using System;
using System.Collections.Generic;
using System.IO;
using cfg;
using QFramework;
using Newtonsoft;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Luban;

public class LubanUtility : IUtility
{
    private string gameConfDir = "Assets/Game/MiniGame_Res/Config"; // 替换为gen.bat中outputDataDir指向的目录
    public Tables Tables;
    
    public async UniTask Initialize()
    {
        
// #if UNITY_EDITOR
//         // // 初始化逻辑
//         // Tables = new cfg.Tables(file =>
//         // {
//         //     var text = File.ReadAllText($"{gameConfDir}/{file}.json");
//         //     return JArray.Parse(text);
//         // });
//         //
//         // return;
// #else
//
// #endif
        
        try
        {
            var handle = GameArchitecture.Interface.GetUtility<YooassetUtility>().LoadConfigsAsync();

            var results = await handle; // 等待异步加载完成
            Dictionary<string, byte[]> dict = new Dictionary<string, byte[]>();
            foreach (var r in results)
            {
                dict[r.name] = r.bytes;
            }
            Tables = new cfg.Tables(file => new ByteBuf(dict[file]));
        }
        catch (Exception e)
        {
            throw; // TODO 处理异常
        }
    }
        
}