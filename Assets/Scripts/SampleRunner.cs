using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Mono.Cecil;
using Sample;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class SampleRunner : MonoBehaviour
{
    /// <summary>
    /// Task,UniTaskの基本的な違い
    ///
    /// Task
    /// - スケジューラ: SynchronizationContext (UnitySynchronizationContext)
    /// - 別スレッド実行: Task.Run() 
    /// - デフォルト実行場所: 別スレッド
    /// - Forget: なし (自作必要)
    /// - 投げっぱなし: バグる
    /// - GC: 頻発
    /// - メモリ: スレッド確保で頻発
    ///
    /// UniTask
    /// - スケジューラ: UnityEngine.PlayerLoop
    /// - 別スレッド実行: UniTask.RunOnThreadPool() 
    /// - デフォルト実行場所: PlayerLoop (メインスレッド)
    /// - Forget: あり 
    /// - 投げっぱなし: UnityEngine.Debug.LogExceptionに通知
    /// - GC: 最小限
    /// - メモリ: 同一スレッドで最小
    //// </summary>
    private void Start()
    {
        // ちゃんとキャッチされ、例外出力もなされる
        RunUniTask().Forget();
        RunUniTask().Forget(Debug.LogException);
        
        // 何にも例外でない. 観測できない (async void の問題点)
        RunTask();
        
        // 例外は出る
        RunTask().ForgetOK0();
        RunTask().ForgetOK1();
        RunTask().ForgetOK2();
        
        // Taskはawait終了後に個別のThreadPool内部のThreadに移動する (ややこしい)
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

        // UniTaskはawait終了後にmainThreadに移動する (ルールがシンプル)
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

    private async UniTaskVoid UsefulSample()
    {
        // Coroutine連携が簡単
        var request = UnityWebRequest.Get("https://google.com");
        await request.SendWebRequest(); // IEnumeratorで定義したCoroutineをそのままUniTask実行できる
        var resource = await Resources.LoadAsync<GameObject>("Sample");
        
        // Coroutineとのキャンセル統合も用意
        var token = this.GetCancellationTokenOnDestroy(); // OnDestroyで自動キャンセル
        await request.SendWebRequest().ToUniTask(cancellationToken: token); // キャンセル連携も簡単

        // Unity似合わせた時間処理も簡単 (1frameだけ待つ処理も簡単)
        await UniTask.Yield();
        await UniTask.NextFrame();
        await UniTask.DelayFrame(1);
        
        // UniTaskはmainThread実行なのでasync内部でのUI操作が安心して行える
        TMP_Text text = GetComponentInChildren<TMP_Text>();
        if (text != null) text.text = "Hello World!";
        
        // Unityコンポーネントとの関数連携が簡単
        Collision collision = await this.GetAsyncCollisionEnterTrigger().OnCollisionEnterAsync(token);
        UnityEngine.UI.Button button =  GetComponent<UnityEngine.UI.Button>();
        await button.OnClickAsync(token);
        
        // その他便利関数多数
        await UniTask.WaitUntil(() => transform.position.y > 0);
        await UniTask.WaitUntilValueChanged(transform, x => x.position.y);
        // ...
    }
}
