using System.Collections.Concurrent;
using System.Threading;

namespace Tenray.Topaz.Core;

internal sealed class ScriptExecutorPool
{
    ScriptExecutor First;
    readonly ScriptExecutor[] Items;

    readonly ConcurrentBag<ScriptExecutor> Objects;
    readonly int FixedLength;
    readonly bool IsThreadSafe;

    public ScriptExecutorPool(int fixedLength = 16, bool isThreadSafe = true)
    {
        IsThreadSafe = isThreadSafe;
        FixedLength = fixedLength;
        Items = new ScriptExecutor[fixedLength];
        Objects = new ConcurrentBag<ScriptExecutor>();
    }

    public ScriptExecutor Get(TopazEngine engine, ScriptExecutor parentScope, ScopeType scopeType)
    {
        if (IsThreadSafe)
        {
            var item = First;
            if (item != null)
            {
                item = Interlocked.Exchange(ref First, null);
                if (item != null)
                {
                    item.Reconstruct(engine, parentScope, scopeType);
                    return item;
                }
            }

            var len = FixedLength;
            for (var i = 0; i < len; ++i)
            {
                item = Items[i];
                if (item != null)
                {
                    item = Interlocked.Exchange(ref Items[i], null);
                    if (item != null)
                    {
                        item.Reconstruct(engine, parentScope, scopeType);
                        return item;
                    }
                }
            }
        }
        else
        {
            var item = First;
            if (item != null)
            {
                First = null;
                item.Reconstruct(engine, parentScope, scopeType);
                return item;
            }

            var len = FixedLength;
            for (var i = 0; i < len; ++i)
            {
                item = Items[i];
                if (item != null)
                {
                    Items[i] = null;
                    item.Reconstruct(engine, parentScope, scopeType);
                    return item;
                }
            }
        }
        
        if (Objects.TryTake(out var bagItem))
        {
            bagItem.Reconstruct(engine, parentScope, scopeType);
            return bagItem;
        }
        return new ScriptExecutor(engine, parentScope, scopeType);
    }

    public void Return(ScriptExecutor item)
    {
        if (First == null)
        {
            First = item;
            return;
        }
        var len = FixedLength;
        for (var i = 0; i < len; ++i)
        {
            var slot = Items[i];
            if (slot == null)
            {
                Items[i] = item;
                return;
            }
        }
        Objects.Add(item);
    }
}
