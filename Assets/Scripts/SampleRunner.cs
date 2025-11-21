using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Sample;
using UnityEngine;

public class SampleRunner : MonoBehaviour
{
    private void Start()
    {
        // ちゃんとキャッチされ、例外出力もなされる
        RunUniTask().Forget();
        RunUniTask().Forget(Debug.LogException);
        
        // 何にも例外でない (async void の問題点)
        RunTask();
        
        // 例外は出る
        RunTask().ForgetOK0();
        RunTask().ForgetOK1();
        RunTask().ForgetOK2();
        
        // await終了後に個別のThreadPool内部のThreadに移動する
        Task.Run(async () =>
        {
            // ThreadID: 1220 (別スレッド1)
            Debug.Log("Task:Before await: " + Thread.CurrentThread.ManagedThreadId);
        
            await Task.Delay(100);
            // ThreadID: 1240 (別スレッド2)
            Debug.Log("Task:After1 await: " + Thread.CurrentThread.ManagedThreadId);
            
            await Task.Delay(100);
            // ThreadID: 1250 (別スレッド3)
            Debug.Log("Task:After2 await: " + Thread.CurrentThread.ManagedThreadId);
        });

        // await終了後にmainThreadに移動する
        UniTask.RunOnThreadPool(async () =>
        {
            // ThreadID: 1234 (別スレッド1)
            Debug.Log("UniTask:Before await: " + Thread.CurrentThread.ManagedThreadId);
        
            await UniTask.Delay(100);
            // ThreadID: 1 (main)
            Debug.Log("UniTask:After1 await: " + Thread.CurrentThread.ManagedThreadId);
            
            await UniTask.Delay(100);
            // ThreadID: 1 (main)
            Debug.Log("UniTask:After2 await: " + Thread.CurrentThread.ManagedThreadId);
        }).Forget();
    }

    private async UniTask RunUniTask()
    {
        Debug.Log("RunUniTask: Start");
        throw new System.Exception("Exception on RunUniTask");
        Debug.Log("RunUniTask: End");
    }

    private async Task RunTask()
    {
        Debug.Log("RunTask: Start");
        throw new System.Exception("Exception on RunTask");
        Debug.Log("RunTask: End");
    }
}
