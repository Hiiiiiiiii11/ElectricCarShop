using System.Threading.Tasks;

namespace AgencyService.Services
{
    public interface ITestDriveReminderService
    {
        Task SendRemindersAsync();
    }
}
