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
#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using MessageBoxIcon = MsBox.Avalonia.Enums.Icon;
using MessageBoxButtons = MsBox.Avalonia.Enums.ButtonEnum;
using MessageDialogResult = MsBox.Avalonia.Enums.ButtonResult;

namespace Nuclex.Avalonia.Messages {

  /// <summary>Contains helper methods for the message service</summary>
#if NET6_0_OR_GREATER
  [SupportedOSPlatform("windows")]
#endif
  public static class MessageServiceHelper {

    /// <summary>Asks the user a question that can be answered with yes or no</summary>
    /// <param name="messageService">
    ///   Message service that will be used to display the question
    /// </param>
    /// <param name="text">Text that will be shown on the message box</param>
    /// <returns>The button the user has clicked on</returns>
    public static Task<MessageDialogResult> AskYesNoAsync(
      this IMessageService messageService, MessageText text
    ) {
      return messageService.ShowQuestionAsync(
        MessageBoxIcon.Question, text, MessageBoxButtons.YesNo
      );
    }

    /// <summary>Asks the user a question that can be answered with ok or cancel</summary>
    /// <param name="messageService">
    ///   Message service that will be used to display the question
    /// </param>
    /// <param name="text">Text that will be shown on the message box</param>
    /// <returns>The button the user has clicked on</returns>
    public static Task<MessageDialogResult> AskOkCancelAsync(
      this IMessageService messageService, MessageText text
    ) {
      return messageService.ShowQuestionAsync(
        MessageBoxIcon.Question, text, MessageBoxButtons.OkCancel
      );
    }

    /// <summary>
    ///   Asks the user a question that can be answered with yes, no or cancel
    /// </summary>
    /// <param name="messageService">
    ///   Message service that will be used to display the question
    /// </param>
    /// <param name="text">Text that will be shown on the message box</param>
    /// <returns>The button the user has clicked on</returns>
    public static Task<MessageDialogResult> AskYesNoCancelAsync(
      this IMessageService messageService, MessageText text
    ) {
      return messageService.ShowQuestionAsync(
        MessageBoxIcon.Question, text, MessageBoxButtons.YesNoCancel
      );
    }

    /// <summary>Displays an informative message</summary>
    /// <param name="messageService">
    ///   Message service that will be used to display the warning
    /// </param>
    /// <param name="text">Text to be displayed on the warning message</param>
    public static Task InformAsync(
      this IMessageService messageService, MessageText text
    ) {
      return messageService.ShowNotificationAsync(MessageBoxIcon.Info, text);
    }

    /// <summary>Displays a warning</summary>
    /// <param name="messageService">
    ///   Message service that will be used to display the warning
    /// </param>
    /// <param name="text">Text to be displayed on the warning message</param>
    public static Task WarnAsync(
      this IMessageService messageService, MessageText text
    ) {
      return messageService.ShowNotificationAsync(MessageBoxIcon.Warning, text);
    }

    /// <summary>Reports an error</summary>
    /// <param name="messageService">
    ///   Message service that will be used to display the warning
    /// </param>
    /// <param name="text">Text to be displayed on the warning message</param>
    public static Task ReportErrorAsync(
      this IMessageService messageService, MessageText text
    ) {
      return messageService.ShowNotificationAsync(MessageBoxIcon.Error, text);
    }

  }

} // namespace Nuclex.Avalonia.Messages
