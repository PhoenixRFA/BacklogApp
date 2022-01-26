using BacklogApp.Models.Db;
using BacklogApp.Models.Resources;
using BacklogApp.Repository;
using MongoDB.Bson;
using MongoDB.Driver.GridFS;

namespace BacklogApp.Managers
{
    public class ResourcesManager
    {
        private readonly IGridFSBucket _gridFs;
        private readonly IResourceRepository _repo;

        public ResourcesManager(IResourceRepository repo, IGridFSBucket gridFs)
        {
            _gridFs = gridFs;
            _repo = repo;
        }

        public ResourceViewModel? Get(string id)
        {
            ResourceModel? res = _repo.GetById(id);
            if (res == null) return null;

            var stream = new MemoryStream();
            _gridFs.DownloadToStream(res.FileId, stream);

            stream.Seek(0, SeekOrigin.Begin);

            return new ResourceViewModel
            {
                FileName = res.OriginalName,
                FileStream = stream,
                MimeType = res.MimeType
            };
        }

        public string Upload(Stream stream, string userId, string filename, string mimeType, string code)
        {
            string ext = Path.GetExtension(filename);
            string generatedFilename = $"{Guid.NewGuid():N}{ext}";

            ObjectId id = _gridFs.UploadFromStream(generatedFilename, stream);

            var res = new ResourceModel
            {
                FileId = id,
                OriginalName = filename,
                MimeType = mimeType,
                Code = code,
                Owner = ObjectId.Parse(userId)
            };

            _repo.Create(res);

            return res.Id;
        }

        public bool Delete(string id, string userId)
        {
            ResourceModel? res = _repo.GetById(id);
            if (res == null || res.Owner != ObjectId.Parse(userId)) return false;

            _gridFs.Delete(res.FileId);

            _repo.Delete(id);
            return true;
        }
    }
}
