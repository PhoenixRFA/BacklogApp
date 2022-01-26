using BacklogApp.Managers;
using BacklogApp.Models.Db;
using BacklogApp.Models.Tasks;
using BacklogApp.Repository;
using BacklogApp.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BacklogApp.Tests.Managers
{
    public class TasksManagerTests
    {
        [Theory, AutoMoqData]
        public void Get_Returns_Task(string id, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            SetupTaskGet(id, out string userId, out string _, repo, projRepo, out TaskModel task);

            TasksManager mng = new(repo.Object, projRepo.Object);

            TaskViewModel? res = mng.Get(id, userId);

            Assert.NotNull(res);
            Assert.Equal(task.Id, res!.Id);
            Assert.True(res!.CanEdit);
        }

        [Theory, AutoMoqData]
        public void Get_Calls_IsUserInProject_Once(string id, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            SetupTaskGet(id, out string userId, out string projId, repo, projRepo, out _);

            TasksManager mng = new(repo.Object, projRepo.Object);

            TaskViewModel? res = mng.Get(id, userId);

            projRepo.Verify(x => x.IsUserInProject(projId, userId), Times.Once);
        }

        [Theory, AutoMoqData]
        public void Get_Calls_IsUserProjectOwner_Once(string id, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            var projectId = ObjectId.GenerateNewId();
            var userId = ObjectId.GenerateNewId();
            var anotherUserId = ObjectId.GenerateNewId();

            var task = new TaskModel
            {
                Id = id,
                Project = new MongoDBRef("", projectId),
                CreatedBy = new MongoDBRef("", anotherUserId)
            };
            repo.Setup(x => x.GetById(id))
                .Returns(task);
            projRepo.Setup(x => x.IsUserInProject(projectId.ToString(), userId.ToString()))
                .Returns(true);
            projRepo.Setup(x => x.IsUserProjectOwner(projectId.ToString(), userId.ToString()))
                .Returns(true);

            TasksManager mng = new(repo.Object, projRepo.Object);

            TaskViewModel? res = mng.Get(id, userId.ToString());

            projRepo.Verify(x => x.IsUserProjectOwner(projectId.ToString(), userId.ToString()), Times.Once);
        }


        [Theory, AutoMoqData]
        public void GetTasksInProject_Returns_Tasks(string projId, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            var userId = ObjectId.GenerateNewId();
            var userDbRef = new MongoDBRef("", userId);
            var projDbRef = new MongoDBRef("", projId);

            var tasks = new List<TaskModel>
            {
                new TaskModel { Id = "1", CreatedBy = userDbRef, Project = projDbRef },
                new TaskModel { Id = "2", CreatedBy = userDbRef, Project = projDbRef },
                new TaskModel { Id = "3", CreatedBy = userDbRef, Project = projDbRef }
            };
            List<TaskViewModel> expected = tasks.Select(x => new TaskViewModel(x, canEdit: true)).ToList();

            repo.Setup(x => x.GetByProject(projId))
                .Returns(tasks);
            projRepo.Setup(x => x.IsUserInProject(projId, userId.ToString()))
                .Returns(true);
            projRepo.Setup(x => x.IsUserProjectOwner(projId, userId.ToString()))
                .Returns(true);

            TasksManager mng = new(repo.Object, projRepo.Object);

            List<TaskViewModel> res = mng.GetTasksInProject(projId, userId.ToString());

            Assert.Equal(expected, res);
        }

        [Theory, AutoMoqData]
        public void GetTasksInProject_Calls_IsUserInProject_Once(string userId, string projId, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            projRepo.Setup(x => x.IsUserInProject(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            TasksManager mng = new(repo.Object, projRepo.Object);

            mng.GetTasksInProject(userId, projId);

            projRepo.Verify(x => x.IsUserInProject(userId, projId), Times.Once);
        }

        [Theory, AutoMoqData]
        public void GetTasksInProject_Calls_IsUserProjectOwner_Once(string userId, string projId, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            projRepo.Setup(x => x.IsUserInProject(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);
            projRepo.Setup(x => x.IsUserInProject(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(true);

            TasksManager mng = new(repo.Object, projRepo.Object);

            mng.GetTasksInProject(userId, projId);

            projRepo.Verify(x => x.IsUserProjectOwner(userId, projId), Times.Once);
        }


        [Theory]
        [InlineAutoMoqData("d", "discussion", 3, "medium")]
        public void Create_Adds_NewModel(string status, string statusCode, int priority, string priorityCode, string name, string projId, string userId, string description, DateTime deadline, string assessment, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            var task = new TaskModel
            {
                Name = name,
                Assessment = assessment,
                CreatedBy = new MongoDBRef("", userId),
                Deadline = deadline,
                Description = description,
                Priority = priority,
                Project = new MongoDBRef("", projId),
                Status = status
            };
            repo.Setup(x => x.Create(projId, userId, It.IsAny<TaskModel>()))
                .Returns(task);
            projRepo.Setup(x => x.IsUserInProject(projId, userId))
                .Returns(true);

            TasksManager mng = new(repo.Object, projRepo.Object);

            TaskViewModel? res = mng.Create(name, projId, userId, description, deadline, priorityCode, assessment);

            Assert.Equal(task.Name, res!.Name);
            Assert.Equal(task.Assessment, res.Assessment);
            Assert.Equal(priorityCode, res.Priority);
            Assert.Equal(task.Deadline, res.Deadline);
            Assert.Equal(task.Description, res.Description);
            Assert.Equal(statusCode, res.Status);
        }

        [Theory, AutoMoqData]
        public void Create_Checks_Access(string name, string projId, string userId, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            projRepo.Setup(x => x.IsUserInProject(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            TasksManager mng = new(repo.Object, projRepo.Object);

            mng.Create(name, projId, userId);

            projRepo.Verify(x => x.IsUserInProject(projId, userId));
        }


        [Theory]
        [InlineAutoMoqData(1, "lowest")]
        [InlineAutoMoqData(2, "low")]
        [InlineAutoMoqData(3, "medium")]
        [InlineAutoMoqData(4, "high")]
        [InlineAutoMoqData(5, "highest")]
        public void Change_Put_ChangedModel(int priority, string priorityCode, string id, string userId, string projId, string assessment, DateTime deadline, string description, string name, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            string ignored = "ignored";
            var dbRef = new MongoDBRef("", ignored);
            var task = new EditTaskModel
            {
                Assessment = assessment,
                Deadline = deadline,
                Description = description,
                Name = name,
                Priority = priorityCode
            };

            repo.Setup(x => x.GetById(id))
                .Returns(new TaskModel { Project = new MongoDBRef("", projId) });
            TaskModel passedTask = null!;
            repo.Setup(x => x.Update(It.IsAny<TaskModel>()))
                .Callback<TaskModel>(task => passedTask = task);
            projRepo.Setup(x => x.IsUserInProject(projId, userId))
                .Returns(true);

            TasksManager mng = new(repo.Object, projRepo.Object);

            mng.Change(id, userId, task);

            Assert.Equal(assessment, passedTask!.Assessment);
            Assert.Equal(deadline, passedTask!.Deadline);
            Assert.Equal(description, passedTask!.Description);
            Assert.Equal(name, passedTask!.Name);
            Assert.Equal(priority, passedTask!.Priority);
            Assert.NotEqual(ignored, passedTask!.Status);
            Assert.NotEqual(dbRef, passedTask!.CreatedBy);
            Assert.NotEqual(dbRef, passedTask!.Project);
            Assert.NotEqual(ignored, passedTask!.Id);
        }

        [Theory, AutoMoqData]
        public void Change_Checks_Access(string id, string userId, string projId, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            var editModel = new EditTaskModel();
            var task = new TaskModel { Project = new MongoDBRef("", projId) };
            repo.Setup(x => x.GetById(id)).Returns(task);
            projRepo.Setup(x => x.IsUserInProject(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(false);

            TasksManager mng = new(repo.Object, projRepo.Object);

            mng.Change(id, userId, editModel);

            projRepo.Verify(x => x.IsUserInProject(projId, userId));
        }


        [Theory]
        [InlineAutoMoqData("discussion")]
        [InlineAutoMoqData("inwork")]
        [InlineAutoMoqData("completed")]
        public void ChangeStatus_Put_CorrectStatus(string status, string id, string userId, string projId, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            var task = new TaskModel { Project = new MongoDBRef("", projId) };
            repo.Setup(x => x.GetById(id))
                .Returns(task);

            projRepo.Setup(x => x.IsUserInProject(It.IsAny<string>(), userId))
                .Returns(true);

            TasksManager mng = new(repo.Object, projRepo.Object);

            mng.ChangeStatus(id, userId, status);

            repo.Verify(x => x.UpdateStatus(id, It.IsNotNull<string>()), Times.Once);
        }

        [Theory]
        [InlineAutoMoqData("foo")]
        [InlineAutoMoqData(null!)]
        public void ChangeStatus_DoesntUpdate_IfStatus_NotCorrect(string status)
        {
            string id = "1";
            string userId = "1";
            var repo = new Mock<ITaskRepository>();
            var projRepo = new Mock<IProjectRepository>();
            projRepo.Setup(x => x.IsUserInProject(It.IsAny<string>(), userId))
                .Returns(true);

            TasksManager mng = new(repo.Object, projRepo.Object);

            mng.ChangeStatus(id, userId, status);

            repo.Verify(x => x.UpdateStatus(id, It.IsNotNull<string>()), Times.Never);
        }

        [Theory]
        [InlineAutoMoqData("discussion")]
        [InlineAutoMoqData("inwork")]
        [InlineAutoMoqData("completed")]
        public void ChangeStatus_Checks_Access(string status, string id, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            SetupTaskGet(id, out string userId, out string projId, repo, projRepo, out TaskModel task);

            TasksManager mng = new(repo.Object, projRepo.Object);

            mng.ChangeStatus(id, userId, status);

            projRepo.Verify(x => x.IsUserInProject(projId, userId), Times.Once);
        }


        [Theory, AutoMoqData]
        public void Delete_Calls_Delete_Once(string id, string userId, string projId, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            var task = new TaskModel
            {
                Id = id,
                Project = new MongoDBRef("", projId)
            };

            repo.Setup(x => x.GetById(id))
                .Returns(task);
            projRepo.Setup(x => x.IsUserProjectOwner(projId, userId))
                .Returns(true);

            TasksManager mng = new(repo.Object, projRepo.Object);

            mng.Delete(id, userId);

            repo.Verify(x => x.Delete(id), Times.Once);
        }

        [Theory, AutoMoqData]
        public void Delete_Checks_Owner(string id, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo)
        {
            SetupTaskGet(id, out string userId, out string projId, repo, projRepo, out TaskModel task);

            TasksManager mng = new(repo.Object, projRepo.Object);

            mng.Delete(id, userId);

            projRepo.Verify(x => x.IsUserProjectOwner(projId, userId), Times.Once);
        }

        private void SetupTaskGet(string id, out string userId, out string projId, Mock<ITaskRepository> repo, Mock<IProjectRepository> projRepo, out TaskModel task)
        {
            var projectIdObj = ObjectId.GenerateNewId();
            var userIdObj = ObjectId.GenerateNewId();

            userId = userIdObj.ToString();
            projId = projectIdObj.ToString();

            task = new TaskModel
            {
                Id = id,
                Project = new MongoDBRef("", projectIdObj),
                CreatedBy = new MongoDBRef("", userIdObj)
            };
            repo.Setup(x => x.GetById(id))
                .Returns(task);
            projRepo.Setup(x => x.IsUserInProject(projectIdObj.ToString(), userIdObj.ToString()))
                .Returns(true);
        }
    }
}
