using System.Collections.Concurrent;
using FreeRedis;

namespace Daily.CoreSwim.Distributed
{
    /// <summary>
    /// 单个任务模型
    /// </summary>
    internal class DistributedTaskCoordinate(RedisClient client, ICoreSwim coreSwim)
    {
        private string _taskKey;

        private string _taskLockKey;

        internal void InitializeSingleTask(string taskKey)
        {
            _taskKey = GenerateRedisKey(taskKey);
            _taskLockKey = GenerateRedisKey($"{taskKey}_lock");

            if (!client.Exists(taskKey))
            {
                InternalInit(taskKey);
            }
            else
            {
                var value = client.Get<int>(taskKey);
                SingeTaskCache.TaskNumbers.TryAdd(taskKey, value);
            }
        }

        private void InternalInit(string taskKey)
        {
            client.Set(taskKey, 0);
            SingeTaskCache.TaskNumbers.TryRemove(taskKey, out _);
            SingeTaskCache.TaskNumbers.TryAdd(taskKey, 0);
        }

        public async Task<bool> GrabTheTaskAsync()
        {
            var isRun = false;
            //分布式锁拦截
            await LockAsync(_taskLockKey, 1, async run =>
            {
                var flag = SingeTaskCache.TaskNumbers.TryGetValue(_taskKey, out var value);
                if (run && flag)
                {
                    if (!await client.ExistsAsync(_taskKey))
                    {
                        InternalInit(_taskKey);
                    }

                    var rTask = await client.GetAsync<long>(_taskKey);
                    if (rTask > value)
                    {
                        //不能执行,当前任务+1
                        var newValue = value + 1;
                        SingeTaskCache.TaskNumbers.TryUpdate(_taskKey, newValue, value);
                        coreSwim.Config.Logger.Info<DistributedTaskCoordinate>($"{_taskKey}已经被其他节点执行.");
                    }
                    else if (rTask == value)
                    {
                        var newValue = value + 1;
                        await client.SetAsync(_taskKey, newValue);
                        SingeTaskCache.TaskNumbers.TryUpdate(_taskKey, newValue, value);
                        try
                        {
                            isRun = true;
                            coreSwim.Config.Logger.Info<DistributedTaskCoordinate>($"{_taskKey}在本节点执行成功..");
                        }
                        catch
                        {
                            coreSwim.Config.Logger.Info<DistributedTaskCoordinate>($"{_taskKey}在本节点执行错误..");
                        }
                    }
                    else if (rTask < value)
                    {
                        if (!await client.ExistsAsync(_taskKey))
                        {
                            InternalInit(_taskKey);
                        }

                        //不能执行,指定为当前任务
                        await client.SetAsync(_taskKey, value);
                    }
                }
            }, false);
            return isRun;
        }

        public async Task LockAsync(string distributedLockKey, int timeoutSeconds, Func<bool, Task> action,
            bool autoDelay = true)
        {
            using var lockController = client.Lock(distributedLockKey, timeoutSeconds, autoDelay);
            await action.Invoke(lockController != null);
        }

        private string GenerateRedisKey(string text)
        {
            return $"daily_coreSwim_{text}";
        }
    }
}