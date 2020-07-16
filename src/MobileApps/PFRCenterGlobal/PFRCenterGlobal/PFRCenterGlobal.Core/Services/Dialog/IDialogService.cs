using System.Threading.Tasks;

namespace PFRCenterGlobal.Core.Core.Services.Dialog
{
    public interface IDialogService
    {
        Task ShowAlertAsync(string message, string title, string buttonLabel);
    }
}