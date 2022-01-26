using System;

namespace BacklogApp.IntegrationTests.Helpers
{
    internal class TestsConstants
    {
        internal const string UserEmail = "user@email.com";
        internal const string UserName = "User 1";
        internal const string Password = "Qwerty_1";
        internal const string RefreshTokenCookieName = "refresh-token";
        internal const string NewUserEmail = "newemail@email.com";
        internal const string NewUsername = "New User";
        internal const string NewPassword = "Qwerty_2";
        internal const string TestAuthenticationScheme = "Test";
        internal const string ProjectName = "Test Project";
        internal const string UserIdHeaderName = "UserId";

        internal const string TaskName = "Test task";
        internal const string TaskDescription = "Test task description";
        internal const string TaskDeadline = "2020-06-15T00:00:00Z";
        internal const int TaskPriority = 3;
        internal const string TaskPriorityCode = "medium";
        internal const string TaskAssessment = "5h";
        internal const string TaskStatus = "d";

        internal const string TaskStatusDiscussion = "discussion";
        internal const string TaskStatusDiscussionShort = "d";
        internal const string TaskStatusInWork = "inwork";
        internal const string TaskStatusInWorkShort = "w";
        internal const string TaskStatusCompleted = "completed";
        internal const string TaskStatusCompletedShort = "c";

        internal const string FileReadName = "read.bin";
        internal const string FileCreateName = "create.bin";
        internal const string FileDeleteName = "delete.bin";
        internal const string FileCode = "test";
        internal const string FileMime = "application/octet-stream";
        internal const int FileSize = 1024 * 1024;
    }
}
