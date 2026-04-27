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

namespace Nuclex.Avalonia {

  /// <summary>Constructs views and view model in a scope</summary>
  /// <remarks>
  ///   <para>
  ///     By default the <see cref="WindowManager" /> uses its own
  ///     <see cref="WindowManager.CreateInstance" /> method to construct views
  ///     and view models via <see cref="Activator.CreateInstance(Type)" />,
  ///     which is enough to create forms (which the Windows Forms designer already
  ///     requires to have parameterless constructors) and view models, so long as
  ///     they also have parameterless constructors.
  ///   </para>
  ///   <para>
  ///     To support dependency injection via constructor parameters, you can
  ///     inherit from the <see cref="WindowManager" /> and provide your own override
  ///     of <see cref="WindowManager.CreateInstance" /> that constructs the required
  ///     instance via your dependency injector. This is decent until you have multiple
  ///     view models all accessing the same resource (i.e. a database) via threads.
  ///   </para>
  ///   <para>
  ///     In this final case, &quot;scopes&quot; have become a common solution. Each
  ///     scope has access to singleton services (these exist for the lifetime of
  ///     the entire application), but there are also scoped services which will have
  ///     new instances constructed within each scope. By implementing the
  ///     <see cref="WindowManager.CreateWindowScope" /> method, you can make
  ///     the window manager set up an implicit scope per window or dialog.
  ///   </para>
  /// </remarks>
  public interface IWindowScope : IDisposable {

    /// <summary>Creates an instance of the specified type in the scope</summary>
    /// <param name="type">Type an instance will be created of</param>
    /// <returns>The created instance</returns>
    /// <remarks>
    ///   Use this to wire up your dependency injection container. By default,
    ///   the Activator class will be used to create instances which only works
    ///   if all of your view models are concrete classes.
    /// </remarks>
    object CreateInstance(Type type);

  }

} // namespace Nuclex.Avalonia
