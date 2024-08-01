using CSGOCheatDetector.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace CSGOCheatDetector.Services
{
    public class FileSearchService
    {
        private readonly List<string> _excludedNames;
        private readonly List<string> _excludedFolderNames;
        private readonly List<string> _suspiciousPatterns;
        private readonly Action<int> _updateStatus;

        public FileSearchService(List<string> excludedNames, List<string> excludedFolderNames, List<string> suspiciousPatterns, Action<int> updateStatus)
        {
            _excludedNames = excludedNames;
            _excludedFolderNames = excludedFolderNames;
            _suspiciousPatterns = suspiciousPatterns;
            _updateStatus = updateStatus;
        }

        public IEnumerable<SuspiciousFile> SearchSuspiciousFiles(string rootPath, string[] allowedExtensions, string[] systemDirectories, long minFileSize, CancellationToken token)
        {
            var suspiciousFiles = new ConcurrentBag<SuspiciousFile>();
            var directoriesToProcess = new ConcurrentQueue<string>();
            directoriesToProcess.Enqueue(rootPath);
            int processedItems = 0;

            while (directoriesToProcess.TryDequeue(out var currentDirectory))
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    foreach (var directory in SafeEnumerateDirectories(currentDirectory))
                    {
                        if (_excludedFolderNames.Any(excludedFolder => directory.IndexOf(excludedFolder, StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            continue;
                        }

                        directoriesToProcess.Enqueue(directory);
                    }

                    foreach (var file in SafeEnumerateFiles(currentDirectory))
                    {
                        if (token.IsCancellationRequested)
                        {
                            break;
                        }

                        if (allowedExtensions.Contains(Path.GetExtension(file), StringComparer.OrdinalIgnoreCase) &&
                            new FileInfo(file).Length >= minFileSize &&
                            !_excludedNames.Any(excludedName => file.IndexOf(excludedName, StringComparison.OrdinalIgnoreCase) >= 0) &&
                            _suspiciousPatterns.Any(pattern => file.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            var fileInfo = new FileInfo(file);
                            suspiciousFiles.Add(new SuspiciousFile
                            {
                                Name = fileInfo.Name,
                                Size = fileInfo.Length / 1024,
                                CreationDate = fileInfo.CreationTime,
                                ModificationDate = fileInfo.LastWriteTime,
                                AccessDate = fileInfo.LastAccessTime,
                                FullPath = fileInfo.FullName
                            });
                        }
                    }

                    processedItems++;
                    _updateStatus?.Invoke(processedItems);
                }
                catch (UnauthorizedAccessException)
                {
                    // Log or handle exception
                }
            }

            return suspiciousFiles;
        }

        private IEnumerable<string> SafeEnumerateDirectories(string path)
        {
            try
            {
                return Directory.EnumerateDirectories(path);
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<string>();
            }
            catch (PathTooLongException)
            {
                return Enumerable.Empty<string>();
            }
            catch (IOException)
            {
                return Enumerable.Empty<string>();
            }
        }

        private IEnumerable<string> SafeEnumerateFiles(string path)
        {
            try
            {
                return Directory.EnumerateFiles(path);
            }
            catch (UnauthorizedAccessException)
            {
                return Enumerable.Empty<string>();
            }
            catch (PathTooLongException)
            {
                return Enumerable.Empty<string>();
            }
            catch (IOException)
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
