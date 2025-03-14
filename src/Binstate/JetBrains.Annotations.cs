/* MIT License

Copyright (c) 2016 JetBrains http://www.jetbrains.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

using System;
using System.Diagnostics;

// ReSharper disable All
namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked symbol is used implicitly (e.g. via reflection, in external library),
/// so this symbol will be ignored by usage-checking inspections. <br/>
/// You can use <see cref="ImplicitUseKindFlags"/> and <see cref="ImplicitUseTargetFlags"/>
/// to configure how this attribute is applied.
/// </summary>
/// <example><code>
/// [UsedImplicitly]
/// public class TypeConverter {}
///
/// public class SummaryData
/// {
/// [UsedImplicitly(ImplicitUseKindFlags.InstantiatedWithFixedConstructorSignature)]
/// public SummaryData() {}
/// }
///
/// [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors | ImplicitUseTargetFlags.Default)]
/// public interface IService {}
/// </code></example>
[AttributeUsage(AttributeTargets.All)]
[Conditional("JETBRAINS_ANNOTATIONS")]
internal sealed class UsedImplicitlyAttribute : Attribute
{
  public UsedImplicitlyAttribute()
    : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default) { }

  public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags)
    : this(useKindFlags, ImplicitUseTargetFlags.Default) { }

  public UsedImplicitlyAttribute(ImplicitUseTargetFlags targetFlags)
    : this(ImplicitUseKindFlags.Default, targetFlags) { }

  public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
  {
    UseKindFlags = useKindFlags;
    TargetFlags  = targetFlags;
  }

  public ImplicitUseKindFlags UseKindFlags { get; }

  public ImplicitUseTargetFlags TargetFlags { get; }
}

/// <summary>
/// Can be applied to attributes, type parameters, and parameters of a type assignable from <see cref="System.Type"/> .
/// When applied to an attribute, the decorated attribute behaves the same as <see cref="UsedImplicitlyAttribute"/>.
/// When applied to a type parameter or to a parameter of type <see cref="System.Type"/>,
/// indicates that the corresponding type is used implicitly.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.GenericParameter | AttributeTargets.Parameter)]
[Conditional("JETBRAINS_ANNOTATIONS")]
internal sealed class MeansImplicitUseAttribute : Attribute
{
  public MeansImplicitUseAttribute()
    : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default) { }

  public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags)
    : this(useKindFlags, ImplicitUseTargetFlags.Default) { }

  public MeansImplicitUseAttribute(ImplicitUseTargetFlags targetFlags)
    : this(ImplicitUseKindFlags.Default, targetFlags) { }

  public MeansImplicitUseAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
  {
    UseKindFlags = useKindFlags;
    TargetFlags  = targetFlags;
  }

  [UsedImplicitly]
  public ImplicitUseKindFlags UseKindFlags { get; }

  [UsedImplicitly]
  public ImplicitUseTargetFlags TargetFlags { get; }
}

/// <summary>
/// Specifies the details of implicitly used symbol when it is marked
/// with <see cref="MeansImplicitUseAttribute"/> or <see cref="UsedImplicitlyAttribute"/>.
/// </summary>
[Flags]
internal enum ImplicitUseKindFlags
{
  Default = Access | Assign | InstantiatedWithFixedConstructorSignature,
  /// <summary>Only entity marked with attribute considered used.</summary>
  Access = 1,
  /// <summary>Indicates implicit assignment to a member.</summary>
  Assign = 2,
  /// <summary>
  /// Indicates implicit instantiation of a type with fixed constructor signature.
  /// That means any unused constructor parameters won't be reported as such.
  /// </summary>
  InstantiatedWithFixedConstructorSignature = 4,
  /// <summary>Indicates implicit instantiation of a type.</summary>
  InstantiatedNoFixedConstructorSignature = 8,
}

/// <summary>
/// Specifies what is considered to be used implicitly when marked
/// with <see cref="MeansImplicitUseAttribute"/> or <see cref="UsedImplicitlyAttribute"/>.
/// </summary>
[Flags]
internal enum ImplicitUseTargetFlags
{
  Default = Itself,
  Itself  = 1,
  /// <summary>Members of the type marked with the attribute are considered used.</summary>
  Members = 2,
  /// <summary> Inherited entities are considered used. </summary>
  WithInheritors = 4,
  /// <summary>Entity marked with the attribute and all its members considered used.</summary>
  WithMembers = Itself | Members
}

/// <summary>
/// This attribute is intended to mark publicly available API,
/// which should not be removed and so is treated as used.
/// </summary>
[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
[AttributeUsage(AttributeTargets.All, Inherited = false)]
[Conditional("JETBRAINS_ANNOTATIONS")]
internal sealed class PublicAPIAttribute : Attribute
{
  public PublicAPIAttribute() { }

  public PublicAPIAttribute(string comment) => Comment = comment;

  public string? Comment { get; }
}

/// <summary>
/// Tells the code analysis engine if the parameter is completely handled when the invoked method is on stack.
/// If the parameter is a delegate, indicates that delegate is executed while the method is executed.
/// If the parameter is an enumerable, indicates that it is enumerated while the method is executed.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
[Conditional("JETBRAINS_ANNOTATIONS")]
internal sealed class InstantHandleAttribute : Attribute { }

/// <summary>
/// Indicates that the marked method builds string by the format pattern and (optional) arguments.
/// The parameter, which contains the format string, should be given in the constructor. The format string
/// should be in <see cref="string.Format(IFormatProvider,string,object[])"/>-like form.
/// </summary>
/// <example><code>
/// [StringFormatMethod("message")]
/// void ShowError(string message, params object[] args) { /* do something */ }
///
/// void Foo() {
/// ShowError("Failed: {0}"); // Warning: Non-existing argument in format string
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Delegate)]
[Conditional("JETBRAINS_ANNOTATIONS")]
internal sealed class StringFormatMethodAttribute : Attribute
{
  /// <param name="formatParameterName">
  /// Specifies which parameter of an annotated method should be treated as the format string
  /// </param>
  public StringFormatMethodAttribute(string formatParameterName)
  {
    FormatParameterName = formatParameterName;
  }

  public string FormatParameterName { get; }
}

/// <summary>
/// Indicates that the marked parameter is a message template where placeholders are to be replaced by the following arguments
/// in the order in which they appear
/// </summary>
/// <example><code>
/// void LogInfo([StructuredMessageTemplate]string message, params object[] args) { /* do something */ }
///
/// void Foo() {
/// LogInfo("User created: {username}"); // Warning: Non-existing argument in format string
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Parameter)]
[Conditional("JETBRAINS_ANNOTATIONS")]
internal sealed class StructuredMessageTemplateAttribute : Attribute { }