// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using System.Linq;
using EleCho.MvvmToolkit.SourceGenerators.Extensions;
using EleCho.MvvmToolkit.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static EleCho.MvvmToolkit.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace EleCho.MvvmToolkit.SourceGenerators;

/// <summary>
/// A source generator for the <c>ObservableObjectAttribute</c> type.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class ObservableObjectGenerator : TransitiveMembersGenerator<int>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableObjectGenerator"/> class.
    /// </summary>
    public ObservableObjectGenerator()
        : base("EleCho.MvvmToolkit.ComponentModel.ObservableObjectAttribute")
    {
    }

    /// <inheritdoc/>
    private protected override int ValidateTargetTypeAndGetInfo(INamedTypeSymbol typeSymbol, AttributeData attributeData, Compilation compilation, out ImmutableArray<DiagnosticInfo> diagnostics)
    {
        diagnostics = ImmutableArray<DiagnosticInfo>.Empty;

        // Check if the type already implements INotifyPropertyChanged...
        if (typeSymbol.AllInterfaces.Any(i => i.HasFullyQualifiedMetadataName("System.ComponentModel.INotifyPropertyChanged")))
        {
            diagnostics = ImmutableArray.Create(DiagnosticInfo.Create(DuplicateINotifyPropertyChangedInterfaceForObservableObjectAttributeError, typeSymbol, typeSymbol));

            goto End;
        }

        // ...or INotifyPropertyChanging
        if (typeSymbol.AllInterfaces.Any(i => i.HasFullyQualifiedMetadataName("System.ComponentModel.INotifyPropertyChanging")))
        {
            diagnostics = ImmutableArray.Create(DiagnosticInfo.Create(DuplicateINotifyPropertyChangingInterfaceForObservableObjectAttributeError, typeSymbol, typeSymbol));

            goto End;
        }

        // Check if the type uses [INotifyPropertyChanged] or [ObservableObject] already (in the type hierarchy too)
        if (typeSymbol.InheritsAttributeWithFullyQualifiedMetadataName("EleCho.MvvmToolkit.ComponentModel.ObservableObjectAttribute") ||
            typeSymbol.HasOrInheritsAttributeWithFullyQualifiedMetadataName("EleCho.MvvmToolkit.ComponentModel.INotifyPropertyChangedAttribute"))
        {
            diagnostics = ImmutableArray.Create(DiagnosticInfo.Create(InvalidAttributeCombinationForObservableObjectAttributeError, typeSymbol, typeSymbol));

            goto End;
        }

        End:
        return 0;
    }

    /// <inheritdoc/>
    protected override ImmutableArray<MemberDeclarationSyntax> FilterDeclaredMembers(int info, ImmutableArray<MemberDeclarationSyntax> memberDeclarations)
    {
        return memberDeclarations;
    }
}
