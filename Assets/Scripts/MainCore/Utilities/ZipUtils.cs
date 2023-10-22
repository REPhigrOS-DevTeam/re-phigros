using System;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace MainCore.Utilities
{
    public static class ZipUtils
    {
        /// <summary>
        /// ZIP:解压一个zip文件
        /// add yuangang by 2016-06-13
        /// </summary>
        /// <param name="ZipFile">需要解压的Zip文件（绝对路径）</param>
        /// <param name="TargetDirectory">解压到的目录</param>
        /// <param name="OverWrite">是否覆盖已存在的文件</param>
        public static void UnZip(string ZipFile, string TargetDirectory, bool OverWrite = true)
        {
            Unzip(File.OpenRead(ZipFile), TargetDirectory, OverWrite);
        }

        /// <summary>
        /// ZIP:解压一个zip文件
        /// add yuangang by 2016-06-13
        /// </summary>
        /// <param name="data">需要解压的Zip文件</param>
        /// <param name="TargetDirectory">解压到的目录</param>
        /// <param name="OverWrite">是否覆盖已存在的文件</param>
        public static void Unzip(byte[] data, string TargetDirectory, bool OverWrite = true)
        {
            Unzip(new MemoryStream(data), TargetDirectory, OverWrite);
        }

        private static void Unzip(Stream stream, string TargetDirectory, bool OverWrite)
        {
            TargetDirectory = TargetDirectory.Replace("\\", "/");
            //如果解压到的目录不存在，则报错
            if (!Directory.Exists(TargetDirectory))
            {
                Directory.CreateDirectory(TargetDirectory);
            }

            //目录结尾
            if (!TargetDirectory.EndsWith("/"))
            {
                TargetDirectory = String.Concat(TargetDirectory, "/");
            }

            using ZipInputStream zipfiles = new ZipInputStream(stream);
            while (zipfiles.GetNextEntry() is { } theEntry)
            {
                theEntry.IsUnicodeText = true;
                string directoryName = "";
                string pathToZip = "";
                pathToZip = theEntry.Name;

                if (pathToZip != "")
                    directoryName = Path.GetDirectoryName(pathToZip) + "/";

                string fileName = Path.GetFileName(pathToZip);

                Directory.CreateDirectory(TargetDirectory + directoryName);

                if (fileName == "") continue;
                if ((!File.Exists(TargetDirectory + directoryName + fileName) || !OverWrite) &&
                    (File.Exists(TargetDirectory + directoryName + fileName))) continue;
                using FileStream streamWriter = File.Create(TargetDirectory + directoryName + fileName);
                int size;
                byte[] data = new byte[2048];
                while (true)
                {
                    size = zipfiles.Read(data, 0, data.Length);

                    if (size > 0)
                        streamWriter.Write(data, 0, size);
                    else
                        break;
                }
            }

            zipfiles.Close();
        }

        /// <summary>
        /// ZIP：压缩文件夹
        /// add yuangang by 2016-06-13
        /// </summary>
        /// <param name="DirectoryToZip">需要压缩的文件夹（绝对路径）</param>
        /// <param name="ZipedPath">压缩后的文件路径（绝对路径）</param>
        public static void ZipDirectory(string DirectoryToZip, string ZipedPath, bool overwrite = true)
        {
            DirectoryToZip = DirectoryToZip.Replace("\\", "/");
            //如果目录不存在，则报错
            if (!Directory.Exists(DirectoryToZip))
            {
                throw new FileNotFoundException("指定的目录: " + DirectoryToZip + " 不存在!");
            }

            //文件名称（默认同源文件名称相同）

            if (File.Exists(ZipedPath))
            {
                if (overwrite) File.Delete(ZipedPath);
                else return;
            }

            string temporaryCachePath = Application.temporaryCachePath + "/zip." + Util.GetMD5(Encoding.UTF8.GetBytes(ZipedPath)) + ".tmp";
            if (File.Exists(temporaryCachePath)) File.Delete(temporaryCachePath);
            using (FileStream ZipFile = File.Create(temporaryCachePath))
            {
                using ZipOutputStream s = new ZipOutputStream(ZipFile);
                s.ZipCryptoEncoding = StringCodec.UnicodeZipEncoding;
                s.SetLevel(6);
                Crc32 crc = new Crc32();
                DirectoryToZip = DirectoryToZip.Replace("\\", "/");
                ZipSetp(crc, DirectoryToZip, s, "");
            }

            File.Move(temporaryCachePath, ZipedPath);
        }

        /// <summary>
        /// 递归遍历目录
        /// add yuangang by 2016-06-13
        /// </summary>
        private static void ZipSetp(Crc32 crc, string strDirectory, ZipOutputStream s, string parentPath)
        {
            if (!strDirectory.EndsWith("/"))
            {
                strDirectory = string.Concat(strDirectory, "/");
            }

            string[] filenames = Directory.GetFileSystemEntries(strDirectory);

            foreach (var str in filenames)
            {
                var file = str.Replace("\\", "/");
                if (Directory.Exists(file)) // 先当作目录处理如果存在这个目录就递归Copy该目录下面的文件
                {
                    ZipSetp(crc, file, s,
                        parentPath + file.Substring(file.LastIndexOf("/", StringComparison.Ordinal) + 1) + "/");
                }
                else // 否则直接压缩文件
                {
                    //打开压缩文件
                    using FileStream fs = File.OpenRead(file);
                    byte[] buffer = new byte[fs.Length];
                    if (fs.Read(buffer, 0, buffer.Length) != buffer.Length) throw new ArgumentException();

                    string fileName = parentPath + file.Substring(file.LastIndexOf("/", StringComparison.Ordinal) + 1);
                    ZipEntry entry = new ZipEntry(fileName)
                    {
                        DateTime = DateTime.Now,
                        Size = fs.Length
                    };

                    fs.Close();

                    crc.Reset();
                    crc.Update(buffer);

                    entry.Crc = crc.Value;
                    s.PutNextEntry(entry);

                    s.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}