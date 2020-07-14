using System.Threading.Tasks;

namespace ArenaSApp.Services.Dialog
{
    public interface IDialogService
    {
        Task ShowAlertAsync(string message, string title, string buttonLabel);
    }
}