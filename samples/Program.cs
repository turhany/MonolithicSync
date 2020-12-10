using System;
using System.Collections.Generic;

namespace MonolithicSync.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            var monolithicSync = new Concrete.MonolithicSync();

            SingleKeyLockReleaseSample(monolithicSync);
            MultipleKeyLockReleaseSample(monolithicSync);
            ReleaseGroupLockSample(monolithicSync);

            Console.ReadLine();
        }

        private static void SingleKeyLockReleaseSample(Concrete.MonolithicSync monolithicSync)
        {
            var groupKey = "singleKeyLockGroup";
            var lockKey = "key";

            if (!monolithicSync.Lock(groupKey, lockKey))
            {
                throw new Exception(String.Format(MonolithicSync.Helpers.MonolithicSyncConstants.LockFailMessage, $"{groupKey}-{lockKey}"));
            }

            try
            {
                //do something
                Console.WriteLine($"GroupKey: {groupKey} - LockKey: {lockKey} locked.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                monolithicSync.Release(groupKey, lockKey);
                Console.WriteLine($"GroupKey: {groupKey} - LockKey: {lockKey} released.");
            }
        }

        private static void MultipleKeyLockReleaseSample(Concrete.MonolithicSync monolithicSync)
        {
            var groupKey = "multipleKeyLockGroup";
            var lockKeys =  new List<string>(){"key1","key2","key3"};

            if (!monolithicSync.Lock(groupKey, lockKeys))
            {
                throw new Exception(String.Format(MonolithicSync.Helpers.MonolithicSyncConstants.LockFailMessage, $"{groupKey}-{string.Join(",",lockKeys)}"));
            }

            try
            {
                //do something
                foreach (var lockKey in lockKeys)
                {
                    Console.WriteLine($"GroupKey: {groupKey} - LockKey: {lockKey} locked.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                monolithicSync.Release(groupKey, lockKeys);
                foreach (var lockKey in lockKeys)
                {
                    Console.WriteLine($"GroupKey: {groupKey} - LockKey: {lockKey} released.");
                }
            }
        }

        private static void ReleaseGroupLockSample(Concrete.MonolithicSync monolithicSync)
        {
            var groupKey = "groupsReleaseKeyLockGroup";
            var lockKeys = new List<string> { "key1", "key2", "key3" };

            if (!monolithicSync.Lock(groupKey, lockKeys))
            {
                throw new Exception(String.Format(MonolithicSync.Helpers.MonolithicSyncConstants.LockFailMessage, $"{groupKey}-{string.Join(",", lockKeys)}"));
            }

            try
            {
                //do something
                foreach (var lockKey in lockKeys)
                {
                    Console.WriteLine($"GroupKey: {groupKey} - LockKey: {lockKey} locked.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            finally
            {
                monolithicSync.Release(groupKey);
                Console.WriteLine($"GroupKey: {groupKey} all keys released.");
            }
        }
    }
}