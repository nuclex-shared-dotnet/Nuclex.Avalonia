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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Nuclex.Avalonia.Commands {

  /// <summary>Asynchronous command that delegates work to an external method</summary>
  /// <typeparam name="TArgument">Type of argument accepted by the command</typeparam>
  internal sealed class AsyncRelayCommand<TArgument> : IAsyncCommand<TArgument> {

    /// <summary>Raised when the command's executable state has changed</summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>Initializes a new asynchronous relay command</summary>
    /// <param name="executeAsync">Action that is executed when the command runs</param>
    /// <param name="canExecute">
    ///   Optional predicate that decides whether execution is currently allowed
    /// </param>
    /// <param name="allowConcurrentExecutions">
    ///   Whether the command may be executed while a previous execution is still running
    /// </param>
    public AsyncRelayCommand(
      Func<TArgument, Task> executeAsync,
      Predicate<TArgument>? canExecute = null,
      bool allowConcurrentExecutions = false
    ) {
      this.executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
      this.canExecute = canExecute;
      this.allowConcurrentExecutions = allowConcurrentExecutions;
    }

    /// <summary>Whether the command is currently executing</summary>
    public bool IsRunning {
      get { return (Volatile.Read(ref this.executionCount) > 0); }
    }

    /// <summary>Checks whether the command may currently execute</summary>
    /// <param name="parameter">Parameter to be passed to the command callback</param>
    /// <returns>True if command execution is currently allowed</returns>
    public bool CanExecute(object? parameter) {
      if((!this.allowConcurrentExecutions) && IsRunning) {
        return false;
      }

      if(this.canExecute == null) {
        return true;
      }

      return this.canExecute(getParameter(parameter));
    }

    /// <summary>Executes the command callback</summary>
    /// <param name="parameter">Parameter passed to the command callback</param>
    public void Execute(object? parameter) {
      _ = ExecuteAsync(getParameter(parameter));
    }

    /// <summary>Executes the command callback asynchronously</summary>
    /// <param name="parameter">Parameter passed to the command callback</param>
    /// <returns>A task that finishes when command execution has completed</returns>
    public async Task ExecuteAsync(TArgument parameter) {
      if(!CanExecute(parameter)) {
        return;
      }

      if(this.allowConcurrentExecutions) {
        Interlocked.Increment(ref this.executionCount);

        try {
          await this.executeAsync(parameter).ConfigureAwait(false);
        }
        finally {
          Interlocked.Decrement(ref this.executionCount);
        }
      } else {
        Interlocked.Increment(ref this.executionCount);
        NotifyCanExecuteChanged();

        try {
          await this.executeAsync(parameter).ConfigureAwait(false);
        }
        finally {
          Interlocked.Decrement(ref this.executionCount);
          NotifyCanExecuteChanged();
        }
      } // if concurrent executions allowed / not allowed
    }

    /// <summary>
    ///   Notifies listeners that <see cref="CanExecute(object?)" /> should be reevaluated
    /// </summary>
    public void NotifyCanExecuteChanged() {
      CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Converts and validates the untyped command parameter</summary>
    /// <param name="parameter">Untyped parameter passed through <see cref="ICommand" /></param>
    /// <returns>The parameter cast to the command's parameter type</returns>
    private static TArgument getParameter(object? parameter) {
      if(parameter is TArgument typedParameter) {
        return typedParameter;
      }

      if(parameter == null) {
        if(default(TArgument) == null) {
          return default!;
        }

        throw new ArgumentException(
          "This command expects a non-null parameter of the configured type.",
          nameof(parameter)
        );
      }

      throw new ArgumentException(
        $"This command expects a parameter of type {typeof(TArgument).FullName}.",
        nameof(parameter)
      );
    }

    /// <summary>Asynchronous callback invoked when command execution is requested</summary>
    private readonly Func<TArgument, Task> executeAsync;
    /// <summary>Optional callback deciding whether command execution is currently allowed</summary>
    private readonly Predicate<TArgument>? canExecute;
    /// <summary>Whether the command may run while a previous invocation is still active</summary>
    private readonly bool allowConcurrentExecutions;

    /// <summary>Number of currently active command executions</summary>
    private int executionCount;

  }

} // namespace Nuclex.Avalonia.Commands
