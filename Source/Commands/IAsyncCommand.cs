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
using System.Windows.Input;

namespace Nuclex.Avalonia.Commands {

  /// <summary>Command that executes asynchronously</summary>
  /// <typeparam name="TArgument">Type of an optional argument for the command</typeparam>
  public interface IAsyncCommand<TArgument> : ICommand {

    /// <summary>Whether the command is currently executing</summary>
    bool IsRunning { get; }

    /// <summary>Runs the command asynchronously</summary>
    /// <param name="argument">Optional argument for the command</param>
    /// <returns>A task that completes when the command has executed</returns>
    Task ExecuteAsync(TArgument argument);

    /// <summary>Triggers reevaluation of <see cref="ICommand.CanExecute(object)" /></summary>
    void NotifyCanExecuteChanged();

  }

} // namespace Nuclex.Avalonia.Commands
