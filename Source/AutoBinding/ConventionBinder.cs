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

using Avalonia.Controls;

using Nuclex.Avalonia.ViewModels;

namespace Nuclex.Avalonia.AutoBinding {

  /// <summary>
  ///   Binds a view to its model using a convention-over-configuration approach
  /// </summary>
  public class ConventionBinder : IAutoBinder {

    /// <summary>Binds the specified view to an explicitly selected view model</summary>
    /// <typeparam name="TViewModel">
    ///   Type of view model the view will be bound to
    /// </typeparam>
    /// <param name="view">View that will be bound to a view model</param>
    /// <param name="viewModel">View model the view will be bound to</param>
    public void Bind<TViewModel>(Control view, TViewModel? viewModel)
      where TViewModel : class {
      if(viewModel != null) {
        bind(view, viewModel);
      }
    }

    /// <summary>
    ///   Binds the specified view to the view model specified in its DataContext
    /// </summary>
    /// <param name="viewControl">View that will be bound</param>
    public void Bind(Control viewControl) {
      if(viewControl.DataContext != null) {
        bind(viewControl, viewControl.DataContext);
      }
    }

    /// <summary>Binds a view to a view model</summary>
    /// <param name="view">View that will be bound</param>
    /// <param name="viewModel">View model the view will be bound to</param>
    private void bind(Control view, object viewModel) {
      IDialogViewModel? dialogViewModel = viewModel as IDialogViewModel;
      if(dialogViewModel != null) {
        Window? viewAsWindow = view as Window;
        if(viewAsWindow != null) {
          bindViewModelSubmitToDialogClose(viewAsWindow, dialogViewModel);
        }
      }
    }

    /// <summary>
    ///   Convention binding for view models with a 'Submit' event,
    ///   closes the dialog when the view model fires the event
    /// </summary>
    /// <param name="dialogWindow">Window the displays the dialog's UI</param>
    /// <param name="dialogViewModel">View model that has the 'Submit' event</param>
    private static void bindViewModelSubmitToDialogClose(
      Window dialogWindow, IDialogViewModel dialogViewModel
    ) {
      EventHandler<DialogResultEventArgs> handler = (
        delegate(object sender, DialogResultEventArgs arguments) {
          dialogWindow.Close(arguments.Result);
        }
      );

      dialogViewModel.Submitted += handler;

      // Does this help anything?
      // Without it, the view has a reference to the view model (via DataContext),
      // and the view model references the view (via event subscription), but this
      // shouldn't bother the .NET garbage collector at all.
      dialogWindow.Closed += delegate(object sender, EventArgs arguments) {
        dialogViewModel.Submitted -= handler;
      };
    }

  }

} // namespace Nuclex.Avalonia.AutoBinding
