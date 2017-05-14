using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Guldan.Services.Interfaces;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Shell;

namespace Guldan.Services
{
    public class MessageService : IMessageService
    {
        private readonly IDispatcherService _dispatcherService;

        public MessageService(IDispatcherService service)
        {
            _dispatcherService = service;
        }

        public Task<MessageBoxResult> ShowAsync(string message, string title = null, MessageBoxButton? buttons = null, MessageBoxImage? image = null)
        {
            var tcs = new TaskCompletionSource<MessageBoxResult>();

            _dispatcherService.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                Window activeWindow = null;
                for (var i = 0; i < Application.Current.Windows.Count; i++)
                {
                    var win = Application.Current.Windows[i];
                    if ((win != null) && (win.IsActive))
                    {
                        activeWindow = win;
                        break;
                    }
                }
                var result = activeWindow != null ? MessageBox.Show(activeWindow, message, title ?? string.Empty, buttons ?? MessageBoxButton.OK, image ?? MessageBoxImage.Information) : MessageBox.Show(message, title ?? string.Empty, buttons ?? MessageBoxButton.OK, image ?? MessageBoxImage.Information);
                tcs.SetResult(result);
            }));


            return tcs.Task;
        }

        public Task<string> ShowOpenFileDialogAsync(string title, string defaultExtension, params Tuple<string, string>[] extensions)
        {
            var tcs = new TaskCompletionSource<string>();

            _dispatcherService.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var result = string.Empty;

                CommonOpenFileDialog cfd = new CommonOpenFileDialog
                {
                    AllowNonFileSystemItems = true,
                    EnsureReadOnly = true,
                    EnsurePathExists = true,
                    EnsureFileExists = true,
                    DefaultExtension = defaultExtension,
                    Multiselect = false, // One file at a time
                    Title = title ?? I18N.GetSplitString("Select File")
                };

                if ((extensions != null) && (extensions.Any()))
                {
                    foreach (var ext in extensions)
                    {
                        cfd.Filters.Add(new CommonFileDialogFilter(ext.Item1, ext.Item2));
                    }
                }

                if (cfd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    ShellObject selectedObj = null;

                    try
                    {
                        // Try to get the selected item 
                        selectedObj = cfd.FileAsShellObject;
                    }
                    catch
                    {
                        //MessageBox.Show("Could not create a ShellObject from the selected item");
                    }

                    if (selectedObj != null)
                    {
                        // Get the file name
                        result = selectedObj.ParsingName;
                    }
                }

                tcs.SetResult(result);
            }));

            return tcs.Task;
        }

        public Task<string> ShowSaveFileDialogAsync(string title, string defaultExtension, params Tuple<string, string>[] extensions)
        {
            var tcs = new TaskCompletionSource<string>();

            _dispatcherService.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var result = string.Empty;

                // Create a CommonSaveFileDialog to select destination file
                var sfd = new CommonSaveFileDialog
                {
                    EnsureReadOnly = true,
                    EnsurePathExists = true,
                    DefaultExtension = defaultExtension,
                    Title = title ?? I18N.GetSplitString("Save File")
                };

                if ((extensions != null) && (extensions.Any()))
                {
                    foreach (var ext in extensions)
                    {
                        sfd.Filters.Add(new CommonFileDialogFilter(ext.Item1, ext.Item2));
                    }
                }

                if (sfd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    ShellObject selectedObj = null;

                    try
                    {
                        // Try to get the selected item 
                        selectedObj = sfd.FileAsShellObject;
                    }
                    catch
                    {
                        //MessageBox.Show("Could not create a ShellObject from the selected item");
                    }

                    if (selectedObj != null)
                    {
                        // Get the file name
                        result = selectedObj.ParsingName;
                    }
                }

                tcs.SetResult(result);
            }));

            return tcs.Task;
        }
    }
}
