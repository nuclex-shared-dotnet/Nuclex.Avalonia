#region Apache License 2.0
/*
Nuclex Foundation libraries for .NET
Copyright (C) 2002-2026 Markus Ewald / Nuclex Development Labs

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

namespace Nuclex.Avalonia.ViewModels {

  /// <summary>Carries the desired dialog result from a view model</summary>
  public class DialogResultEventArgs : EventArgs {

    /// <summary>Initializes a new dialog result event argument container</summary>
    /// <param name="result">Result the dialog should exit with</param>
    public DialogResultEventArgs(object? result) {
      this.result = result;
    }

    /// <summary>Result that should be returned from the dialog</summary>
    public object? Result {
      get { return this.result; }
    }

    /// <summary>Result that should be returned from the dialog</summary>
    private readonly object? result;

  }

} // namespace Nuclex.Avalonia.ViewModels
