// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace EleCho.MvvmToolkit.SourceGenerators.Extensions;

/// <summary>
/// Extension methods for the <see cref="INamedTypeSymbol"/> type.
/// </summary>
internal static class INamedTypeSymbolExtensions
{
    /// <summary>
    /// Gets all member symbols from a given <see cref="INamedTypeSymbol"/> instance, including inherited ones.
    /// </summary>
    /// <param name="symbol">The input <see cref="INamedTypeSymbol"/> instance.</param>
    /// <returns>A sequence of all member symbols for <paramref name="symbol"/>.</returns>
    public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol symbol)
    {
        for (INamedTypeSymbol? currentSymbol = symbol; currentSymbol is { SpecialType: not SpecialType.System_Object }; currentSymbol = currentSymbol.BaseType)
        {
            foreach (ISymbol memberSymbol in currentSymbol.GetMembers())
            {
                yield return memberSymbol;
            }
        }
    }

    /// <summary>
    /// Gets all member symbols from a given <see cref="INamedTypeSymbol"/> instance, including inherited ones.
    /// </summary>
    /// <param name="symbol">The input <see cref="INamedTypeSymbol"/> instance.</param>
    /// <param name="name">The name of the members to look for.</param>
    /// <returns>A sequence of all member symbols for <paramref name="symbol"/>.</returns>
    public static IEnumerable<ISymbol> GetAllMembers(this INamedTypeSymbol symbol, string name)
    {
        for (INamedTypeSymbol? currentSymbol = symbol; currentSymbol is { SpecialType: not SpecialType.System_Object }; currentSymbol = currentSymbol.BaseType)
        {
            foreach (ISymbol memberSymbol in currentSymbol.GetMembers(name))
            {
                yield return memberSymbol;
            }
        }
    }
}
