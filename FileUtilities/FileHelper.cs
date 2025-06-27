using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Qynix.EAP.Utilities.FileUtilities
{
    public static class FileHelper
    {
        public static string GetFileNameFromFilePath(string filePath)
        {
            var fi = new FileInfo(filePath);

            return fi.Name;
        }

        public static void CreateDirIfNotExist(string path, out string text)
        {
            var exist = Directory.Exists(path);

            if (!exist)
            {
                text = "Path:" + path + " not exist. Create directory.";
                Directory.CreateDirectory(path);
            }
            else
            {
                text = "Path:" + path + " existed.";
            }
        }

        public static void AsyncMD5ChecksumCompare(string sourceFile, byte[] compareData, string replySubject, int retryCount, int retryInterval, Action<object, bool, bool> waitOrTimeoutCallback)
        {
            ThreadPool.QueueUserWorkItem(
                (object state) =>
                {
                    if (!File.Exists(sourceFile))
                    {
                        waitOrTimeoutCallback(new string[] { sourceFile, replySubject, "File " + sourceFile + " not exist!" }, false, false);
                        return;
                    }

                    var data = new byte[0];
                    
                    while (true)
                    {
                        try
                        {
                            var fs = File.Open(sourceFile, FileMode.Open);

                            using (var br = new BinaryReader(fs))
                            {
                                data = new byte[br.BaseStream.Length];

                                for (int i = 0; i < data.Length; i++)
                                {
                                    data[i] = br.ReadByte();
                                }

                                br.Close();
                            }

                            fs.Close();
                            fs.Dispose();

                            using (var md5 = MD5.Create())
                            {
                                var hash1 = md5.ComputeHash(data);
                                var hash2 = md5.ComputeHash(compareData);

                                waitOrTimeoutCallback(new string[] { sourceFile, replySubject, "Binary file:" + sourceFile + " retrieve success." }, true, hash1.SequenceEqual(hash2));
                            }

                            break;
                        }
                        catch (FileLoadException)
                        {
                            if (retryCount-- > 0)
                            {
                                Thread.Sleep(retryInterval);
                                continue;
                            }
                            else
                            {
                                waitOrTimeoutCallback(new string[] { sourceFile, replySubject, "Unable to access the file, File:" + sourceFile }, false, false);
                            }
                        }
                    }

                });
        }

        /// <summary>
        /// This function is for writing binary file asynchronously and fire a callback regardless a failed or success attempt.
        /// </summary>
        /// <param name="path">The file path for the binary data to be saved.</param>
        /// <param name="data">The byte array of binary data.</param>
        /// <param name="retryCount">The amount of retry if failed attempt.</param>
        /// <param name="retryInterval">The milliseconds wait for the next retry.</param>
        /// <param name="callBack">The callback function.</param>
        /// <returns></returns>
        public static void AsyncWriteBinaryFile(string path, byte[] data, int retryCount, int retryInterval, Action<object, bool> callBack)
        {
            AsyncWriteBinaryFile(path, data, string.Empty, retryCount, retryInterval, callBack);
        }

        public static void AsyncWriteBinaryFile(string path, byte[] data, string replySubject, int retryCount, int retryInterval, Action<object, bool> callBack)
        {

            ThreadPool.QueueUserWorkItem(
                (object state) =>
                {
                    var deleteRetryCount = retryCount;
                    var writeRetryCount = retryCount;

                    if (File.Exists(path))
                    {
                        while (true)
                        {
                            try
                            {
                                Console.WriteLine("Deleting File:{0}", path);
                                File.Delete(path);
                                Console.WriteLine("Deleted File:{0}", path);
                                break;
                            }
                            catch (FileLoadException ex)
                            {
                                if (deleteRetryCount-- > 0)
                                {
                                    Thread.Sleep(retryInterval);
                                    continue;
                                }
                                else
                                {
                                    callBack(new string[] { path, "Delete file failed. Retried " + retryCount + " times. Ex:" + ex.ToString(), replySubject }, false);
                                    //throw new Exception("Delete file failed.", ex.InnerException);
                                    return;
                                }
                            }
                        }

                    }

                    Console.WriteLine("Writing file:{0}", path);

                    while (true)
                    {
                        try
                        {
                            using (var fs = new FileStream(path, FileMode.OpenOrCreate))
                            using (var bw = new BinaryWriter(fs))
                            {
                                foreach (var val in data)
                                {
                                    bw.Write(val);
                                }

                                bw.Flush();
                                fs.Flush();
                                bw.Close();
                                fs.Close();
                            }
                            break;
                        }
                        catch (Exception ex)
                        {
                            if (writeRetryCount-- > 0)
                            {
                                Thread.Sleep(retryInterval);
                                continue;
                            }
                            else
                            {
                                callBack(new string[] { path, "Write file failed. Retried " + retryCount + " times. Ex:" + ex.ToString(), replySubject }, false);
                                //throw new Exception("Delete file failed.", ex.InnerException);
                                return;
                            }
                        }
                    }


                    callBack(new string[] { path, "Write file:" + path + " success!", replySubject }, true);
                }
            );
        }

        /// <summary>
        /// This function is for writing binary file asynchronously and fire a callback regardless a failed or success attempt.
        /// </summary>
        /// <param name="path">The file path for the binary data to be read.</param>
        /// <param name="retryCount">The amount of retry if failed attempt.</param>
        /// <param name="retryInterval">The milliseconds wait for the next retry.</param>
        /// <param name="waitOrTimeoutCallback">The callback function.</param>
        /// <returns></returns>
        public static void AsyncReadBinaryFile(string path, string replySubject, int retryCount, int retryInterval, Action<object, bool, byte[]> waitOrTimeoutCallback)
        {
            ThreadPool.QueueUserWorkItem(
                (object state) =>
                {
                    if (!File.Exists(path))
                    {
                        waitOrTimeoutCallback(new string[] { path, replySubject, "File " + path + " not exist!" }, false, null);
                        return;
                    }

                    var data = new byte[0];

                    while (true)
                    {
                        try
                        {
                            var fs = File.Open(path, FileMode.Open);

                            using (var br = new BinaryReader(fs))
                            {
                                data = new byte[br.BaseStream.Length];

                                for (int i = 0; i < data.Length; i++)
                                {
                                    data[i] = br.ReadByte();
                                }

                                br.Close();
                            }

                            fs.Close();
                            fs.Dispose();

                            waitOrTimeoutCallback(new string[] { path, replySubject, "Binary file:" + path + " retrieve success." }, true, data);
                            break;
                        }
                        catch (FileLoadException)
                        {
                            if (retryCount-- > 0)
                            {
                                Thread.Sleep(retryInterval);
                                continue;
                            }
                            else
                            {
                                waitOrTimeoutCallback(new string[] { path, replySubject, "Unable to access the file, File:" + path }, false, null);
                            }
                        }
                    }
                });
        }

        public static void AsyncReadFile(string path, string replySubject, int retryCount, int retryInterval, Action<object, bool, string[]> waitOrTimeoutCallback)
        {
            ThreadPool.QueueUserWorkItem(
                (object state) =>
                {
                    if (!File.Exists(path))
                    {
                        waitOrTimeoutCallback(new string[] { path, replySubject, "File " + path + " not exist!" }, false, null);
                        return;
                    }

                    var data = new byte[0];

                    while (true)
                    {
                        try
                        {
                            var strings = File.ReadAllLines(path);

                            waitOrTimeoutCallback(new string[] { path, replySubject, "File:" + path + " retrieve success." }, true, strings);
                            break;
                        }
                        catch (FileLoadException)
                        {
                            if (retryCount-- > 0)
                            {
                                Thread.Sleep(retryInterval);
                                continue;
                            }
                            else
                            {
                                waitOrTimeoutCallback(new string[] { path, replySubject, "Unable to access the file, File:" + path }, false, null);
                            }
                        }
                    }
                });
        }

        public static void AsyncExtractFile(string compressedFile, string targetFile, string replySubject, int retryCount, int retryInterval, Action<object, bool, string[]> waitOrTimeoutCallback)
        {
#if DEBUG
            //targetFile = Directory.GetCurrentDirectory().ToString() + @"\ExternalExecutable\" + Path.GetFileName(Path.GetDirectoryName(compressedFile)) + @"\" + targetFile;
#else
            targetFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString() + @"\ExternalExecutable\" + Path.GetFileName(Path.GetDirectoryName(compressedFile)) + @"\" + targetFile;
#endif
            ThreadPool.QueueUserWorkItem(
                (object state) =>
                {
                    string arguments = string.Format(" -xvf {0}", compressedFile);

                    var startInfo = new ProcessStartInfo();
                    startInfo.FileName = "cmd";
#if DEBUG
                    startInfo.Arguments = " /c cd " + Directory.GetCurrentDirectory().ToString() + @"\ExternalExecutable" + @"\" + Path.GetFileName(Path.GetDirectoryName(compressedFile)) + @" &"
                     + Directory.GetCurrentDirectory().ToString() + @"\ExternalExecutable\tar.exe " + arguments;
#else
                    startInfo.Arguments = " /c cd " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString() + @"\ExternalExecutable" + @"\" + Path.GetFileName(Path.GetDirectoryName(compressedFile)) + @" &"
                     + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location).ToString() + @"\ExternalExecutable\tar.exe " + arguments;
                    //startInfo.Arguments = " /c cd " + @"C:\test\QynixEAP\Source\EAP\EAPController\bin\Release\ExternalExecutable &"
                    // + @"C:\test\QynixEAP\Source\EAP\EAPController\bin\Release\ExternalExecutable\tar.exe " + arguments;
#endif
                    startInfo.CreateNoWindow = true;
                    startInfo.UseShellExecute = false;
                    startInfo.RedirectStandardError = true;
                    startInfo.RedirectStandardInput = true;
                    startInfo.RedirectStandardOutput = true;
                    startInfo.ErrorDialog = false;
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    //using (var process = Process.Start(startInfo))
                    //{
                    //    process.WaitForExit();              
                    //}
                    Process proc = new Process();
                    proc.StartInfo = startInfo;
                    proc.Start();
                    proc.WaitForExit();

                    int lCount = 0;
                    while (true)
                    {
                        lCount++;
                        if (File.Exists(targetFile))
                            break;

                        if (!File.Exists(targetFile) && lCount > 100)
                        {
                            waitOrTimeoutCallback(new string[] { compressedFile, targetFile, replySubject }, false, null);
                            return;
                        }
                    }

                    // Open file
                    var strings = File.ReadAllLines(targetFile);

                    try
                    {
                        File.Delete(targetFile);
                    }
                    catch { }

                    waitOrTimeoutCallback(new string[] { compressedFile, targetFile, replySubject }, strings != null && strings.Length > 0, strings);
                });
        }
    }
}
