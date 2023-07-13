using System;
using System.Collections.Generic;

//Developer: SangonomiyaSakunovi

namespace SangoTimer
{
    public class FrameTimer : BaseTimer
    {
        private readonly Dictionary<int, FrameTask> _frameTaskDict;
        private List<int> _frameTaskList;

        private ulong _currentFrame;

        private const string _taskIdLock = "FrameTimer_TaskIdLock";

        public FrameTimer(ulong currentFrameId = 0)
        {
            _currentFrame = currentFrameId;
            _frameTaskDict = new Dictionary<int, FrameTask>();
            _frameTaskList = new List<int>();
        }

        public override int AddTask(uint delayTime, Action<int> taskCallBack, Action<int> cancelCallBack, int count = 1)
        {
            int taskId = GenerateTaskId();
            ulong destinationFrame = _currentFrame + delayTime;
            FrameTask frameTask = new FrameTask(taskId, delayTime, count, destinationFrame, taskCallBack, cancelCallBack);
            if (_frameTaskDict.ContainsKey(taskId))
            {
                LogWarnCallBack?.Invoke($"key:{taskId} already exist in FrameTaskDict.");
                return -1;
            }
            else
            {
                _frameTaskDict.Add(taskId, frameTask);
                return taskId;
            }
        }

        public override bool DeleteTask(int taskId)
        {
            if (_frameTaskDict.TryGetValue(taskId, out FrameTask frameTask))
            {
                if (_frameTaskDict.Remove(taskId))
                {
                    frameTask.CancelCallBack?.Invoke(taskId);
                    return true;
                }
                else
                {
                    LogErrorCallBack?.Invoke($"Remove taskId:{taskId} in FrameTaskDict failed.");
                    return false;
                }
            }
            else
            {
                LogWarnCallBack?.Invoke($"TaskId:{taskId} is not exist in FrameTaskDict.");
                return false;
            }
        }

        public override void Reset()
        {
            _frameTaskDict.Clear();
            _frameTaskList.Clear();
            _currentFrame = 0;
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
                    if (!_frameTaskDict.ContainsKey(_taskId))
                    {
                        return _taskId;
                    }
                }
            }
        }

        public void UpdateTask()
        {
            _currentFrame++;
            _frameTaskList.Clear();
            foreach (var item in _frameTaskDict)
            {
                FrameTask frameTask = item.Value;
                if (frameTask.DestinationFrame <= _currentFrame)
                {
                    frameTask.TaskCallBack?.Invoke(frameTask.TaskId);
                    frameTask.DestinationFrame += frameTask.DelayFrame;
                    frameTask.Count--;
                    if (frameTask.Count == 0)
                    {
                        _frameTaskList.Add(frameTask.TaskId);
                    }
                }
            }
            for (int i = 0; i < _frameTaskList.Count; i++)
            {
                if (_frameTaskDict.Remove(_frameTaskList[i]))
                {
                    LogInfoCallBack?.Invoke($"Task taskId:{_frameTaskList[i]} has Run to Done.");
                }
                else
                {
                    LogErrorCallBack?.Invoke($"Remove taskId:{_frameTaskList[i]} task in FrameTaskDict failed.");
                }
            }
        }

        private class FrameTask
        {
            public int TaskId;
            public uint DelayFrame;
            public int Count;
            public ulong DestinationFrame;
            public Action<int> TaskCallBack;
            public Action<int> CancelCallBack;

            public FrameTask(int taskId, uint delayFrame, int count, ulong destinationFrame, Action<int> taskCallBack, Action<int> cancelCallBack)
            {
                TaskId = taskId;
                DelayFrame = delayFrame;
                Count = count;
                DestinationFrame = destinationFrame;
                TaskCallBack = taskCallBack;
                CancelCallBack = cancelCallBack;
            }
        }
    }
}
