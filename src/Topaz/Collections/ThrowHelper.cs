﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Collections.Extensions;

internal static class ThrowHelper
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void ThrowInvalidOperationException_ConcurrentOperationsNotSupported()
    {
        throw new InvalidOperationException(Strings.InvalidOperation_ConcurrentOperationsNotSupported);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void ThrowKeyArgumentNullException()
    {
        throw new ArgumentNullException("key");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void ThrowCapacityArgumentOutOfRangeException()
    {
        throw new ArgumentOutOfRangeException("capacity");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool ThrowNotSupportedException_ReadOnly_Modification()
    {
        throw new NotSupportedException(Strings.ReadOnly_Modification);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool ThrowNotSupportedException()
    {
        throw new NotSupportedException();
    }
}
