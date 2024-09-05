// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using EleCho.MvvmToolkit.SourceGenerators.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using static EleCho.MvvmToolkit.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace EleCho.MvvmToolkit.SourceGenerators;

/// <summary>
/// A diagnostic analyzer that generates an error whenever a source-generator attribute is used with not high enough C# version enabled.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnsupportedCSharpLanguageVersionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The mapping of target attributes that will trigger the analyzer.
    /// </summary>
    private static readonly ImmutableDictionary<string, string> GeneratorAttributeNamesToFullyQualifiedNamesMap = ImmutableDictionary.CreateRange(new[]
    {
        new KeyValuePair<string, string>("INotifyPropertyChangedAttribute", "EleCho.MvvmToolkit.ComponentModel.INotifyPropertyChangedAttribute"),
        new KeyValuePair<string, string>("NotifyCanExecuteChangedForAttribute", "EleCho.MvvmToolkit.ComponentModel.NotifyCanExecuteChangedForAttribute"),
        new KeyValuePair<string, string>("NotifyDataErrorInfoAttribute", "EleCho.MvvmToolkit.ComponentModel.NotifyDataErrorInfoAttribute"),
        new KeyValuePair<string, string>("NotifyPropertyChangedForAttribute", "EleCho.MvvmToolkit.ComponentModel.NotifyPropertyChangedForAttribute"),
        new KeyValuePair<string, string>("NotifyPropertyChangedRecipientsAttribute", "EleCho.MvvmToolkit.ComponentModel.NotifyPropertyChangedRecipientsAttribute"),
        new KeyValuePair<string, string>("ObservableObjectAttribute", "EleCho.MvvmToolkit.ComponentModel.ObservableObjectAttribute"),
        new KeyValuePair<string, string>("ObservablePropertyAttribute", "EleCho.MvvmToolkit.ComponentModel.ObservablePropertyAttribute"),
        new KeyValuePair<string, string>("ObservableRecipientAttribute", "EleCho.MvvmToolkit.ComponentModel.ObservableRecipientAttribute"),
        new KeyValuePair<string, string>("RelayCommandAttribute", "EleCho.MvvmToolkit.Input.RelayCommandAttribute")
    });

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(UnsupportedCSharpLanguageVersionError);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        // Defer the callback registration to when the compilation starts, so we can execute more
        // preliminary checks and skip registering any kind of symbol analysis at all if not needed.
        context.RegisterCompilationStartAction(static context =>
        {
            // Check that the language version is not high enough, otherwise no diagnostic should ever be produced
            if (context.Compilation.HasLanguageVersionAtLeastEqualTo(LanguageVersion.CSharp8))
            {
                return;
            }

            // Try to get all necessary type symbols
            if (!context.Compilation.TryBuildNamedTypeSymbolMap(GeneratorAttributeNamesToFullyQualifiedNamesMap, out ImmutableDictionary<string, INamedTypeSymbol>? typeSymbols))
            {
                return;
            }

            context.RegisterSymbolAction(context =>
            {
                // The possible attribute targets are only fields, classes and methods
                if (context.Symbol is not (IFieldSymbol or INamedTypeSymbol { TypeKind: TypeKind.Class, IsImplicitlyDeclared: false } or IMethodSymbol))
                {
                    return;
                }

                foreach (AttributeData attribute in context.Symbol.GetAttributes())
                {
                    // Go over each attribute on the target symbol, and check if the attribute type name is a candidate.
                    // If it is, double check by actually resolving the symbol from the mapping and comparing against it.
                    if (attribute.AttributeClass is { Name: string attributeName } attributeClass &&
                        typeSymbols.TryGetValue(attributeName, out INamedTypeSymbol? attributeSymbol) &&
                        SymbolEqualityComparer.Default.Equals(attributeClass, attributeSymbol))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(UnsupportedCSharpLanguageVersionError, context.Symbol.Locations.FirstOrDefault()));

                        // If we created a diagnostic for this symbol, we can stop. Even if there's multiple attributes, no need for repeated errors
                        return;
                    }
                }
            }, SymbolKind.Field, SymbolKind.NamedType, SymbolKind.Method);
        });
    }
}
