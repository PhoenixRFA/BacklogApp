using BacklogApp.Managers;
using BacklogApp.Models.Db;
using BacklogApp.Models.Resources;
using BacklogApp.Repository;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace BacklogApp.Tests.Managers
{
    public class ResourcesManagerTests
    {
        [Theory, AutoMoqData]
        public void Get_Returns_Resource(string id, string mime, string filename, Mock<IResourceRepository> repo, Mock<IGridFSBucket> gridFs)
        {
            var resource = new ResourceModel
            {
                FileId = ObjectId.GenerateNewId(),
                MimeType = mime,
                OriginalName = filename
            };
            repo.Setup(x=>x.GetById(id))
                .Returns(resource);

            ResourcesManager mng = new(repo.Object, gridFs.Object);

            ResourceViewModel? res = mng.Get(id);

            Assert.NotNull(res);
            Assert.Equal(mime, res!.MimeType);
            Assert.Equal(filename, res.FileName);
        }

        [Theory, AutoMoqData]
        public void Get_Calls_GetById_Once(string id, Mock<IResourceRepository> repo, Mock<IGridFSBucket> gridFs)
        {
            ResourcesManager mng = new(repo.Object, gridFs.Object);

            ResourceViewModel? res = mng.Get(id);

            repo.Verify(x => x.GetById(id), Times.Once);
        }

        [Theory, AutoMoqData]
        public void Get_Calls_DownloadToStream_Once(string id, Mock<IResourceRepository> repo, Mock<IGridFSBucket> gridFs)
        {
            var resource = new ResourceModel
            {
                FileId = ObjectId.GenerateNewId()
            };
            repo.Setup(x => x.GetById(id))
                .Returns(resource);
            ResourcesManager mng = new(repo.Object, gridFs.Object);

            ResourceViewModel? res = mng.Get(id);

            gridFs.Verify(x => x.DownloadToStream(resource.FileId, It.IsAny<Stream>(), It.IsAny<GridFSDownloadOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }


        [Theory, AutoMoqData]
        public void Upload_Returns_ResourceId(string id, Stream s, string filename, string ext, string mime, string code, Mock<IResourceRepository> repo, Mock<IGridFSBucket> gridFs)
        {
            ObjectId userId = ObjectId.GenerateNewId();
            ObjectId fileId = ObjectId.GenerateNewId();
            string file = $"{filename}.{ext}";

            gridFs.Setup(x=>x.UploadFromStream(file, It.IsAny<Stream>(), It.IsAny<GridFSUploadOptions>(), It.IsAny<CancellationToken>()))
                .Returns(fileId);
            repo.Setup(x=>x.Create(It.IsAny<ResourceModel>()))
                .Callback<ResourceModel>(x => x.Id = id);

            ResourcesManager mng = new(repo.Object, gridFs.Object);

            string res = mng.Upload(s, userId.ToString(), file, mime, code);

            Assert.Equal(id, res);
        }

        [Theory, AutoMoqData]
        public void Upload_Calls_UploadFromStream_Once(Stream s, string filename, string ext, string mime, string code, Mock<IResourceRepository> repo, Mock<IGridFSBucket> gridFs)
        {
            ObjectId userId = ObjectId.GenerateNewId();
            string file = $"{filename}.{ext}";

            ResourcesManager mng = new(repo.Object, gridFs.Object);

            mng.Upload(s, userId.ToString(), file, mime, code);
            
            gridFs.Verify(x => x.UploadFromStream(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<GridFSUploadOptions>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory, AutoMoqData]
        public void Upload_Calls_Create_Once(Stream s, string filename, string ext, string mime, string code, Mock<IResourceRepository> repo, Mock<IGridFSBucket> gridFs)
        {
            ObjectId userId = ObjectId.GenerateNewId();
            ObjectId fileId = ObjectId.GenerateNewId();
            string file = $"{filename}.{ext}";

            gridFs.Setup(x => x.UploadFromStream(file, It.IsAny<Stream>(), It.IsAny<GridFSUploadOptions>(), It.IsAny<CancellationToken>()))
                .Returns(fileId);

            ResourcesManager mng = new(repo.Object, gridFs.Object);

            mng.Upload(s, userId.ToString(), file, mime, code);
            
            repo.Verify(x => x.Create(It.IsAny<ResourceModel>()), Times.Once);
        }


        [Theory, AutoMoqData]
        public void Delete_Returns_True_OnSuccessfullDelete(Mock<IResourceRepository> repo, Mock<IGridFSBucket> gridFs)
        {
            ObjectId id = ObjectId.GenerateNewId();
            ObjectId userId = ObjectId.GenerateNewId();
            var resource = new ResourceModel
            {
                FileId = ObjectId.GenerateNewId(),
                Owner = userId
            };

            repo.Setup(x=>x.GetById(id.ToString()))
                .Returns(resource);

            ResourcesManager mng = new(repo.Object, gridFs.Object);

            bool res = mng.Delete(id.ToString(), userId.ToString());

            Assert.True(res);
        }

        [Theory, AutoMoqData]
        public void Delete_Returns_False_OnFileNotFound(Mock<IResourceRepository> repo, Mock<IGridFSBucket> gridFs)
        {
            ObjectId id = ObjectId.GenerateNewId();
            ObjectId userId = ObjectId.GenerateNewId();
            
            repo.Setup(x => x.GetById(id.ToString()))
                .Returns((ResourceModel?)null);

            ResourcesManager mng = new(repo.Object, gridFs.Object);

            bool res = mng.Delete(id.ToString(), userId.ToString());

            Assert.False(res);
        }

        [Theory, AutoMoqData]
        public void Delete_Returns_False_OnOwnerMissmatch(Mock<IResourceRepository> repo, Mock<IGridFSBucket> gridFs)
        {
            ObjectId id = ObjectId.GenerateNewId();
            ObjectId userId = ObjectId.GenerateNewId();
            var resource = new ResourceModel
            {
                FileId = ObjectId.GenerateNewId(),
                Owner = id
            };

            repo.Setup(x => x.GetById(id.ToString()))
                .Returns(resource);

            ResourcesManager mng = new(repo.Object, gridFs.Object);

            bool res = mng.Delete(id.ToString(), userId.ToString());

            Assert.False(res);
        }

        [Theory, AutoMoqData]
        public void Delete_Calls_GetById_Once(Mock<IResourceRepository> repo, Mock<IGridFSBucket> gridFs)
        {
            ObjectId id = ObjectId.GenerateNewId();
            ObjectId userId = ObjectId.GenerateNewId();

            ResourcesManager mng = new(repo.Object, gridFs.Object);

            mng.Delete(id.ToString(), userId.ToString());

            repo.Verify(x => x.GetById(id.ToString()), Times.Once);
        }

        [Theory, AutoMoqData]
        public void Delete_Calls_Delete_Once(Mock<IResourceRepository> repo, Mock<IGridFSBucket> gridFs)
        {
            ObjectId id = ObjectId.GenerateNewId();
            ObjectId userId = ObjectId.GenerateNewId();
            var resource = new ResourceModel
            {
                FileId = ObjectId.GenerateNewId(),
                Owner = userId
            };

            repo.Setup(x => x.GetById(id.ToString()))
                .Returns(resource);

            ResourcesManager mng = new(repo.Object, gridFs.Object);

            mng.Delete(id.ToString(), userId.ToString());

            repo.Verify(x=>x.Delete(id.ToString()), Times.Once);
            gridFs.Verify(x=>x.Delete(resource.FileId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
