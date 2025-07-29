#region Apache License 2.0
/*
Nuclex Foundation libraries for .NET
Copyright (C) 2002-2025 Markus Ewald / Nuclex Development Labs

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
#endregion // Apache License 2.0

using System;

using Avalonia;
using Avalonia.Controls;

namespace Nuclex.Avalonia {

  /// <summary>Additional properties that can be attached to Avalonia objects</summary>
  public static class Properties {

    #region class InvalidateMeasureOnChangeObserver

    /// <summary>
    ///   Invalidates the calculated measurement of the control that is reporting
    ///   a change to the property value
    /// </summary>
    private class InvalidateMeasureOnChangeObserver :
      IObserver<AvaloniaPropertyChangedEventArgs<object>> {

      /// <summary>The one and only instance you need</summary>
      public static readonly InvalidateMeasureOnChangeObserver Instance = new();

      /// <summary>Called after all observers have been notified successfully</summary>
      public void OnCompleted() {}

      /// <summary>Reports when the observed control encountered an error</summary>
      /// <param name="error">Error the observed control has encountered</param>
      public void OnError(Exception error) {}

      /// <summary>Reports the updated value of the property to the observer</summary>
      /// <param name="value">New value the property has assumed</param>
      public void OnNext(AvaloniaPropertyChangedEventArgs<object> value) {
        if(value.Sender is Control senderAsControl) {
          senderAsControl.InvalidateMeasure();
        }
      }

    }

    #endregion // class InvalidateMeasureOnChangeObserver

    /// <summary>
    ///   Invalidates the object's measured size of a control and triggers a new layout pass
    /// </summary>
    /// <remarks>
    ///   <para>
    ///     If, for some reason, an Avalonia control whose size depends on a data-bound value
    ///     (for example, a &lt;Border /&gt; that holds a &lt;TextBlock /gt; that uses data
    ///     binding) does not update its own size, that is an Avalonia bug.
    ///   </para>
    ///   <para>
    ///     By attaching this property to the misbehaving control and data-binding its value
    ///     to the trigger that should cause a reevaluation of its dimensions, you can work
    ///     around the issue:
    ///   </para>
    ///   <example>
    ///     <code>
    ///       &lt;Border
    ///         CornerRadius="4"
    ///         Padding="4,0"
    ///         HorizontalAlignment="Center"
    ///         nuclex:Properties.InvalidateMeasureOnChange="{Binding Status}"
    ///       &gt;
    ///         &lt;TextBlock
    ///           HorizontalAlignment="Center"
    ///           Text="{Binding Status}"
    ///         /&gt;
    ///       &lt;/Border&gt;
    ///     </code>
    ///   </example>
    /// </remarks>
    public static readonly AttachedProperty<object> InvalidateMeasureOnChangeProperty = (
      AvaloniaProperty.RegisterAttached<Control, object>(
        "InvalidateMeasureOnChange",
        typeof(Properties),
        defaultValue: null!,
        inherits: false
      )
    );

    /// <summary>Initializes all static members of the class</summary>
    static Properties() {
      InvalidateMeasureOnChangeProperty.Changed.Subscribe(InvalidateMeasureOnChangeObserver.Instance);
    }

    /// <summary>Reads the value of the 'InvalidateMeasureOnChange' property</summary>
    /// <param name="control">Control from which the property will be read</param>
    /// <returns>The current value of the property attached to the control</returns>
    public static object GetInvalidateMeasureOnChange(Control control) {
      return control.GetValue(InvalidateMeasureOnChangeProperty);
    }

    /// <summary>Updates the value of the 'InvalidateMeasureOnChange' property</summary>
    /// <param name="control">Control on which the property will be updated</param>
    /// <param name="value">New value to assign to the attached property</param>
    public static void SetInvalidateMeasureOnChange(Control control, object value) {
      control.SetValue(InvalidateMeasureOnChangeProperty, value);
    }

  }

} // namespace Nuclex.Avalonia
