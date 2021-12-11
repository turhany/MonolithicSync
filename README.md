![alt tag](/img/monolithicsync.png)  

MonolithicSync is a simple lock management library for .NET monolithic applications.

[![NuGet version](https://badge.fury.io/nu/MonolithicSync.svg)](https://badge.fury.io/nu/MonolithicSync)  ![Nuget](https://img.shields.io/nuget/dt/MonolithicSync)

#### Features:
- Single key lock / async lock
- Multiple key lock / async lock
- Single key release
- Multiple key release
- Can group locks
- Can relase locks with group key

#### Usages:
-----
DI Registration:

```cs
  public void ConfigureServices(IServiceCollection services)
  {
      services.AddScoped<IMonolithicSync, MonolithicSync.Concrete.MonolithicSync>();
  }
```

Lock/Release:

```cs
  public void BestWayToUse()
  {
      var groupKey = "group1";
      var lockKey = "key";

      if (!_monolithicSync.Lock(groupKey, lockKey))
      {
          throw new Exception(String.Format(MonolithicSync.Helpers.MonolithicSyncConstants.LockFailMessage, $"{groupKey}-{lockKey}"));
      }

      try
      {
          //do something
      }
      catch (Exception e)
      {
          Console.WriteLine(e);
          throw;
      }
      finally
      {
          _monolithicSync.Release(groupKey, lockKey);
      }
  }
```
### Release Notes

#### 1.0.4
* Async Lock methods added.

#### 1.0.3
* Relase by Group Key bug fixed.

#### 1.0.2
* Base Release (deprecated)

#### 1.0.0 - 1.0.1
* Broken releases (deprecated)
