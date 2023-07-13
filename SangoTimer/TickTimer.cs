using System;
using System.Collections.Concurrent;
using System.Threading;

//Developer: SangonomiyaSakunovi

namespace SangoTimer
{
    public class TickTimer : BaseTimer
    {
        private readonly DateTime _initDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        private readonly ConcurrentDictionary<int, TickTask> _tickTaskDict;
        private readonly ConcurrentQueue<TickTaskPack> _tickTaskPackQueue;
        private readonly bool _isSetHandleThread;

        private const string _taskIdLock = "TickTimer_TaskIdLock";

        private readonly Thread _timerThread;

        public TickTimer(int interval = 0, bool isSetHandleThreads = true)
        {
            _tickTaskDict = new ConcurrentDictionary<int, TickTask>();
            _isSetHandleThread = isSetHandleThreads;
            if (_isSetHandleThread)
            {
                _tickTaskPackQueue = new ConcurrentQueue<TickTaskPack>();
            }
            if (interval != 0)
            {
                _timerThread = new Thread(new ThreadStart(() =>
                {
                    try
                    {
                        while (true)
                        {
                            UpdateTask();
                            Thread.Sleep(interval);
                        }
                    }
                    catch (ThreadAbortException ex)
                    {
                        LogWarnCallBack?.Invoke($"Tick Thread Abort: {ex}.");
                    }
                }));
                _timerThread.Start();
            }
        }

        public override int AddTask(uint delayTime, Action<int> taskCallBack, Action<int> cancelCallBack, int count = 1)
        {
            int taskId = GenerateTaskId();
            double startTime = GetUTCMilliseconds();
            double destinationTime = startTime + delayTime;
            TickTask tickTask = new TickTask(taskId, delayTime, count, startTime, destinationTime, taskCallBack, cancelCallBack);

            if (_tickTaskDict.TryAdd(taskId, tickTask))
            {
                return taskId;
            }
            else
            {
                LogWarnCallBack?.Invoke($"key:{taskId} already exist in TickTaskDict.");
                return -1;
            }
        }

        public override bool DeleteTask(int taskId)
        {
            if (_tickTaskDict.TryRemove(taskId, out TickTask tickTask))
            {
                if (_isSetHandleThread && tickTask.CancelCallBack != null)
                {
                    _tickTaskPackQueue.Enqueue(new TickTaskPack(taskId, tickTask.CancelCallBack));
                }
                else
                {
                    tickTask.CancelCallBack?.Invoke(taskId);
                }
                return true;
            }
            else
            {
                LogWarnCallBack?.Invoke($"key:{taskId} remove TickTask failed.");
                return false;
            }
        }

        public override void Reset()
        {
            if (!_tickTaskPackQueue.IsEmpty)
            {
                LogWarnCallBack?.Invoke("TickTaskPackQueue is not empty.");
            }
            _tickTaskDict.Clear();
            _taskId = 0;
            if (_timerThread != null)
            {
                _timerThread.Abort();
            }
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
                    if (!_tickTaskDict.ContainsKey(_taskId))
                    {
                        return _taskId;
                    }
                }
            }
        }

        public void UpdateTask()
        {
            double currentTime = GetUTCMilliseconds();
            foreach (var item in _tickTaskDict)
            {
                TickTask tickTask = item.Value;
                if (currentTime < tickTask.DestinationTime) { continue; }
                tickTask.LoopIndex++;
                if (tickTask.Count > 0)
                {
                    tickTask.Count--;
                    if (tickTask.Count == 0)
                    {
                        DoneTask(tickTask.TaskId);
                    }
                    else
                    {
                        tickTask.DestinationTime = tickTask.StartTime + tickTask.DelayTime * (tickTask.LoopIndex + 1);
                        CallTaskCallBack(tickTask.TaskId, tickTask.TaskCallBack);
                    }
                }
                else
                {
                    tickTask.DestinationTime = tickTask.StartTime + tickTask.DelayTime * (tickTask.LoopIndex + 1);
                    CallTaskCallBack(tickTask.TaskId, tickTask.TaskCallBack);
                }
            }
        }

        public void HandleTask()
        {
            while(_tickTaskPackQueue != null && _tickTaskPackQueue.Count > 0)
            {
                if (_tickTaskPackQueue.TryDequeue(out TickTaskPack tickTaskPack))
                {
                    tickTaskPack.ProxyCallBack(tickTaskPack.TaskId);
                }
                else
                {
                    LogErrorCallBack?.Invoke("TickTaskPackQueue Dequeue Data Error.");
                }
            }
        }

        private void CallTaskCallBack(int taskId, Action<int> taskCallBack)
        {
            if (_isSetHandleThread && taskCallBack != null)
            {
                _tickTaskPackQueue.Enqueue(new TickTaskPack(taskId, taskCallBack));
            }
            else
            {
                taskCallBack?.Invoke(taskId);
            }
        }

        private void DoneTask(int taskId)
        {
            if (_tickTaskDict.TryRemove(taskId, out TickTask tickTask))
            {
                CallTaskCallBack(taskId, tickTask.TaskCallBack);
                tickTask.TaskCallBack = null;
            }
            else
            {
                LogWarnCallBack?.Invoke($"Remove taskId: {taskId} in taskDict failed");
            }
        }

        private double GetUTCMilliseconds()
        {
            TimeSpan timeSpan = DateTime.UtcNow - _initDateTime;
            return timeSpan.TotalMilliseconds;
        }

        private class TickTask
        {
            public int TaskId;
            public uint DelayTime;
            public int Count;
            public double StartTime;
            public double DestinationTime;
            public Action<int> TaskCallBack;
            public Action<int> CancelCallBack;
            public ulong LoopIndex;

            public TickTask(int taskId, uint delayTime, int count, double startTime, double destinationTime, Action<int> taskCallBack, Action<int> cancelCallBack)
            {
                TaskId = taskId;
                DelayTime = delayTime;
                Count = count;
                StartTime = startTime;
                DestinationTime = destinationTime;
                TaskCallBack = taskCallBack;
                CancelCallBack = cancelCallBack;
                LoopIndex = 0;
            }
        }

        private class TickTaskPack
        {
            public int TaskId;
            public Action<int> ProxyCallBack;

            public TickTaskPack(int taskId, Action<int> proxyCallBack)
            {
                TaskId = taskId;
                ProxyCallBack = proxyCallBack;
            }
        }
    }
}
