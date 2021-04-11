using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileCacheService
{
    internal class FileCache
    {
        private readonly TimeSpan _timeToLive = TimeSpan.FromSeconds(5);

        private readonly DateTime _lastCreatedOrUpdatedTime;

        internal Dictionary<string, List<string>> Files { get; }

        internal bool IsActual => (DateTime.UtcNow - _lastCreatedOrUpdatedTime) < _timeToLive;

        internal FileCache()
        {
            Files = new Dictionary<string, List<string>>();
            FillFilesCache();
            _lastCreatedOrUpdatedTime = DateTime.UtcNow;
        }


        private void FillFilesCache()
        {
            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives.Where(d => d.DriveType == DriveType.Fixed))
            {
                FillFilesCache(drive.RootDirectory, 3);
            }
        }

        private void FillFilesCache(DirectoryInfo directory, int level = 0)
        {
            if (level == 0)
            {
                try
                {
                    AddFilesToCache(directory.GetFiles());
                }
                catch (DirectoryNotFoundException) { }
                return;
            }
            Stack<DirectoryInfo> directories = new();
            directories.Push(directory);
            while (directories.Count > 0)
            {
                var currentDirectory = directories.Pop();
                DirectoryInfo[] subDirectories;
                try
                {
                    subDirectories = currentDirectory.GetDirectories();
                }
                catch (UnauthorizedAccessException)
                {
                    continue;
                }
                catch (DirectoryNotFoundException)
                {
                    continue;
                }

                try
                {
                    var files = currentDirectory.GetFiles();
                    AddFilesToCache(files);
                }
                catch (DirectoryNotFoundException)
                {
                    continue;
                }

                if (subDirectories.Length > 0 && subDirectories.All(d => d.FullName.Count(c => c == '\\') <= level))
                {
                    foreach (var subDirectory in subDirectories)
                    {
                        directories.Push(subDirectory);
                    }
                }
            }
        }

        private void AddFilesToCache(IEnumerable<FileInfo> files)
        {
            foreach (var file in files)
            {
                if (Files.ContainsKey(file.Extension))
                {
                    Files[file.Extension].Add(file.FullName);
                }
                else
                {
                    Files.Add(file.Extension, new List<string> { file.FullName });
                }
            }
        }
    }
}
