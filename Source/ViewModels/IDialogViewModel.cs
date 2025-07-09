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

using Nuclex.Avalonia.ViewModels;

namespace Nuclex.Avalonia.ViewModels {

  /// <summary>Interface for dialog view models (typically modal ones)</summary>
  public interface IDialogViewModel {

    /// <summary>Indicates that the view should close</summary>
    event EventHandler<DialogResultEventArgs> Submitted;

  }

} // namespace Nuclex.Avalonia.ViewModels
