using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

//Developer: SangonomiyaSakunovi

namespace SangoTimer
{
    public class ASyncTimer : BaseTimer
    {
        private readonly ConcurrentDictionary<int, ASyncTask> _asyncTaskDict;
        private readonly ConcurrentQueue<ASyncTaskPack> _asyncTaskPackQueue;
        private readonly bool _isSetHandleThread;

        private const string _taskIdLock = "ASyncTimer_TaskIdLock";

        public ASyncTimer(bool isSetHandleThreads)
        {
            _isSetHandleThread = isSetHandleThreads;
            _asyncTaskDict = new ConcurrentDictionary<int, ASyncTask>();
            if (_isSetHandleThread)
            {
                _asyncTaskPackQueue = new ConcurrentQueue<ASyncTaskPack>();
            }
        }

        public override int AddTask(uint delayTime, Action<int> taskCallBack, Action<int> cancelCallBack, int count = 1)
        {
            int taskId = GenerateTaskId();
            ASyncTask asyncTask = new ASyncTask(taskId, delayTime, count, taskCallBack, cancelCallBack);
            RunTaskInThreadPool(asyncTask);

            if (_asyncTaskDict.TryAdd(taskId, asyncTask))
            {
                return taskId;
            }
            else
            {
                LogWarnCallBack?.Invoke($"key:{taskId} already exist in ASyncTaskDict.");
                return -1;
            }
        }

        public override bool DeleteTask(int taskId)
        {
            if (_asyncTaskDict.TryRemove(taskId,out ASyncTask asyncTask))
            {
                LogInfoCallBack?.Invoke($"Remove taskId: {asyncTask.TaskId} task in ASyncTaskDict success.");
                asyncTask.CancellationTokenSource.Cancel();
                if (_isSetHandleThread && asyncTask.CancelCallBack != null)
                {
                    _asyncTaskPackQueue.Enqueue(new ASyncTaskPack(asyncTask.TaskId, asyncTask.CancelCallBack));
                }
                else
                {
                    asyncTask.CancelCallBack?.Invoke(asyncTask.TaskId);
                }
                return true;
            }
            else
            {
                LogErrorCallBack?.Invoke($"Remove taskId: {asyncTask.TaskId} ASyncTask in ASyncTaskDict failed.");
                return false;
            }
        }

        public override void Reset()
        {
            if (_asyncTaskPackQueue !=null && !_asyncTaskPackQueue.IsEmpty)
            {
                LogWarnCallBack?.Invoke("ASyncTaskPackQueue is not empty.");
            }
            _asyncTaskDict.Clear();
            _taskId = 0;
        }

        protected override int GenerateTaskId()
        {
            lock (_taskIdLock)
            {
                while (true)
                {
                    ++_taskId;
                    if (_taskId == int.MaxValue)
                    {
                        _taskId = 0;
                    }
                    if (!_asyncTaskDict.ContainsKey(_taskId))
                    {
                        return _taskId;
                    }
                }
            }
        }

        public void HandleTask()
        {
            while(_asyncTaskPackQueue != null && _asyncTaskPackQueue.Count > 0)
            {
                if (_asyncTaskPackQueue.TryDequeue(out ASyncTaskPack asyncTaskPack))
                {
                    asyncTaskPack.ProxyCallBack(asyncTaskPack.TaskId);
                }
                else
                {
                    LogErrorCallBack?.Invoke("ASyncTaskPackQueue Dequeue Data Error.");
                }
            }
        }

        private void RunTaskInThreadPool(ASyncTask asyncTask)
        {
            Task.Run(async () =>
            {
                if (asyncTask.Count > 0)
                {
                    do
                    {
                        asyncTask.Count--;
                        asyncTask.LoopIndex++;
                        int delayTime = (int)(asyncTask.DelayTime + asyncTask.FixDelta);
                        if (delayTime > 0)
                        {
                            await Task.Delay(delayTime, asyncTask.CancellationToken);
                        }
                        TimeSpan timeSpan = DateTime.UtcNow - asyncTask.StartTime;
                        asyncTask.FixDelta = (int)(asyncTask.DelayTime * asyncTask.LoopIndex - timeSpan.TotalMilliseconds);
                        CallTaskCallBack(asyncTask);
                    } while (asyncTask.Count > 0);
                }
                else
                {
                    while (true)
                    {
                        asyncTask.LoopIndex++;
                        int delayTime = (int)(asyncTask.DelayTime + asyncTask.FixDelta);
                        if (delayTime > 0)
                        {
                            await Task.Delay(delayTime, asyncTask.CancellationToken);
                        }
                        TimeSpan timeSpan = DateTime.UtcNow - asyncTask.StartTime;
                        asyncTask.FixDelta = (int)(asyncTask.DelayTime * asyncTask.LoopIndex - timeSpan.TotalMilliseconds);
                        CallTaskCallBack(asyncTask);
                    }
                }
            });
        }

        private void CallTaskCallBack(ASyncTask asyncTask)
        {
            if (_isSetHandleThread && asyncTask.TaskCallBack != null)
            {
                _asyncTaskPackQueue.Enqueue(new ASyncTaskPack(asyncTask.TaskId, asyncTask.TaskCallBack));
            }
            else
            {
                asyncTask.TaskCallBack?.Invoke(asyncTask.TaskId);
            }
            if (asyncTask.Count == 0)
            {
                if (_asyncTaskDict.TryRemove(asyncTask.TaskId,out ASyncTask tempTask))
                {
                    LogInfoCallBack?.Invoke($"TaskId: {asyncTask.TaskId} has Run to Done.");
                }
                else
                {
                    LogErrorCallBack?.Invoke($"Remove TaskId: {asyncTask.TaskId} ASyncTask in taskDict failed.");
                }
            }
        }

        private class ASyncTask
        {
            public int TaskId;
            public uint DelayTime;
            public int Count;
            public DateTime StartTime;
            public Action<int> TaskCallBack;
            public Action<int> CancelCallBack;
            public ulong LoopIndex;
            public int FixDelta;

            public CancellationTokenSource CancellationTokenSource;
            public CancellationToken CancellationToken;

            public ASyncTask(int taskId, uint delayTime, int count, Action<int> taskCallBack, Action<int> cancelCallBack)
            {
                TaskId = taskId;
                DelayTime = delayTime;
                Count = count;
                TaskCallBack = taskCallBack;
                CancelCallBack = cancelCallBack;
                StartTime = DateTime.UtcNow;
                LoopIndex = 0;
                FixDelta = 0;

                CancellationTokenSource = new CancellationTokenSource();
                CancellationToken = CancellationTokenSource.Token;
            }
        }

        private class ASyncTaskPack
        {
            public int TaskId;
            public Action<int> ProxyCallBack;

            public ASyncTaskPack(int taskId, Action<int> proxyCallBack)
            {
                TaskId = taskId;
                ProxyCallBack = proxyCallBack;
            }
        }
    }
}
