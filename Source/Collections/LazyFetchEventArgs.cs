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

namespace Nuclex.Avalonia.Collections {

  /// <summary>
  ///   Argument container to report when items have been fetch by a lazy-loading collection
  /// </summary>
  public class LazyFetchEventArgs {

    /// <summary>Initializes a new lazy-fetched item argument container</summary>
    /// <param name="startIndex">Index of the first item that has been fetched</param>
    /// <param name="count">Number of items that have been fetched</param>
    public LazyFetchEventArgs(int startIndex, int count) {
      this.startIndex = startIndex;
      this.count = count;
    }

    /// <summary>Index of the first item that has been fetched</summary>
    public int StartIndex {
      get { return this.startIndex; }
    }

    /// <summary>Number of items that have been fetched</summary>
    public int Count {
      get { return this.count; }
    }

    /// <summary>Index of the first item that has been fetched</summary>
    private int startIndex;
    /// <summary>Number of items that have been fetched</summary>
    private int count;

  }

} // namespace Nuclex.Avalonia.Collections