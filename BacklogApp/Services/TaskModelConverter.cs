using BacklogApp.Models.Tasks;

namespace BacklogApp.Services
{
    public static class TaskModelConverter
    {
        public static string ToStatusCode(string? status) => status switch
        {
            TaskStatuses.Discussion => TaskStatusCodes.Discussion,
            TaskStatuses.InWork => TaskStatusCodes.InWork,
            TaskStatuses.Completed => TaskStatusCodes.Completed,
            _ => string.Empty
        };

        public static string ToStatus(string? statusCode) => statusCode switch
        {
            TaskStatusCodes.Discussion => TaskStatuses.Discussion,
            TaskStatusCodes.InWork => TaskStatuses.InWork,
            TaskStatusCodes.Completed => TaskStatuses.Completed,
            _ => string.Empty
        };

        public static bool IsStatusValid(string status) =>
               status == TaskStatuses.Discussion
            || status == TaskStatuses.InWork
            || status == TaskStatuses.Completed;


        public static string ToPriorityCode(int priority) => priority switch
        {
            1 => TaskPriotiryCodes.Lowest,
            2 => TaskPriotiryCodes.Low,
            3 => TaskPriotiryCodes.Medium,
            4 => TaskPriotiryCodes.High,
            5 => TaskPriotiryCodes.Highest,
            _ => string.Empty
        };

        public static int ToPriority(string? priorityCode) => priorityCode switch
        {
            TaskPriotiryCodes.Lowest => 1,
            TaskPriotiryCodes.Low => 2,
            TaskPriotiryCodes.Medium => 3,
            TaskPriotiryCodes.High => 4,
            TaskPriotiryCodes.Highest => 5,
            _ => 3
        };

        public static bool IsPriorityValid(int priority) => priority > 0 && priority < 6;
    }
}
