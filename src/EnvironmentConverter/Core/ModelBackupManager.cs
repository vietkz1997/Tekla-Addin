using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BimCommands.EnvironmentConverter.Core
{
    /// <summary>
    /// Manages safe creation of full model backups before making any modifications.
    /// </summary>
    public static class ModelBackupManager
    {
        public delegate void BackupProgressHandler(string currentItem, int percent, string message);

        /// <summary>
        /// Creates a complete timestamped backup of the model directory.
        /// </summary>
        public static async Task<string> CreateModelBackupAsync(string modelPath, BackupProgressHandler progressCallback, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(modelPath) || !Directory.Exists(modelPath))
            {
                throw new DirectoryNotFoundException($"Model directory not found: {modelPath}");
            }

            string cleanPath = modelPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string parentDir = Path.GetDirectoryName(cleanPath);
            string modelName = Path.GetFileName(cleanPath);

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupDir = Path.Combine(parentDir, $"{modelName}_Backup_{timestamp}");

            progressCallback?.Invoke(modelName, 0, $"Khởi tạo thư mục sao lưu: {Path.GetFileName(backupDir)}...");

            Directory.CreateDirectory(backupDir);

            // Collect all files to copy
            var allFiles = Directory.GetFiles(cleanPath, "*.*", SearchOption.AllDirectories);
            int totalFiles = allFiles.Length;
            int copiedFiles = 0;

            await Task.Run(() =>
            {
                foreach (var file in allFiles)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string relativePath = file.Substring(cleanPath.Length + 1);
                    string targetFile = Path.Combine(backupDir, relativePath);
                    string targetFolder = Path.GetDirectoryName(targetFile);

                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }

                    // Use safe copy to handle locked database files (e.g. .db1, .db2, session.db)
                    SafeCopyFile(file, targetFile);

                    copiedFiles++;
                    if (copiedFiles % 25 == 0 || copiedFiles == totalFiles)
                    {
                        int pct = totalFiles > 0 ? (copiedFiles * 100) / totalFiles : 100;
                        progressCallback?.Invoke(Path.GetFileName(file), pct, $"Đang sao lưu: {copiedFiles}/{totalFiles} files ({pct}%)");
                    }
                }
            }, cancellationToken);

            progressCallback?.Invoke(backupDir, 100, $"Sao lưu hoàn tất! Thư mục lưu tại: {backupDir}");
            return backupDir;
        }

        private static void SafeCopyFile(string sourceFile, string targetFile)
        {
            try
            {
                File.Copy(sourceFile, targetFile, true);
                return;
            }
            catch (IOException)
            {
                // Fallback for files locked by Tekla (e.g. .db1, .db2, session.db)
                try
                {
                    using (var inStream = new FileStream(sourceFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var outStream = new FileStream(targetFile, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        inStream.CopyTo(outStream);
                    }
                    return;
                }
                catch { }
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch { }
        }
    }
}
