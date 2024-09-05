// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace EleCho.MvvmToolkit;

internal static unsafe class UnsafeWrapper
{
    public static bool IsNullRef<T>(ref T value)
    {
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
        fixed (T* ptr = &value)
        {
            return ptr == null;
        }
#pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
    }
}