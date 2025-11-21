using System;
using System.Threading.Tasks;
using UnityEngine;

namespace Sample
{
    public static class TaskExtension
    {
        public static async void Forget1(this Task task)
        {
            try
            {
                // 待ち合わせをせずにタスク実行する
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        public static async void Forget2(this Task task)
        {
            try
            {
                // 失敗時の例外を出力する
                await task.ContinueWith(
                    x => { Debug.LogException(x.Exception); },
                    TaskContinuationOptions.OnlyOnFaulted
                );
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}