using BacklogApp.Managers;
using BacklogApp.Models.Auth;
using BacklogApp.Models.Db;
using BacklogApp.Models.Projects;
using BacklogApp.Repository;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BacklogApp.Tests.Managers
{
    public class ProjectManagerTests
    {
        [Theory, AutoMoqData]
        public void Get_Returns_Project(string id, int count, List<UserModel> userModels, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo, Mock<IUsersRepository> userRepo)
        {
            var userId = ObjectId.GenerateNewId();
            var proj = new ProjectModel { Id = id, Owner = new MongoDBRef("",  userId) };
            List<UserViewModel> projectUsers = userModels.Select(x => new UserViewModel(x)).ToList();

            repo.Setup(x => x.GetById(proj.Id))
                .Returns(proj);
            repo.Setup(x => x.IsUserInProject(id, userId.ToString()))
                .Returns(true);
            taskRepo.Setup(x=>x.CountTasksInProject(proj.Id))
                .Returns(count);
            userRepo.Setup(x=>x.GetByIds(It.IsAny<IEnumerable<string>>()))
                .Returns(userModels);

            ProjectsManager mng = new(repo.Object, taskRepo.Object, userRepo.Object);

            ProjectViewModel? res = mng.Get(id, userId.ToString());

            Assert.NotNull(res);
            Assert.Equal(proj.Id, res!.Id);
            Assert.True(res.CanEdit);
            Assert.Equal(count, res.TasksCount);
            Assert.Equal(projectUsers, res.Users);
        }

        [Theory, AutoMoqData]
        public void Get_Calls_CountTasks_Once(string id, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo, Mock<IUsersRepository> userRepo)
        {
            var userId = ObjectId.GenerateNewId();
            var proj = new ProjectModel { Id = id, Owner = new MongoDBRef("", userId) };
            repo.Setup(x => x.GetById(proj.Id))
                .Returns(proj);
            repo.Setup(x => x.IsUserInProject(id, userId.ToString()))
                .Returns(true);

            ProjectsManager mng = new(repo.Object, taskRepo.Object, userRepo.Object);

            ProjectViewModel? res = mng.Get(id, userId.ToString());

            taskRepo.Verify(x=>x.CountTasksInProject(proj.Id), Times.Once);
        }


        [Theory, AutoMoqData]
        public void GetUserProjects_Returns_ProjectsList(string[] ids, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            var userId = ObjectId.GenerateNewId();
            List<ProjectModel> projects = ids.Select(x => new ProjectModel { Id = x, Owner = new MongoDBRef("", userId) }).ToList();
            List<ProjectViewModel> expected = projects.Select(x => new ProjectViewModel { Id = x.Id, CanEdit = true }).ToList();

            repo.Setup(x => x.GetUserProjects(userId.ToString()))
                .Returns(projects);

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            List<ProjectViewModel>? res = mng.GetUserProjects(userId.ToString());

            Assert.NotNull(res);
            Assert.NotEmpty(res);
            Assert.Equal(expected, res);
        }

        [Theory, AutoMoqData]
        public void GetUserProjects_Calls_CountTasks_Once(string[] ids, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            var userId = ObjectId.GenerateNewId();
            List<ProjectModel> projects = ids.Select(x => new ProjectModel { Id = x, Owner = new MongoDBRef("", userId) }).ToList();

            repo.Setup(x => x.GetUserProjects(userId.ToString()))
                .Returns(projects);

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            List<ProjectViewModel>? res = mng.GetUserProjects(userId.ToString());

            taskRepo.Verify(x=>x.CountTasksInProjects(It.IsAny<IEnumerable<string>>()), Times.Once);
        }


        [Theory, AutoMoqData]
        public void Create_Calls_Create_Once(string name, string ownerId, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.Create(name, ownerId);

            repo.Verify(x => x.Create(name, ownerId), Times.Once);
        }

        [Theory, AutoMoqData]
        public void Create_Returns_NewProject(string id, string ownerId, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            var proj = new ProjectModel { Id = id, Owner = new MongoDBRef("", ownerId) };

            repo.Setup(x => x.Create(proj.Name, ownerId))
                .Returns(proj);

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            ProjectViewModel? res = mng.Create(proj.Name, ownerId);

            Assert.NotNull(res);
            Assert.Equal(proj.Id, res.Id);
        }


        [Theory, AutoMoqData]
        public void ChangeName_Calls_GetById_Once(string id, string newName, string ownerId, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            repo.Setup(x => x.GetById(id))
                .Returns((ProjectModel?)null);

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.ChangeName(id, newName, ownerId);

            repo.Verify(x => x.GetById(id), Times.Once);
        }

        [Theory, AutoMoqData]
        public void ChangeName_Calls_UpdateName_Once(string id, string newName, ObjectId ownerId, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            repo.Setup(x => x.GetById(id))
                .Returns(new ProjectModel { Id = id, Owner = new MongoDBRef("", ownerId) });

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.ChangeName(id, newName, ownerId.ToString());

            repo.Verify(x => x.UpdateName(id, newName), Times.Once);
        }

        [Theory, AutoMoqData]
        public void ChangeName_DoesntCall_UpdateName_IfOwnerMissmatch(string id, string newName, ObjectId ownerId, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            repo.Setup(x => x.GetById(id))
                .Returns(new ProjectModel { Owner = new MongoDBRef("", "") });

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.ChangeName(id, newName, ownerId.ToString());

            repo.Verify(x => x.UpdateName(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }


        [Theory, AutoMoqData]
        public void ChangeUsers_Calls_GetById_Once(string id, string[] userIds, string ownerId, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            repo.Setup(x => x.GetById(id))
                .Returns((ProjectModel?)null);

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.ChangeUsers(id, userIds, ownerId);

            repo.Verify(x => x.GetById(id), Times.Once);
        }

        [Theory, AutoMoqData]
        public void ChangeUsers_Calls_UpdateUsers_Once(string id, string[] userIds, ObjectId ownerId, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            repo.Setup(x => x.GetById(id))
                .Returns(new ProjectModel { Id = id, Owner = new MongoDBRef("", ownerId) });

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.ChangeUsers(id, userIds, ownerId.ToString());

            repo.Verify(x => x.UpdateUsers(id, userIds), Times.Once);
        }

        [Theory, AutoMoqData]
        public void ChangeUsers_DoesntCall_UpdateUsers_IfOwnerMissmatch(string id, string[] userIds, ObjectId ownerId, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            repo.Setup(x => x.GetById(id))
                .Returns(new ProjectModel { Owner = new MongoDBRef("", "") });

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.ChangeUsers(id, userIds, ownerId.ToString());

            repo.Verify(x => x.UpdateUsers(It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }


        [Theory, AutoMoqData]
        public void AddUser_Insert_NewUserId_Inro_UserIds(string id, string newUserId, ObjectId ownerId, string[] userIds, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            List<MongoDBRef> users = userIds.Select(x => new MongoDBRef("", x)).ToList();
            var proj = new ProjectModel { Users = users, Owner = new MongoDBRef("", ownerId) };

            repo.Setup(x => x.GetById(id))
                .Returns(proj);
            string[] ids = null!;
            repo.Setup(x => x.UpdateUsers(It.IsAny<string>(), It.IsAny<ICollection<string>>()))
                .Callback<string, ICollection<string>>((id, users) => ids = users.ToArray());

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.AddUser(id, newUserId, ownerId.ToString());

            Assert.Contains(newUserId, ids);
        }

        [Theory, AutoMoqData]
        public void AddUser_DoesntCall_UpdateUsers_IfOwnerMissmatch(string id, string newUserId, ObjectId ownerId, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            repo.Setup(x => x.GetById(id))
                .Returns(new ProjectModel { Owner = new MongoDBRef("", "") });

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.AddUser(id, newUserId, ownerId.ToString());

            repo.Verify(x => x.UpdateUsers(It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }


        [Theory, AutoMoqData]
        public void RemoveUser_Removes_UserId_From_UserIds(string id, ObjectId userId, ObjectId ownerId, string[] userIds, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            List<MongoDBRef> users = userIds.Select(x => new MongoDBRef("", x)).ToList();
            users.Add(new MongoDBRef("", userId));
            var proj = new ProjectModel { Users = users, Owner = new MongoDBRef("", ownerId) };

            repo.Setup(x => x.GetById(id))
                .Returns(proj);
            string[] ids = null!;
            repo.Setup(x => x.UpdateUsers(It.IsAny<string>(), It.IsAny<ICollection<string>>()))
                .Callback<string, ICollection<string>>((id, users) => ids = users.ToArray());

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.RemoveUser(id, userId.ToString(), ownerId.ToString());

            Assert.DoesNotContain(userId.ToString(), ids);
        }

        [Theory, AutoMoqData]
        public void RemoveUser_DoesntCall_UpdateUsers_IfOwnerMissmatch(string id, ObjectId userId, ObjectId ownerId, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            repo.Setup(x => x.GetById(id))
                .Returns(new ProjectModel { Owner = new MongoDBRef("", "") });

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.RemoveUser(id, userId.ToString(), ownerId.ToString());

            repo.Verify(x => x.UpdateUsers(It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        }


        [Theory, AutoMoqData]
        public void Delete_Calls_Delete_Once(string id, ObjectId ownerId, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            repo.Setup(x => x.GetById(id))
                .Returns(new ProjectModel { Id = id, Owner = new MongoDBRef("", ownerId) });

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.Delete(id, ownerId.ToString());

            repo.Verify(x => x.Delete(id), Times.Once);
        }

        [Theory, AutoMoqData]
        public void Delete_DoesntCall_Delete_IfOwnerMissmatch(string id, ObjectId ownerId, Mock<IProjectRepository> repo, Mock<ITaskRepository> taskRepo)
        {
            repo.Setup(x => x.GetById(id))
                .Returns(new ProjectModel { Owner = new MongoDBRef("", "") });

            ProjectsManager mng = new(repo.Object, taskRepo.Object, null!);

            mng.Delete(id, ownerId.ToString());

            repo.Verify(x => x.Delete(id), Times.Never);
        }
    }
}
