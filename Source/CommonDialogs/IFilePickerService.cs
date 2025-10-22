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

using Avalonia.Platform.Storage;

namespace Nuclex.Avalonia.CommonDialogs {

  /// <summary>Service that asks the user to pick a file to open or to save</summary>
  /// <remarks>
  ///   All methods here work with Avalonia's <see cref="IStorageFile" /> interface to
  ///   support web and mobile platforms.
  /// </remarks>
  public interface IFilePickerService {

    /// <summary>Shows a file selector asking the user where to save a file</summary>
    /// <param name="caption">
    ///   Caption to use for the dialog asking the user for a save location
    /// </param>
    /// <param name="fileTypes">File types the user can choose to save as</param>
    /// <returns>
    ///   A task that will provide the pre-opened file for saving or null if
    ///   the user has canceled the file picker dialog
    /// </returns>
    Task<IStorageFile?> AskForSaveLocationAsync(
      string caption, params FilePickerFileType[] fileTypes
    );

    /// <summary>Shows a file selector asking the user for a file to open</summary>
    /// <param name="caption">
    ///   Caption to use for the dialog asking the user to select a file
    /// </param>
    /// <param name="fileTypes">File types the list is filtered for by default</param>
    /// <returns>
    ///   A task that will provide the pre-opened file for readiong or null if
    ///   the user has canceled the file picker dialog
    /// </returns>
    Task<IStorageFile?> AskForFileToOpeAsync(
      string caption, params FilePickerFileType[] fileTypes
    );

    /// <summary>Shows a file selector asking the user for a file to open</summary>
    /// <param name="caption">
    ///   Caption to use for the dialog asking the user to select a file
    /// </param>
    /// <param name="fileTypes">File types the list is filtered for by default</param>
    /// <returns>
    ///   A task that will provide the pre-opened file for readiong or null if
    ///   the user has canceled the file picker dialog
    /// </returns>
    Task<IReadOnlyList<IStorageFile>?> AskForFilesToOpenAsync(
      string caption, params FilePickerFileType[] fileTypes
    );

  }

} // namespace Nuclex.Avalonia.CommonDialogs
