using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

/**
**该题目校招岗位可以不作答，社招需要作答。**

按照要求在 {@link Q3.onStartBtnClick} 中编写一段异步任务处理逻辑，具体执行步骤如下：
1. 调用 {@link Q3.loadConfig} 加载配置文件，获取资源列表
2. 根据资源列表调用 {@link Q3.loadFile} 加载资源文件
3. 资源列表中的所有文件加载完毕后，调用 {@link Q3.initSystem} 进行系统初始化
4. 系统初始化完成后，打印日志

附加要求
1. 加载文件时，需要做并发控制，最多并发 3 个文件
2. 加载文件时，需要添加超时控制，超时时间为 3 秒
3. 加载文件失败时，需要对单文件做 backoff retry 处理，重试次数为 3 次
4. 对错误进行捕获并打印输出
*/

public class Q3 : MonoBehaviour
{
    private string[] mAllFiles;
    public async void OnStartBtnClick()
    {
        // TODO: 请在此处开始作答
        bool success = await LoadConfigFile();
        if (!success)
        {
            return;
        }

        bool loadAllFileSuccess = await LoadAllFile();
        if (loadAllFileSuccess)
        {
            await InitSystem();
            Debug.Log("初始化完成");
        }
        else
        {
            Debug.LogError("资源加载不完整。无法调用初始化");
        }
    }

    async Task<bool> LoadConfigFile()
    {
        int count = 4;
        for (int i = 0; i < count; i++)
        {
            try
            {
                Task<string[]>  loadconfigTask = LoadConfig();
                var task = await Task.WhenAny(loadconfigTask, Task.Delay(3000));
                if (task == loadconfigTask)
                {
                    mAllFiles =  loadconfigTask.Result;
                    return true;
                }
                await Task.Delay(100 * count);
            }
            catch (Exception e)
            {
                Debug.LogError("加载文件失败" + e.Message);
                await Task.Delay(100 * count);
            }
        }
        return false;
    }

  
    //加载所有文件。
    async Task<bool> LoadAllFile()
    {
        int loadReulstCount = 0;
        int loadSuccessCount = 0;
        int currentLoadingCount = 0;
        int maxLoadingCount = 3;

        var allTaskQuene = new Queue<string>();
        var runningTask = new List<Task<(bool,string)>>();
        for (int i = 0; i < mAllFiles.Length; i++)
        {
            allTaskQuene.Enqueue(mAllFiles[i]);
        }
        
        while (allTaskQuene.Count > 0 || runningTask .Count > 0 )
        {
            while (runningTask.Count < maxLoadingCount && allTaskQuene.Count > 0)
            {
                string filename = allTaskQuene.Dequeue();
                Task<(bool,string)> task = LoadOneFile(filename);
                runningTask.Add(task);
            }
            Task<(bool,string)> complateTask = await Task.WhenAny(runningTask);
            runningTask.Remove(complateTask);
            if (complateTask.Result.Item1)
            {
                Debug.Log($"文件加载成功{complateTask.Result.Item2}");
                loadSuccessCount++;
            }
            else
            {
                Debug.Log($"文件重试3次以后还是 加载失败{complateTask.Result.Item2}");
            }

            loadReulstCount++;
        }   

        if (loadSuccessCount == mAllFiles.Length)
        {
            return true;
        }

        return false;
    }
    async Task<(bool,string)> LoadOneFile(string file)
    {
        //首次加载算一次。如果失败重试3次
        int count = 4;
        for (int i = 0; i < count; i++)
        {
            try
            {
                Task loadconfigTask = LoadFile(file);
                var task = await Task.WhenAny(loadconfigTask, Task.Delay(3000));
                if (task == loadconfigTask)
                {
                    await loadconfigTask;
                    return (true,file);
                }
                Debug.Log($"加载超时{file} 第{i} 次");
            }
            catch (Exception e)
            {
                Debug.LogError("加载文件失败" + e.Message);
            }
            await Task.Delay(100 * count);
        }
        return (false,file);
    }
    
    
    
    // #region 以下是辅助测试题而写的一些 mock 函数，请勿修改

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <returns>文件列表</returns>
    public async Task<string[]> LoadConfig()
    {
        Debug.Log("load config start");
        await Task.Delay(1000);
        if (Random.value > 0.01f)
        {
            Debug.Log("load config success");
            string[] files = new string[100];
            for (int i = 0; i < 100; i++)
            {
                files[i] = $"file-{i}";
            }
            return files;
        }
        else
        {
            Debug.Log("load config failed");
            throw new System.Exception("Load config failed");
        }
    }

    /// <summary>
    /// 加载文件
    /// </summary>
    /// <param name="file">文件名</param>
    /// <returns></returns>
    public async Task LoadFile(string file)
    {
        Debug.Log($"load file start: {file}");
        await Task.Delay(Random.Range(1000, 5000));
        if (Random.value > 0.01f)
        {
            Debug.Log($"load file success: {file}");
        }
        else
        {
            Debug.Log($"load file failed: {file}");
            throw new System.Exception($"Load file failed: {file}");
        }
    }

    /// <summary>
    /// 初始化系统
    /// </summary>
    /// <returns></returns>
    public async Task InitSystem()
    {
        Debug.Log("init system start");
        await Task.Delay(1000);
        Debug.Log("init system success");
    }

    // #endregion
}
