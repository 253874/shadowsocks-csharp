using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Guldan.Services.Interfaces
{
    public interface IMessageService
    {
        Task<MessageBoxResult> ShowAsync(string message, string title = null, MessageBoxButton? buttons = null, MessageBoxImage? image = null);
        Task<string> ShowOpenFileDialogAsync(string title, string defaultExtension, params Tuple<string, string>[] extensions);
        Task<string> ShowSaveFileDialogAsync(string title, string defaultExtension, params Tuple<string, string>[] extensions);
    }
}
