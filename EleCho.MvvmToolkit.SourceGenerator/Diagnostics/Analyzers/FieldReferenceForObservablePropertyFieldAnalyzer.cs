// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using static EleCho.MvvmToolkit.SourceGenerators.Diagnostics.DiagnosticDescriptors;

namespace EleCho.MvvmToolkit.SourceGenerators;

/// <summary>
/// A diagnostic analyzer that generates a warning when accessing a field instead of a generated observable property.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FieldReferenceForObservablePropertyFieldAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// The key for the name of the target field to update.
    /// </summary>
    internal const string FieldNameKey = "FieldName";

    /// <summary>
    /// The key for the name of the generated property to update a field reference to.
    /// </summary>
    internal const string PropertyNameKey = "PropertyName";

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(FieldReferenceForObservablePropertyFieldWarning);

    /// <inheritdoc/>
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();

        context.RegisterCompilationStartAction(static context =>
        {
            // Get the symbol for [ObservableProperty]
            if (context.Compilation.GetTypeByMetadataName("EleCho.MvvmToolkit.ComponentModel.ObservablePropertyAttribute") is not INamedTypeSymbol observablePropertySymbol)
            {
                return;
            }

            context.RegisterOperationAction(context =>
            {
                // We're only looking for references to fields that could potentially be observable properties
                if (context.Operation is not IFieldReferenceOperation
                    {
                        Field: IFieldSymbol { IsStatic: false, IsConst: false, IsImplicitlyDeclared: false, ContainingType: INamedTypeSymbol } fieldSymbol,
                        Instance.Type: ITypeSymbol typeSymbol
                    })
                {
                    return;
                }

                // Special case field references from within a constructor and don't ever emit warnings for them. The point of this
                // analyzer is to prevent mistakes when users assign a field instead of a property and then get confused when the
                // property changed event is not raised. But this would never be the case from a constructur anyway, given that
                // no handler for that event would possibly be present. Suppressing warnings in this cases though will help to
                // avoid scenarios where people get nullability warnings they cannot suppress, in case they were pushed by the
                // analyzer in the MVVM Toolkit to not assign a field marked with a non-nullable reference type. Ideally this
                // would be solved by habing the generated setter be marked with [MemberNotNullIfNotNull("field", "value")],
                // but such an annotation does not currently exist.
                if (context.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.Constructor, ContainingType: INamedTypeSymbol instanceType } &&
                    SymbolEqualityComparer.Default.Equals(instanceType, typeSymbol))
                {
                    return;
                }

                foreach (AttributeData attribute in fieldSymbol.GetAttributes())
                {
                    // Look for the [ObservableProperty] attribute (there can only ever be one per field)
                    if (attribute.AttributeClass is { Name: "ObservablePropertyAttribute" } attributeClass &&
                        SymbolEqualityComparer.Default.Equals(attributeClass, observablePropertySymbol))
                    {
                        // Emit a warning to redirect users to access the generated property instead
                        context.ReportDiagnostic(Diagnostic.Create(
                            FieldReferenceForObservablePropertyFieldWarning,
                            context.Operation.Syntax.GetLocation(),
                            ImmutableDictionary.Create<string, string?>()
                                .Add(FieldNameKey, fieldSymbol.Name)
                                .Add(PropertyNameKey, ObservablePropertyGenerator.Execute.GetGeneratedPropertyName(fieldSymbol)),
                            fieldSymbol));

                        return;
                    }
                }
            }, OperationKind.FieldReference);
        });
    }
}
