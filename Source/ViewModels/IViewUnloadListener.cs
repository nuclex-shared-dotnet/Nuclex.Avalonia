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
using System.Threading.Tasks;

namespace Nuclex.Avalonia.ViewModels {

  /// <summary>
  ///   Can be implemented by view models that wish to know when their view is unloading
  /// </summary>
  public interface IViewUnloadListener {

    /// <summary>Called when the view is about to unload</summary>
    /// <returns>A task what finishes when all view unload processing is done</returns>
    Task OnViewUnloading();

  }

} // namespace Nuclex.Avalonia.ViewModels
