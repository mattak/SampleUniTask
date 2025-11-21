using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Sample
{
    public static class TaskExtension
    {
        public static async void ForgetOK0(this Task task)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Debug.Log($"ThreadID: {Thread.CurrentThread.ManagedThreadId}");
                Debug.LogException(ex);
            }
        }

        public static async void ForgetOK1(this Task task)
        {
            try
            {
                // 待ち合わせをせずにタスク実行する. コンテキストをぶった切る
                await task.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Debug.Log($"ThreadID: {Thread.CurrentThread.ManagedThreadId}");
                Debug.LogException(ex);
            }
        }

        public static async void ForgetOK2(this Task task)
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