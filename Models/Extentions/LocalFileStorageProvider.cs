using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace WebDtt.Models.Extentions
{
    public class LocalFileStorageProvider
    {
        private readonly string _rootPath;
        public LocalFileStorageProvider(string rootPath)
        {
            _rootPath = rootPath;
        }

        public async Task<byte[]> Get<T>(Guid id, string fileName)
        {
            var path = _getPath<T>(id, fileName);
            return await _getFile(path);
        }

        public Stream GetStream<T>(string fileName)
        {
            var path = _getPath<T>(fileName);
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public async Task<byte[]> _getFile(string path)
        {
            if (!System.IO.File.Exists(path))
                throw new FileNotFoundException($"{path} не найден в хранилище сервера");
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                var result = new byte[fs.Length];
                await fs.ReadAsync(result, 0, (int)fs.Length);
                return result;
            }
        }

        public async Task<byte[]> Get<T>(string fileName)
        {
            var path = _getPath<T>(fileName);
            return await _getFile(path);
        }

        public async Task<bool> Put<T>(Guid id, HttpPostedFile file)
        {
            var preRoot = Path.Combine(_rootPath, typeof(T).Name, id.ToString());
            if (!Directory.Exists(preRoot))
                Directory.CreateDirectory(preRoot);
            var path = _getPath<T>(id, file.FileName);
            await Task.Factory.StartNew(() => { file.SaveAs(path); });
            return true;
        }

        public async Task<bool> Delete<T>(Guid id, string fileName)
        {
            try
            {
                var path = _getPath<T>(id, fileName);
                await DoCleanup(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public async Task<bool> Delete<T>(string fileName)
        {
            try
            {
                var path = _getPath<T>(fileName);
                await DoCleanup(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private string _getPath<T>(string fileName) => Path.Combine(_rootPath, typeof(T).Name, fileName);
        private string _getPath<T>(Guid id, string fileName) => Path.Combine(_rootPath, typeof(T).Name, id.ToString(), fileName);

        public async Task DoCleanup(string path, int tryCount = 5, int delay = 10)
        {
            var attr = System.IO.File.GetAttributes(path);
            var isDirectory = (attr & FileAttributes.Directory) == FileAttributes.Directory;
            for (int i = 0; i < tryCount; i++)
            {
                try
                {
                    bool result = false;
                    result = !isDirectory ? Cleanup(new FileInfo(path)) : Cleanup(new DirectoryInfo(path));
                    if (result)
                        return;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(delay));
            }
        }

        private bool Cleanup(DirectoryInfo path)
        {
            if (!path.Exists) return true;
            path.Delete(true);
            return true;
        }
        private bool Cleanup(FileInfo path)
        {
            if (!path.Exists) return true;
            path.Delete();
            return true;
        }

    }
}