// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Linq;

namespace EleCho.MvvmToolkit.ComponentModel;

/// <summary>
/// An attribute that can be used to support <see cref="ObservablePropertyAttribute"/> in generated properties. When this attribute is
/// used, the generated property setter will also call <see cref="ObservableObject.OnPropertyChanged(string?)"/> (or the equivalent
/// method in the target class) for the properties specified in the attribute data. This can be useful to keep the code compact when
/// there are one or more dependent properties that should also be reported as updated when the value of the annotated observable
/// property is changed. If this attribute is used in a field without <see cref="ObservablePropertyAttribute"/>, it is ignored.
/// <para>
/// In order to use this attribute, the containing type has to implement the <see cref="INotifyPropertyChanged"/> interface
/// and expose a method with the same signature as <see cref="ObservableObject.OnPropertyChanged(string?)"/>. If the containing
/// type also implements the <see cref="INotifyPropertyChanging"/> interface and exposes a method with the same signature as
/// <see cref="ObservableObject.OnPropertyChanging(string?)"/>, then this method will be invoked as well by the property setter.
/// </para>
/// <para>
/// This attribute can be used as follows:
/// <code>
/// partial class MyViewModel : ObservableObject
/// {
///     [ObservableProperty]
///     [NotifyPropertyChangedFor(nameof(FullName))]
///     private string name;
///
///     [ObservableProperty]
///     [NotifyPropertyChangedFor(nameof(FullName))]
///     private string surname;
///
///     public string FullName => $"{Name} {Surname}";
/// }
/// </code>
/// </para>
/// And with this, code analogous to this will be generated:
/// <code>
/// partial class MyViewModel
/// {
///     public string Name
///     {
///         get => name;
///         set
///         {
///             if (!EqualityComparer&lt;string&gt;.Default.Equals(name, value))
///             {
///                 OnPropertyChanging(nameof(Name));
///                 OnPropertyChanged(nameof(FullName));
///                 
///                 name = value;
///                 
///                 OnPropertyChanged(nameof(Name));
///                 OnPropertyChanged(nameof(FullName));
///             }
///         }
///     }
///
///     public string Surname
///     {
///         get => surname;
///         set
///         {
///             if (!EqualityComparer&lt;string&gt;.Default.Equals(name, value))
///             {
///                 OnPropertyChanging(nameof(Surname));
///                 OnPropertyChanged(nameof(FullName));
///                 
///                 surname = value;
///                 
///                 OnPropertyChanged(nameof(Surname));
///                 OnPropertyChanged(nameof(FullName));
///             }
///         }
///     }
/// }
/// </code>
/// </summary>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
public sealed class NotifyPropertyChangedForAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotifyPropertyChangedForAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property to also notify when the annotated property changes.</param>
    public NotifyPropertyChangedForAttribute(string propertyName)
    {
        PropertyNames = new[] { propertyName };
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotifyPropertyChangedForAttribute"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the property to also notify when the annotated property changes.</param>
    /// <param name="otherPropertyNames">
    /// The other property names to also notify when the annotated property changes. This parameter can optionally
    /// be used to indicate a series of dependent properties from the same attribute, to keep the code more compact.
    /// </param>
    public NotifyPropertyChangedForAttribute(string propertyName, params string[] otherPropertyNames)
    {
        PropertyNames = new[] { propertyName }.Concat(otherPropertyNames).ToArray();
    }

    /// <summary>
    /// Gets the property names to also notify when the annotated property changes.
    /// </summary>
    public string[] PropertyNames { get; }
}
