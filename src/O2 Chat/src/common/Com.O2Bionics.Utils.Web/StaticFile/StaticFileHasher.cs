using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Web;

namespace Com.O2Bionics.Utils.Web.StaticFile
{
    public static class StaticFileHasher
    {
        private static readonly ConcurrentDictionary<FilePathAndLastUpdate, string> m_staticFileHashes =
            new ConcurrentDictionary<FilePathAndLastUpdate, string>(FilePathAndLastUpdate.DefaultComparer);

        public static string GetFileHash(string path)
        {
            return File.Exists(path)
                ? m_staticFileHashes.GetOrAdd(new FilePathAndLastUpdate(path), f => ComputeFileHash(f.Path))
                : string.Empty;
        }

        private static string ComputeFileHash(string path)
        {
            byte[] fileHash;
            using (var fs = File.OpenRead(path))
            {
                using (var hasher = new SHA256Managed())
                {
                    fileHash = hasher.ComputeHash(fs);
                }
            }
            return HttpServerUtility.UrlTokenEncode(fileHash);
        }

        private struct FilePathAndLastUpdate
        {
            public FilePathAndLastUpdate(string path)
            {
                Path = path;
                LastUpdate = File.GetLastWriteTimeUtc(path);
            }

            public readonly string Path;
            // ReSharper disable once MemberCanBePrivate.Local
            public readonly DateTime LastUpdate;

            private sealed class Comparer : IEqualityComparer<FilePathAndLastUpdate>
            {
                public bool Equals(FilePathAndLastUpdate x, FilePathAndLastUpdate y)
                {
                    return x.LastUpdate.Equals(y.LastUpdate)
                           && string.Equals(x.Path, y.Path, StringComparison.OrdinalIgnoreCase);
                }

                public int GetHashCode(FilePathAndLastUpdate obj)
                {
                    unchecked
                    {
                        return (obj.LastUpdate.GetHashCode() * 397)
                               ^ StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Path);
                    }
                }
            }

            private static readonly IEqualityComparer<FilePathAndLastUpdate> m_defaultComparerInstance = new Comparer();
            public static IEqualityComparer<FilePathAndLastUpdate> DefaultComparer
            {
                get { return m_defaultComparerInstance; }
            }
        }
    }
}