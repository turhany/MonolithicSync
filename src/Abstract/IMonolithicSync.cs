using System.Collections.Generic;
using System.Threading;
using MonolithicSync.Helpers;

namespace MonolithicSync.Abstract
{
    public interface IMonolithicSync
    {
        /// <summary>
        /// Lock.
        /// </summary>
        /// <param name="groupKey">The group key(ReleaseCurrentThreadLocks work over this group key).</param>
        /// <param name="key">The key for lock.</param>
        /// <param name="maxCount">The maximum number of requests for the lock that can be granted.</param>
        /// <param name="timeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.</param>
        /// <returns>
        /// If lock succeeded return "true", if it failed or timeout returned "false".
        /// </returns>
        bool Lock(string groupKey, string key, int maxCount = 1, int timeout = MonolithicSyncConstants.MillisecondsTimeout);

        /// <summary>
        /// Locks.
        /// </summary>
        /// <param name="groupKey">The group key(ReleaseCurrentThreadLocks work over this group key).</param>
        /// <param name="keys">The keys for lock.</param>
        /// <param name="maxCount">The maximum number of requests for the lock that can be granted.</param>
        /// <param name="timeout">The number of milliseconds to wait, or <see cref="Timeout.Infinite"/>(-1) to wait indefinitely.</param>
        /// <returns>
        /// If lock succeeded return "true", if it failed or timeout returned "false".
        /// </returns>
        bool Lock(string groupKey, IEnumerable<string> keys, int maxCount = 1, int timeout = MonolithicSyncConstants.MillisecondsTimeout);

        /// <summary>
        /// Releases the Lock.
        /// </summary>
        /// <param name="groupKey">The group key.</param>
        /// <param name="key">The key for lock.</param>
        int Release(string groupKey, string key);

        /// <summary>
        /// Releases the Locks.
        /// </summary>
        /// <param name="groupKey">The group key.</param>
        /// <param name="keys">The keys for lock.</param>
        int Release(string groupKey, IEnumerable<string> keys);

        /// <summary>
        /// Release the all locks by groupKey.
        /// </summary>
        /// <param name="groupKey">The group key.</param>
        void Release(string groupKey);
    }
}