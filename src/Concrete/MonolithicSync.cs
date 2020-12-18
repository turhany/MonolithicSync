using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MonolithicSync.Abstract;
using MonolithicSync.Helpers;
// ReSharper disable ConditionIsAlwaysTrueOrFalse

// ReSharper disable IdentifierTypo
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace MonolithicSync.Concrete
{
    public class MonolithicSync : IMonolithicSync
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Slims = new ConcurrentDictionary<string, SemaphoreSlim>();
        private static SemaphoreSlim _waitSlim = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _waitListSlim = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _waitAsyncSlim = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _waitListAsyncSlim = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _releaseSlim = new SemaphoreSlim(1, 1);
        private static SemaphoreSlim _releaseGroupsSlim = new SemaphoreSlim(1, 1);

        private static string GenerateSlimId(string groupKey, string key) => string.Concat(groupKey, "-", key);
        private static string GenerateSlimIdWithoutKey(string type) => string.Concat(type, "-");

        /// <inheritdoc />
        public async Task<bool> LockAsync(string groupKey, string key, int maxCount = 1, int timeout = MonolithicSyncConstants.MillisecondsTimeout)
        {
            try
            {
                await _waitAsyncSlim.WaitAsync();

                var slimId = GenerateSlimId(groupKey, key);
                if (!Slims.TryGetValue(slimId, out var currentSlim))
                {
                    Slims.TryAdd(slimId, currentSlim = new SemaphoreSlim(1, maxCount));
                }

                return await currentSlim.WaitAsync(timeout);
            }
            finally
            {
                _waitAsyncSlim.Release();
            }
        }

        /// <inheritdoc />
        public async Task<bool> LockAsync(string groupKey, IEnumerable<string> keys, int maxCount = 1, int timeout = MonolithicSyncConstants.MillisecondsTimeout)
        {
            try
            {
                await _waitListAsyncSlim.WaitAsync();
                var lockedItems = new List<LockItem>();

                var result = true;
                foreach (var key in keys.Distinct())
                {
                    result = await LockAsync(groupKey, key, maxCount, timeout) && result;
                    if (result)
                    {
                        lockedItems.Add(new LockItem
                        {
                            Key = key,
                            GroupKey = groupKey
                        });
                    }
                    else
                    {
                        break;
                    }
                }

                if (!result)
                {
                    foreach (var lockedItem in lockedItems)
                    {
                        Release(lockedItem.GroupKey, lockedItem.Key);
                    }
                }
                lockedItems.Clear();
                return result;
            }
            finally
            {
                _waitListAsyncSlim.Release();
            }
        }

        /// <inheritdoc />
        public bool Lock(string groupKey, string key, int maxCount = 1, int timeout = MonolithicSyncConstants.MillisecondsTimeout)
        {
            try
            {
                _waitSlim.Wait();

                var slimId = GenerateSlimId(groupKey, key);
                if (!Slims.TryGetValue(slimId, out var currentSlim))
                {
                    Slims.TryAdd(slimId, currentSlim = new SemaphoreSlim(1, maxCount));
                }

                return currentSlim.Wait(timeout);
            }
            finally
            {
                _waitSlim.Release();
            }
        }

        /// <inheritdoc />
        public bool Lock(string groupKey, IEnumerable<string> keys, int maxCount = 1, int timeout = MonolithicSyncConstants.MillisecondsTimeout)
        {
            try
            {
                _waitListSlim.Wait();
                var lockedItems = new List<LockItem>();

                var result = true;
                foreach (var key in keys.Distinct())
                {
                    result = Lock(groupKey, key, maxCount, timeout) && result;
                    if (result)
                    {
                        lockedItems.Add(new LockItem
                        {
                            Key = key,
                            GroupKey = groupKey
                        });
                    }
                    else
                    {
                        break;
                    }
                }

                if (!result)
                {
                    foreach (var lockedItem in lockedItems)
                    {
                        Release(lockedItem.GroupKey, lockedItem.Key);
                    }
                }
                lockedItems.Clear();
                return result;
            }
            finally
            {
                _waitListSlim.Release();
            }
        }

        /// <inheritdoc />
        public int Release(string type, string value)
        {
            try
            {
                _releaseSlim.Wait();

                int previousCount;
                var slimId = GenerateSlimId(type, value);
                if (!Slims.TryGetValue(slimId, out var currentSlim))
                {
                    return 0;
                }

                try
                {
                    previousCount = currentSlim.Release();
                }
                finally
                {
                    if (currentSlim.CurrentCount == 1)
                    {
                        try
                        {
                            currentSlim.Dispose();
                        }
                        finally
                        {
                            Slims.TryRemove(slimId, out var _);
                        }
                    }
                }

                return previousCount;
            }
            finally
            {
                _releaseSlim.Release();
            }
        }

        /// <inheritdoc />
        public int Release(string type, IEnumerable<string> values)
        {
            var totalPreviousCount = 0;
            foreach (var value in values.Distinct())
            {
                totalPreviousCount += Release(type, value);
            }

            return totalPreviousCount;
        }

        /// <inheritdoc />
        public void Release(string type)
        {
            try
            {
                _releaseGroupsSlim.Wait();

                var slimId = GenerateSlimIdWithoutKey(type);
                var keys = new List<string>();
                var slimRawKeysToRelease = Slims.Where(p => p.Key.StartsWith(slimId)).Select(p => p.Key);

                foreach (var currentKey in slimRawKeysToRelease)
                {
                    keys.Add(currentKey.Replace(slimId, string.Empty));
                }

                Release(type, keys);
            }
            finally
            {
                _releaseGroupsSlim.Release();
            }
        }
    }
}