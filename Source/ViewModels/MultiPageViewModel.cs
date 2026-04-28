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
using System.Collections;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Nuclex.Avalonia.Commands;
using Nuclex.Support;

namespace Nuclex.Avalonia.ViewModels {

  /// <summary>Base class for view models that have multiple child view models</summary>
  /// <typeparam name="TPageEnumeration">
  ///   Type by which pages can be indicated (typically an enum)
  /// </typeparam>
  public abstract class MultiPageViewModel<TPageEnumeration> :
    Observable, IMultiPageViewModel<TPageEnumeration>, IDisposable
    where TPageEnumeration : struct {

    /// <summary>Initializes a new multi-page view model</summary>
    /// <param name="windowManager">
    ///   Window manager the view model uses to create child views
    /// </param>
    /// <param name="cachePageViewModels">
    ///   Whether child view models will be kept alive and reused
    /// </param>
    public MultiPageViewModel(IWindowManager windowManager, bool cachePageViewModels = false) {
      this.windowManager = windowManager;
      this.createViewModelForPageDelegate = (
        new Func<TPageEnumeration, object>(CreateViewModelForPage)
      );

      if(cachePageViewModels) {
        this.cachedViewModels = new ConcurrentDictionary<TPageEnumeration, object>();
      }

      SwitchPageCommand = new AsyncRelayCommand<TPageEnumeration>(switchPageAsync);
    }

    /// <summary>Command to switch the active view</summary>
    public IAsyncCommand<TPageEnumeration> SwitchPageCommand { get; }

    /// <summary>Immediately releases all resources owned by the instance</summary>
    public virtual void Dispose() {

      // If view models are being cached, simply dispose anything in the cache that
      // implements IDisposable, the active view will be part of the cache.
      if(this.cachedViewModels != null) {
        foreach(object cacheViewModel in this.cachedViewModels.Values) {
          disposeIfSupported(cacheViewModel);
        }
        this.activePageViewModel = null;

        this.cachedViewModels.Clear();
      } else if(this.activePageViewModel != null) { // No cache? Dispose active view.
        disposeIfSupported(this.activePageViewModel);
        this.activePageViewModel = null;
      }

    }

    /// <summary>Child page that is currently being displayed by the view model</summary>
    public TPageEnumeration? ActivePage {
      get { return this.activePage; }
    }

    /// <summary>Retrieves (and, if needed, creates) the view model for the active page</summary>
    /// <returns>A view model for the active page on the multi-page view model</returns>
    public object? ActivePageViewModel {
      get { return this.activePageViewModel; }
    }


    /// <summary>Windowmanager that can create view models and display other views</summary>
    protected IWindowManager WindowManager {
      get { return this.windowManager; }
    }

    /// <summary>Creates a view model for the specified page</summary>
    /// <param name="page">Page for which a view model will be created</param>
    /// <returns>The view model for the specified page</returns>
    protected abstract object CreateViewModelForPage(TPageEnumeration page);

    /// <summary>Switches to another page</summary>
    /// <param name="newPage">New page to switch to</param>
    /// <returns>A task that will finish when the new page has been switched to</returns>
    private Task switchPageAsync(TPageEnumeration newPage) {
      if(newPage.Equals(this.activePage)) {
        return Task.CompletedTask;
      }

      object? viewModelToDispose;
      if(this.cachedViewModels == null) {
        viewModelToDispose = this.activePageViewModel;

        object? newPageViewModel = CreateViewModelForPage(newPage);

        this.activePage = newPage;
        this.activePageViewModel = newPageViewModel;
      } else {
        viewModelToDispose = null;

        // Double-checked locking to avoid creating a view model for the same page
        // multiple times if the construction takes time
        object? newPageViewModel;
        if(!this.cachedViewModels.TryGetValue(newPage, out newPageViewModel)) {
          lock(this.cachedViewModels) {
            if(!this.cachedViewModels.TryGetValue(newPage, out newPageViewModel)) {
              newPageViewModel = CreateViewModelForPage(newPage);
              this.cachedViewModels.TryAdd(newPage, newPageViewModel);
            }
          }
        }

        this.activePage = newPage;
        this.activePageViewModel = newPageViewModel;
      }

      OnPropertyChanged(nameof(ActivePage));
      OnPropertyChanged(nameof(ActivePageViewModel));

      disposeIfSupported(viewModelToDispose);

      return Task.CompletedTask;
    }

    /// <summary>Disposes the specified object if it is disposable</summary>
    /// <param name="potentiallyDisposable">Object that will be disposed if supported</param>
    private static void disposeIfSupported(object? potentiallyDisposable) {
      var disposable = potentiallyDisposable as IDisposable;
      if(disposable != null) {
        disposable.Dispose();
      }
    }

    /// <summary>Window manager that can be used to display other views</summary>
    private readonly IWindowManager windowManager;
    /// <summary>Delegate for the CreateViewModelForPage() method</summary>
    private readonly Func<TPageEnumeration, object> createViewModelForPageDelegate;
    /// <summary>Cached page view models, if caching is enabled</summary>
    private readonly ConcurrentDictionary<TPageEnumeration, object>? cachedViewModels;

    /// <summary>Page that is currently active in the multi-page view model</summary>
    private TPageEnumeration? activePage;
    /// <summary>View model for the active page</summary>
    private object? activePageViewModel;

  }

} // namespace Nuclex.Avalonia.ViewModels
