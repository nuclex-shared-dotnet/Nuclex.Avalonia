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
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
#if NET6_0_OR_GREATER
using System.Runtime.Versioning;
#endif

using Avalonia.Controls;

using MessageBoxIcon = MsBox.Avalonia.Enums.Icon;
using MessageBoxButtons = MsBox.Avalonia.Enums.ButtonEnum;
using MessageDialogResult = MsBox.Avalonia.Enums.ButtonResult;

namespace Nuclex.Avalonia.Messages {

  /// <summary>Uses Avalonia to display message boxes</summary>
  public class AvaloniaMessagePresenter : IMessageService {

    #region class MessageScope

    /// <summary>Triggers the message displayed and acknowledged events</summary>
    private class MessageScope : IDisposable {

      /// <summary>
      ///   Initializes a new message scope, triggering the message displayed event
      /// </summary>
      /// <param name="self">Message service the scope belongs to</param>
      /// <param name="image">Image of the message being displayed</param>
      /// <param name="text">Text contained in the message being displayed</param>
      public MessageScope(
        AvaloniaMessagePresenter self, MessageBoxIcon image, MessageText text
      ) {
        EventHandler<MessageEventArgs>? messageDisplayed = self.MessageDisplaying;
        if(messageDisplayed != null) {
          messageDisplayed(this, new MessageEventArgs(image, text));
        }

        this.self = self;
      }

      /// <summary>Triggers the message acknowledged event</summary>
      public void Dispose() {
        EventHandler? messageAcknowledged = self.MessageAcknowledged;
        if(messageAcknowledged != null) {
          messageAcknowledged(this, EventArgs.Empty);
        }
      }

      /// <summary>Message service the scope belongs to</summary>
      private AvaloniaMessagePresenter self;

    }

    #endregion // class MessageScope

    /// <summary>Triggered when a message is displayed to the user</summary>
    public event EventHandler<MessageEventArgs>? MessageDisplaying;

    /// <summary>Triggered when the user has acknowledged the current message</summary>
    public event EventHandler? MessageAcknowledged;

    /// <summary>Initializes a new Avalonia message service</summary>
    /// <param name="tracker">Used to determine the current top-level window</param>
    public AvaloniaMessagePresenter(IActiveWindowTracker tracker) {
      this.tracker = tracker;
    }

    /// <summary>Asks the user a question that can be answered via several buttons</summary>
    /// <param name="image">Image that will be shown on the message box</param>
    /// <param name="text">Text that will be shown to the user</param>
    /// <param name="buttons">Buttons available for the user to click on</param>
    /// <returns>The button the user has clicked on</returns>
    public Task<MessageDialogResult> ShowQuestionAsync(
      MessageBoxIcon image, MessageText text, MessageBoxButtons buttons
    ) {
      using(var scope = new MessageScope(this, image, text)) {
        MsBox.Avalonia.Base.IMsBox<MessageDialogResult> messageBox = (
          MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
            new MsBox.Avalonia.Dto.MessageBoxStandardParams() {
              ContentTitle = text.Caption,
              ContentHeader = text.Message,
              ContentMessage = text.Details ?? string.Empty,
              ButtonDefinitions = buttons,
              Icon = image,
              WindowStartupLocation = WindowStartupLocation.CenterOwner
            }
          )
        );
        return messageBox.ShowAsync(); // TODO: Make modal to current or main window
      }
    }

    /// <summary>Displays a notification to the user</summary>
    /// <param name="image">Image that will be shown on the message bx</param>
    /// <param name="text">Text that will be shown to the user</param>
    public Task ShowNotificationAsync(MessageBoxIcon image, MessageText text) {
      using(var scope = new MessageScope(this, image, text)) {
        MsBox.Avalonia.Base.IMsBox<MessageDialogResult> messageBox = (
          MsBox.Avalonia.MessageBoxManager.GetMessageBoxStandard(
            new MsBox.Avalonia.Dto.MessageBoxStandardParams() {
              ContentTitle = text.Caption,
              ContentHeader = text.Message,
              ContentMessage = text.Details ?? string.Empty,
              ButtonDefinitions = MessageBoxButtons.Ok,
              Icon = image,
              WindowStartupLocation = WindowStartupLocation.CenterOwner
            }
          )
        );

        Window? activeWindow = this.tracker.ActiveWindow;
        if(activeWindow == null) {
          return messageBox.ShowAsync();
        } else {
          //return messageBox.ShowAsPopupAsync(activeWindow);
          return messageBox.ShowWindowDialogAsync(activeWindow);
        }
      }
    }

    /// <summary>Reports an error using the system's message box functions</summary>
    /// <param name="title">Title of the message box</param>
    /// <param name="message">Message text that will be displayed</param>
    public static void FallbackReportError(string title, string message) {
      // TODO: Escape quotes for the command-line tools
      // TODO: Wait for the child process to exit so display is certain

      if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
        MessageBoxW(IntPtr.Zero, message, title, MB_OK | MB_ICONEXCLAMATION);
      } else if(RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
        Process.Start("zenity", $"--error --title=\"{title}\" --text=\"{message}\"");
      } else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) {
        Process.Start("osascript", $"-e 'display dialog \"{message}\" with title \"{title}\" with icon stop'");
      }
    }

    /// <summary>Windows only: display a message box with an OK button</summary>
#if NET6_0_OR_GREATER
  [SupportedOSPlatform("windows")]
#endif
    private const uint MB_OK = 0x00000000;

    /// <summary>Windows only: display a message box with an Exclamation icon</summary>
#if NET6_0_OR_GREATER
  [SupportedOSPlatform("windows")]
#endif
    private const uint MB_ICONEXCLAMATION = 0x00000030;

    /// <summary>Windows only: displays a native Windows message box</summary>
    /// <param name="parentWindowHandle">Handle of the window that owns the message box</param>
    /// <param name="text">Text that should be in the message box</param>
    /// <param name="caption">Caption or window title of the message box</param>
    /// <param name="type">Which icons and buttons that message box should have</param>
    /// <returns>How the user closed the message box and which button they clicked</returns>
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
#if NET6_0_OR_GREATER
  [SupportedOSPlatform("windows")]
#endif
    private static extern int MessageBoxW(IntPtr parentWindowHandle, string text, string caption, uint type);

    // <summary>Provides the currently active top-level window</summary>
    private IActiveWindowTracker tracker;

  }

} // namespace Nuclex.Avalonia.Messages

