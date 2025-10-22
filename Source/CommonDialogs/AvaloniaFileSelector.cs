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
using System.Collections.Generic;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Nuclex.Avalonia.CommonDialogs {

  /// <summary>Displays the file picked dialog using the Avalonia interfaces</summary>
  /// <remarks>
  ///   The <see cref="IActiveWindowTracker" /> interface is used to determine the topmost
  ///   window of your application. so you either need to implement said interface or
  ///   build an MVVM application based on the <see cref="WindowManager" /> class which
  ///   will automatically provide an implementation of this interface and track which
  ///   dialog (or main window) of the application is active.
  /// </remarks>
  public class AvaloniaFileDialogs : IFilePickerService, IDirectoryPickerService {

    /// <summary>Initialzies a new Avalonia-based file selector service</summary>
    /// <param name="activeWindowTracker">
    ///   Used to determine the currently active window to which the file selector
    ///   dialog will be parented
    /// </param>
    public AvaloniaFileDialogs(IActiveWindowTracker activeWindowTracker) {
      this.activeWindowTracker = activeWindowTracker;
    }

    /// <summary>Shows a file selector asking the user where to save a file</summary>
    /// <param name="caption">
    ///   Caption to use for the dialog asking the user for a save location
    /// </param>
    /// <param name="fileTypes">File types the user can choose to save as</param>
    /// <returns>
    ///   A task that will provide the pre-opened file for saving or null if
    ///   the user has canceled the file picker dialog
    /// </returns>
    public Task<IStorageFile?> AskForSaveLocationAsync(
      string caption, params FilePickerFileType[] fileTypes
    ) {
      var options = new FilePickerSaveOptions() {
        Title = caption,
        ShowOverwritePrompt = true,
        FileTypeChoices = fileTypes,
      };

      string? firstExtensionMentioned = findFirstExtensionMentioned(fileTypes);
      if(firstExtensionMentioned != null) {
        options.DefaultExtension = firstExtensionMentioned;
      }

      // The currently active window / view in which the user must have clicked
      // on some kind of button or menu that triggered the open file(s) dialog
      Window? askingWindow = this.activeWindowTracker.ActiveWindow;
      if(askingWindow == null) {
        throw new InvalidOperationException(
          "Active window tracker did not provide an active Avalonia window. " +
          "Was the file selector perhaps used in a headless app or in a unit test?"
        );
      }

      // Show the Avalonia file picker dialog. Avalonia will return null if
      // the user cancels the dialog, which matches our own interface contract.
      return askingWindow.StorageProvider.SaveFilePickerAsync(options);
    }

    /// <summary>Shows a file selector asking the user for a file to open</summary>
    /// <param name="caption">
    ///   Caption to use for the dialog asking the user to select a file
    /// </param>
    /// <param name="fileTypes">File types the list is filtered for by default</param>
    /// <returns>
    ///   A task that will provide the pre-opened file for readiong or null if
    ///   the user has canceled the file picker dialog
    /// </returns>
    public async Task<IStorageFile?> AskForFileToOpeAsync(
      string caption, params FilePickerFileType[] fileTypes
    ) {
      var options = new FilePickerOpenOptions() {
        Title = caption,
        AllowMultiple = false,
        FileTypeFilter = fileTypes
      };

      // The currently active window / view in which the user must have clicked
      // on some kind of button or menu that triggered the open file(s) dialog
      Window? askingWindow = this.activeWindowTracker.ActiveWindow;
      if(askingWindow == null) {
        throw new InvalidOperationException(
          "Active window tracker did not provide an active Avalonia window. " +
          "Was the file selector perhaps used in a headless app or in a unit test?"
        );
      }

      // Show the Avalonia file picker dialog. Avalonia will return an empty list
      // if the user cancels the dialog, but we want to be explicit, so if
      // the list is either null (defensive programming) or contains no items,
      // we will explicitly return a null result instead of an empty list.
      IReadOnlyList<IStorageFile> selectedFile = (
        await askingWindow.StorageProvider.OpenFilePickerAsync(options)
      );
      if((selectedFile == null) || (selectedFile.Count != 1)) {
        return null;
      } else {
        return selectedFile[0];
      }
    }

    /// <summary>Shows a file selector asking the user for a file to open</summary>
    /// <param name="caption">
    ///   Caption to use for the dialog asking the user to select a file
    /// </param>
    /// <param name="fileTypes">File types the list is filtered for by default</param>
    /// <returns>
    ///   A task that will provide the pre-opened file for readiong or null if
    ///   the user has canceled the file picker dialog
    /// </returns>
    public async Task<IReadOnlyList<IStorageFile>?> AskForFilesToOpenAsync(
      string caption, params FilePickerFileType[] fileTypes
    ) {
      var options = new FilePickerOpenOptions() {
        Title = caption,
        AllowMultiple = true,
        FileTypeFilter = fileTypes
      };

      // The currently active window / view in which the user must have clicked
      // on some kind of button or menu that triggered the open file(s) dialog
      Window? askingWindow = this.activeWindowTracker.ActiveWindow;
      if(askingWindow == null) {
        throw new InvalidOperationException(
          "Active window tracker did not provide an active Avalonia window. " +
          "Was the file selector perhaps used in a headless app or in a unit test?"
        );
      }

      // Show the Avalonia file picker dialog. Avalonia will return an empty list
      // if the user cancels the dialog, but we want to be explicit, so if
      // the list is either null (defensive programming) or contains no items,
      // we will explicitly return a null result instead of an empty list.
      IReadOnlyList<IStorageFile> selectedFiles = (
        await askingWindow.StorageProvider.OpenFilePickerAsync(options)
      );
      if((selectedFiles == null) || (selectedFiles.Count == 0)) {
        return null;
      } else {
        return selectedFiles;
      }
    }

    /// <summary>Asks the user to select a directory</summary>
    /// <returns>A task that will provide the directory the user has selected</returns>
    public async Task<IStorageFolder?> AskForDirectory(string caption) {
      var options = new FolderPickerOpenOptions() {
        Title = caption,
        AllowMultiple = false
      };

      // The currently active window / view in which the user must have clicked
      // on some kind of button or menu that triggered the open directory dialog
      Window? askingWindow = this.activeWindowTracker.ActiveWindow;
      if(askingWindow == null) {
        throw new InvalidOperationException(
          "Active window tracker did not provide an active Avalonia window. " +
          "Was the directory selector perhaps used in a headless app or in a unit test?"
        );
      }

      // Show the Avalonia folder picker dialog. Avalonia will return an empty list
      // if the user cancels the dialog, but we want to be explicit, so if
      // the list is either null (defensive programming) or contains no items,
      // we will explicitly return a null result instead of an empty list.
      IReadOnlyList<IStorageFolder> selectedFolders = (
        await askingWindow.StorageProvider.OpenFolderPickerAsync(options)
      );
      if((selectedFolders == null) || (selectedFolders.Count != 1)) {
        return null;
      } else {
        return selectedFolders[0];
      }
    }

    /// <summary>Looks for the first file extension mentioned in the filters</summary>
    /// <param name="fileTypes">Filters to search for the first mentioned extension</param>
    /// <returns>The first extension mentioned or null if none are mentioned</returns>
    private static string? findFirstExtensionMentioned(FilePickerFileType[] fileTypes) {
      for(int index = 0; index < fileTypes.Length; ++index) {
        IReadOnlyList<string>? patterns = fileTypes[index].Patterns;
        if(patterns != null) {
          int count = patterns.Count;
          for(int patternIndex = 0; patternIndex < count; ++patternIndex) {
            if(!string.IsNullOrEmpty(patterns[patternIndex])) {
              string pattern = patterns[patternIndex];
              int finalDotIndex = pattern.LastIndexOf('.');
              if(finalDotIndex != -1) {
                return patterns[patternIndex].Substring(finalDotIndex + 1);
              }
            }
          } // for each pattern index
        } // if patterns set
      } // for each file type index

      return null;
    }

    /// <summary>
    ///   Provides the active window the file picked dialog should become a child of
    /// </summary>
    private readonly IActiveWindowTracker activeWindowTracker;

  }

} // namespace Nuclex.Avalonia.CommonDialogs
