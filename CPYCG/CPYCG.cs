using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using ICSharpCode.SharpZipLib.Zip;

namespace CPYCG
{
    public class CCG
    {
        public CCG(string path)
        {
            canZip = File.Exists(path + @"\ICSharpCode.SharpZipLib.dll");
            canRar = File.Exists(path + @"\UnRAR64.dll");
            canGif = File.Exists(path + @"\Gif.Components.dll");
            canWebp = File.Exists(path + @"\Imazen.WebP.dll") && File.Exists(path + @"\libwebp.dll");
            canApng = File.Exists(path + @"\LibAPNG.dll");
        }

        public CCG() { }

        /// <summary>
        /// 是否能读取rar文件的标志
        /// </summary>
        public bool canRar = false;
        /// <summary>
        /// 是否能读取zip文件的标志
        /// </summary>
        public bool canZip = false;
        /// <summary>
        /// 是否能生成gif文件的标志
        /// </summary>
        public bool canGif = false;
        /// <summary>
        /// 是否能读取Apng文件的标志
        /// </summary>
        public bool canApng = false;
        /// <summary>
        /// 是否能读取Webp文件的标志
        /// </summary>
        public bool canWebp = false;
        /// <summary>
        /// 当前是否是长图的标志
        /// </summary>
        public bool isLongPic = false;
        /// <summary>
        /// 翻转标志
        /// </summary>
        public bool isFlip = false;
        /// <summary>
        /// 旋转标志
        /// </summary>
        public bool isRotate = false;
        /// <summary>
        /// 存储ccg文件中的每张图的文件后缀名
        /// </summary>
        public List<string> fileExtensionList;
        /// <summary>
        /// 用于自动添加压缩包的密码
        /// </summary>
        public Dictionary<string, string> pwdList = null;
        /// <summary>
        /// 解压rar文件时所需的临时解压路径
        /// </summary>
        public string extractPath = "";
        /// <summary>
        /// 永远顺序播放，亦或是一正一反来回播放
        /// </summary>
        public bool isAlwaysAsc = true;
        /// <summary>
        /// 是否自带音频
        /// </summary>
        public bool hasSound = false;
        /// <summary>
        /// 每秒帧数
        /// </summary>
        public int fps = 3;
        /// <summary>
        /// 每秒帧数
        /// </summary>
        public bool isTooLong = true;
        /// <summary>
        /// 长图模式的实际图片数量
        /// </summary>
        //public int longPicCount = 2;
        /// <summary>
        /// 旋转角度
        /// </summary>
        public int angle = 0;
        /// <summary>
        /// 翻转X值，-1则水平翻转
        /// </summary>
        public int scale_X = 1;
        /// <summary>
        /// 翻转Y值，-1则垂直翻转
        /// </summary>
        public int scale_Y = 1;
        /// <summary>
        /// 存储压缩包中的非ccg文件的文件名
        /// </summary>
        public List<List<string>> FileLists
        {
            get { return fileLists; }
        }
        /// <summary>
        /// ccg文件的文件类型，或传入文件的类型
        /// </summary>
        public FileType Filetype
        {
            set { fileType = value; }
            get { return fileType; }
        }
        /// <summary>
        /// 图片和音频的播放方式
        /// </summary>
        public PlayType Playtype
        {
            set { playType = value; }
            get { return playType; }
        }

        /// <summary>
        /// 图片加载事件的Hander
        /// </summary>
        /// <param name="picData">图片数据</param>
        public delegate void LoadPicEventHander(byte[] picData, int index);

        /// <summary>
        /// 图片加载中的事件
        /// </summary>
        public event LoadPicEventHander PicLoading;

        /// <summary>
        /// 处理数据使用的流对象
        /// </summary>
        private Stream basedata;
        /// <summary>
        /// 压缩包的路径
        /// </summary>
        private string archivePath = "";
        /// <summary>
        /// 压缩包的名字
        /// </summary>
        private string archiveName = "";
        /// <summary>
        /// 在保存ccg文件时使用的开始/结束标志
        /// </summary>
        private bool isWriting = false;
        /// <summary>
        /// 存储压缩包中的非ccg文件的文件名
        /// </summary>
        private List<List<string>> fileLists = new List<List<string>>();
        /// <summary>
        /// ccg文件的文件类型，或传入文件的类型
        /// </summary>
        private FileType fileType = FileType.Error;
        /// <summary>
        /// 在保存ccg文件时使用的临时文件类型
        /// </summary>
        private FileType ft_Output = FileType.Error;
        /// <summary>
        /// 图片和音频的播放方式
        /// </summary>
        private PlayType playType = PlayType.Asc_None;
        /// <summary>
        /// 在保存ccg文件时使用的临时播放方式
        /// </summary>
        private int pt_Output = 5;
        
        /// <summary>
        /// ccg文件的文件类型，或传入文件的类型
        /// </summary>
        public enum FileType
        {
            /// <summary>
            /// 图片数据无压缩 + 有缩略图 + 固定时间间隔
            /// </summary>
            Normal = 0,
            /// <summary>
            /// 图片数据无压缩 + 有缩略图 + 时间间隔可变
            /// </summary>
            Variable = 1,
            /// <summary>
            /// 图片数据无压缩 + 无缩略图 + 固定时间间隔
            /// </summary>
            NoThumbnail = 2,
            /// <summary>
            /// 图片数据无压缩 + 无缩略图 + 时间间隔可变
            /// </summary>
            NoThumbnail_Variable = 3,
            /// <summary>
            /// 图片数据有压缩 + 有缩略图 + 固定时间间隔
            /// </summary>
            SmallSize = 4,
            /// <summary>
            /// 图片数据有压缩 + 有缩略图 + 时间间隔可变
            /// </summary>
            SmallSize_Variable = 5,
            /// <summary>
            /// 图片数据有压缩 + 无缩略图 + 固定时间间隔
            /// </summary>
            SmallSize_NoThumbnail = 6,
            /// <summary>
            /// 图片数据有压缩 + 无缩略图 + 时间间隔可变
            /// </summary>
            SmallSize_NoThumbnail_Variable = 7,
            /// <summary>
            /// 传入的是zip文件
            /// </summary>
            Zip = 8,
            /// <summary>
            /// 当前是Gif文件
            /// </summary>
            Gif = 9,
            /// <summary>
            /// 当前是Apng文件
            /// </summary>
            Apng = 10,
            /// <summary>
            /// 当前是Webp文件
            /// </summary>
            Webp = 11,
            /// <summary>
            /// 传入的是rar文件
            /// </summary>
            Rar = 16,
            /// <summary>
            /// 垂直拼接的长图
            /// </summary>
            LongPic = 32,
            /// <summary>
            /// 垂直拼接的长图，无缩略图
            /// </summary>
            LongPic_NoThumbnail = 33,
            /// <summary>
            /// 文件类型有误，或尚未初始化
            /// </summary>
            Error = 128
        };
        /// <summary>
        /// 图片和音频的播放方式
        /// </summary>
        public enum PlayType
        {
            /// <summary>
            /// 顺序播放，音频无限自动循环
            /// </summary>
            Asc_Auto = 1,
            /// <summary>
            /// 正反轮播，音频无限自动循环
            /// </summary>
            Desc_Auto = 2,
            /// <summary>
            /// 顺序播放，每回到初始图片则音频重置
            /// </summary>
            Asc_Reset = 3,
            /// <summary>
            /// 正反轮播，每回到初始图片则音频重置
            /// </summary>
            Desc_Reset = 4,
            /// <summary>
            /// 顺序播放，没有音乐
            /// </summary>
            Asc_None = 5,
            /// <summary>
            /// 正反轮播，没有音乐
            /// </summary>
            Desc_None = 6
        };

        //--------------------------------------------------Read---------------------------------------------------

        /// <summary>
        /// 读取并返回ccg文件的文件类型
        /// </summary>
        /// <param name="filePath">ccg文件路径</param>
        /// <returns>ccg文件的文件类型</returns>
        public FileType ReadFileType(string filePath)
        {
            if(fileType != FileType.Rar)
                fileLists.Clear();
            basedata = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] s = new byte[5];
            basedata.Read(s, 0, 5);
            fileType = (Encoding.UTF8.GetString(s) == "CPYCG") ? (FileType)basedata.ReadByte() : FileType.Error;
            return fileType;
        }

        /// <summary>
        /// 读取并返回ccg文件的文件类型
        /// </summary>
        /// <param name="data">ccg文件的byte数据</param>
        /// <returns>ccg文件的文件类型</returns>
        public FileType ReadFileType(byte[] data)
        {
            if(fileType != FileType.Zip)
                fileLists.Clear();
            basedata = new MemoryStream(data);
            byte[] s = new byte[5];
            basedata.Read(s, 0, 5);
            fileType = (Encoding.UTF8.GetString(s) == "CPYCG") ? (FileType)basedata.ReadByte() : FileType.Error;
            return fileType;
        }

        /// <summary>
        /// 从ccg文件中获取图片数据
        /// </summary>
        /// <param name="fileType">文件种类</param>
        /// <returns>(图片-时间间隔)的列表</returns>
        public List<PicData_PlayTime> ReadFromCcg()
        {
            if (fileType == FileType.Error || fileType == FileType.Zip || fileType == FileType.Rar)
                throw new Exception("FileType Error!");

            if (fileType != FileType.NoThumbnail 
                && fileType != FileType.NoThumbnail_Variable 
                && fileType != FileType.LongPic_NoThumbnail
                && fileType != FileType.SmallSize_NoThumbnail
                && fileType != FileType.SmallSize_NoThumbnail_Variable)
            {
                int thumbnailLength = ReadDataLength();
                basedata.Seek(thumbnailLength, SeekOrigin.Current);
            }

            fileExtensionList = new List<string>();
            List<PicData_PlayTime> pdpt = new List<PicData_PlayTime>();
            int picLength = 0;
            byte[] data;
            
            int _playType = basedata.ReadByte();
            picLength = _playType >> 3;
            if (picLength != 0)
            {
                if (picLength >> 4 == 1)
                {
                    scale_X = -1;
                    isFlip = true;
                }
                else if (picLength >> 3 == 1)
                {
                    scale_Y = -1;
                    isFlip = true;
                }
                if ((picLength & 0x01) == 1)
                {
                    angle = 90;
                    isRotate = true;
                }
                else if ((picLength & 0x02) == 2)
                {
                    angle = 180;
                    isRotate = true;
                }
                else if ((picLength & 0x04) == 4)
                {
                    angle = 270;
                    isRotate = true;
                }
                else
                {
                    angle = 0;
                }
                _playType = _playType & 0x07;
            }

            playType = (PlayType)_playType;
            if (_playType < 5)
            {
                hasSound = true;
                picLength = ReadDataLength();
                data = new byte[picLength];
                basedata.Read(data, 0, picLength);
                try
                {
                    File.WriteAllBytes(extractPath + ".mp3", data);
                }
                catch
                {
                    if (_playType % 2 == 1)
                        playType = PlayType.Asc_None;
                    else
                        playType = PlayType.Desc_None;
                }
            }
            else
                hasSound = false;
            
            if (fileType == FileType.LongPic || fileType == FileType.LongPic_NoThumbnail)
            {
                fps = basedata.ReadByte();
                isTooLong = ((fps >> 7) == 0);
                if (!isTooLong) fps -= 128;
                picLength = ReadDataLength();
                data = new byte[picLength];
                basedata.Read(data, 0, picLength);
                pdpt.Add(new PicData_PlayTime(data));
                string ex = ReadFileExtension();
                if (ex == "jpe")
                    ex = "jpg";
                fileExtensionList.Add(ex);
                pdpt[0].playTime = ((basedata.ReadByte() << 8) + basedata.ReadByte());
                Closebasedata();
                isLongPic = true;
                return pdpt;
            }

            //isLongPic = false;
            if ((int)fileType % 2 == 1)
            {
                fps = 3;
                while ((picLength = ReadDataLength()) != 0)
                {
                    data = new byte[picLength];
                    basedata.Read(data, 0, picLength);
                    pdpt.Add(new PicData_PlayTime(data, (basedata.ReadByte() * 10)));
                    fileExtensionList.Add(ReadFileExtension());
                    if (null != PicLoading)
                        PicLoading(data, pdpt.Count);
                }
            }
            else
            {
                fps = basedata.ReadByte();
                while ((picLength = ReadDataLength()) != 0)
                {
                    data = new byte[picLength];
                    basedata.Read(data, 0, picLength);
                    pdpt.Add(new PicData_PlayTime(data));
                    fileExtensionList.Add(ReadFileExtension());
                    if (null != PicLoading)
                        PicLoading(data, pdpt.Count);
                }
            }
            Closebasedata();
            return pdpt;
        }
        /*
        /// <summary>
        /// 尝试获取Apng数据
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>出错则返回null，不是Apng则长度为0</returns>
        public List<PicData_PlayTime> ReadFromPng(string filePath)
        {
            if (!canApng) return new List<PicData_PlayTime>();
            APNG apng = null;
            try
            {
                apng = new APNG(filePath);
            }
            catch
            {
                return null;
            }
            if (apng.Frames.Length == 0)
                return new List<PicData_PlayTime>();
            else
            {
                List<PicData_PlayTime> pdpt = new List<PicData_PlayTime>();
                PicData_PlayTime p = null;
                double time = 0.0;
                playType = PlayType.Asc_None;
                fileExtensionList = new List<string>();
                for (int i = 0; i < apng.Frames.Length; i++)
                {
                    p = new PicData_PlayTime(apng.Frames[i].GetStream().ToArray());
                    if (apng.Frames[i].fcTLChunk.DelayNum == 0)
                    {
                        time = 10;
                    }
                    else
                    {
                        if (apng.Frames[i].fcTLChunk.DelayDen == 0)
                        {
                            time = apng.Frames[i].fcTLChunk.DelayNum * 10;
                        }
                        else
                        {
                            time = apng.Frames[i].fcTLChunk.DelayNum * 1000 / apng.Frames[i].fcTLChunk.DelayDen;
                        }
                    }
                    p.playTime = (int)time;
                    pdpt.Add(p);
                    fileExtensionList.Add("png");
                }
                return pdpt;
            }
        }

        /// <summary>
        /// 从Webp文件中获取图片数据
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>转换为byte[]数组的图片数据</returns>
        public byte[] ReadFromWebp(string filePath)
        {
            try
            {
                SimpleDecoder sd = new SimpleDecoder();
                byte[] data = File.ReadAllBytes(filePath);
                Bitmap bi = sd.DecodeFromBytes(data, data.Length);
                MemoryStream ms = new MemoryStream();
                bi.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                data = ms.ToArray();
                ms.Close();
                bi.Dispose();
                return data;
            }
            catch
            {
                return null;
            }
        }
        */

        /// <summary>
        /// 从gif文件中获取图片数据
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="isNoVariable"></param>
        /// <returns></returns>
        public PicData_PlayTime[] ReadFromGif(string filePath, out bool isNoVariable)
        {
            isNoVariable = true;
            try
            {
                fileExtensionList = new List<string>();
                GifDecoder gd = new GifDecoder(PicLoading, fileExtensionList);
                return gd.GetGifData(filePath, out isNoVariable);
                //return pdpt;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 从zip文件中读取图片数据列表
        /// </summary>
        /// <param name="filePath">zip文件路径</param>
        /// <returns>zip文件中的文件列表</returns>
        public List<FileName_Index> ReadFromZip(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            fileExtensionList = new List<string>();
            Closebasedata();
            //isLongPic = false;
            ZipInputStream zipStream = null;
            ZipEntry ent = null;
            List<FileName_Index> result = new List<FileName_Index>();
            List<string> zipFileList = new List<string>();
            try
            {
                zipStream = new ZipInputStream(File.OpenRead(filePath));
                if (null != pwdList && pwdList.ContainsKey(filePath))
                    zipStream.Password = pwdList[filePath];

                while ((ent = zipStream.GetNextEntry()) != null)
                {
                    if (!ent.Name.EndsWith("/"))
                    {
                        if (ent.Name.EndsWith(".ccg"))
                        {
                            result.Add(new FileName_Index(ent.Name, 0));
                        }
                        else if (ent.Name.EndsWith(".jpg") || ent.Name.EndsWith(".png") || ent.Name.EndsWith(".bmp") || ent.Name.EndsWith(".jpeg"))
                        {
                            zipFileList.Add(ent.Name);
                        }
                    }
                }
                archivePath = filePath;
                if (zipFileList.Count == 0) return result;

                string group = "";
                string temp = "";
                OrdinalComparer comp = new OrdinalComparer();
                fileLists = new List<List<string>>();
                zipFileList.OrderBy(f => SortZipList(f));
                for (int i = 0; i < zipFileList.Count; i++)
                {
                    temp = zipFileList[i];
                    group = SortZipList(temp);
                    //fileLists.Add(zipFileList.Where(f => SortZipList(f) == group).OrderBy(f => SortFileNameLength(f)).ThenBy(f => f, comp).ToList());
                    fileLists.Add(zipFileList.Where(f => SortZipList(f) == group).OrderBy(f => f, comp).ToList());
                    i += fileLists.Last().Count - 1;
                    result.Add(new FileName_Index(temp, fileLists.Count - 1));
                }
            }
            catch (Exception ex)
            {
                archivePath = "";
                throw ex;
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Close();
                    zipStream.Dispose();
                }
                if (ent != null)
                    ent = null;
                GC.Collect();
                GC.Collect(1);
            }
            return result;
        }

        /// <summary>
        /// 从zip文件中读取全部数据列表
        /// </summary>
        /// <param name="filePath">zip文件路径</param>
        /// <returns>zip文件中的文件列表</returns>
        public List<FileName_Index> ReadAllFromZip(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            fileExtensionList = new List<string>();
            Closebasedata();
            //isLongPic = false;
            ZipInputStream zipStream = null;
            ZipEntry ent = null;
            List<FileName_Index> result = new List<FileName_Index>();
            List<string> zipFileList = new List<string>();
            try
            {
                zipStream = new ZipInputStream(File.OpenRead(filePath));
                if (null != pwdList && pwdList.ContainsKey(filePath))
                    zipStream.Password = pwdList[filePath];

                while ((ent = zipStream.GetNextEntry()) != null)
                {
                    if (!ent.Name.EndsWith("/"))
                    {
                        if (ent.Name.EndsWith(".ccg"))
                        {
                            result.Add(new FileName_Index(ent.Name, 0));
                        }
                        else
                        {
                            zipFileList.Add(ent.Name);
                        }
                    }
                }
                archivePath = filePath;
                if (zipFileList.Count == 0) return result;

                string group = "";
                string temp = "";
                fileLists = new List<List<string>>();
                OrdinalComparer comp = new OrdinalComparer();
                zipFileList.OrderBy(f => SortZipList(f));
                for (int i = 0; i < zipFileList.Count; i++)
                {
                    temp = zipFileList[i];
                    group = SortZipList(temp);
                    //fileLists.Add(zipFileList.Where(f => SortZipList(f) == group).OrderBy(f => SortFileNameLength(f)).ThenBy(f => f, comp).ToList());
                    fileLists.Add(zipFileList.Where(f => SortZipList(f) == group).OrderBy(f => f, comp).ToList());
                    i += fileLists.Last().Count - 1;
                    result.Add(new FileName_Index(temp, fileLists.Count - 1));
                }
            }
            catch (Exception ex)
            {
                archivePath = "";
                throw ex;
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Close();
                    zipStream.Dispose();
                }
                if (ent != null)
                    ent = null;
                GC.Collect();
                GC.Collect(1);
            }
            return result;
        }

        /// <summary>
        /// 从zip文件中获取相应数据
        /// </summary>
        /// <param name="fni">目标对象</param>
        /// <param name="time">播放时间间隔</param>
        /// <param name="isNoVariable">时间间隔是否固定: true则固定</param>
        /// <returns>(图片-时间间隔)的列表</returns>
        public PicData_PlayTime[] ExtractZip(FileName_Index fni, out bool isNoVariable)
        {
            ZipInputStream zipStream = null;
            ZipEntry ent = null;
            //isLongPic = false;
            byte[] data = null;
            try
            {
                zipStream = new ZipInputStream(File.OpenRead(archivePath));
                if (null != pwdList && pwdList.ContainsKey(archivePath))
                    zipStream.Password = pwdList[archivePath];
                if (fni.fileName.EndsWith(".ccg"))
                {
                    List<PicData_PlayTime> pdpt = new List<PicData_PlayTime>();
                    while ((ent = zipStream.GetNextEntry()) != null)
                    {
                        if (fni.fileName == ent.Name)
                        {
                            data = new byte[zipStream.Length];
                            zipStream.Read(data, 0, data.Length);
                            break;
                        }
                    }
                    fileType = ReadFileType(data);
                    pdpt = ReadFromCcg();
                    isNoVariable = (fileType == FileType.Normal || fileType == FileType.NoThumbnail || fileType == FileType.SmallSize || fileType == FileType.SmallSize_NoThumbnail);
                    fileType = FileType.Zip;
                    return pdpt.ToArray();
                }
                else
                {
                    int count = fileLists[fni.index].Count;
                    PicData_PlayTime[] pdpt = new PicData_PlayTime[count];
                    string[] extenList = new string[count];
                    int extenLength = 0, index = 0;
                    while ((ent = zipStream.GetNextEntry()) != null)
                    {
                        List<string> dd = fileLists[fni.index];
                        index = dd.IndexOf(ent.Name);
                        if (index != -1)
                        {
                            data = new byte[zipStream.Length];
                            zipStream.Read(data, 0, data.Length);
                            
                            pdpt[index] = new PicData_PlayTime(data);
                            extenLength = ent.Name.LastIndexOf('.');
                            extenList[index] = ent.Name.Substring(extenLength + 1, ent.Name.Length - extenLength - 1);
                            if (--count == 0) break;
                        }
                    }
                    fileExtensionList = extenList.ToList();
                    fps = 3;
                    playType = PlayType.Asc_None;
                    isNoVariable = true;
                    return pdpt;
                }
            }
            catch (Exception ex)
            {
                fileType = FileType.Zip;
                throw ex;
            }
            finally
            {
                if (zipStream != null)
                {
                    zipStream.Close();
                    zipStream.Dispose();
                }
                if (ent != null)
                    ent = null;
                GC.Collect();
                GC.Collect(1);
            }
        }

        /// <summary>
        /// 从rar文件中读取数据列表
        /// </summary>
        /// <param name="filePath">rar文件路径</param>
        /// <returns>rar文件中的文件列表</returns>
        public List<FileName_Index> ReadFromRar(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            List<string> rarFileList = new List<string>();
            //isLongPic = false;
            List<FileName_Index> result = new List<FileName_Index>();
            Unrar unrar = new Unrar();
            try
            {
                if (null != pwdList && pwdList.ContainsKey(filePath))
                    unrar.Password = pwdList[filePath];
                unrar.Open(filePath, Unrar.OpenMode.List);
                while (unrar.ReadHeader())
                {
                    if (!unrar.CurrentFile.IsDirectory)
                    {
                        if (unrar.CurrentFile.FileName.EndsWith(".ccg"))
                        {
                            result.Add(new FileName_Index(unrar.CurrentFile.FileName, 0));
                        }
                        else if (unrar.CurrentFile.FileName.EndsWith(".jpg") || unrar.CurrentFile.FileName.EndsWith(".bmp") || unrar.CurrentFile.FileName.EndsWith(".png") || unrar.CurrentFile.FileName.EndsWith(".jpeg"))
                        {
                            rarFileList.Add(unrar.CurrentFile.FileName);
                        }
                    }
                    unrar.Skip();
                }
                unrar.Close();
            }
            catch (Exception ex)
            {
                archivePath = "";
                unrar.Close();
                throw new Exception(ex.Message);
            }

            archivePath = filePath;
            if (rarFileList.Count == 0) return result;

            string group = "";
            string temp = "";
            fileLists = new List<List<string>>();
            OrdinalComparer comp = new OrdinalComparer();
            rarFileList.OrderBy(f => SortRarList(f));
            for (int i = 0; i < rarFileList.Count; i++)
            {
                temp = rarFileList[i];
                group = SortRarList(temp);
                //fileLists.Add(rarFileList.Where(f => SortRarList(f) == group).OrderBy(f => SortFileNameLength(f)).ThenBy(f => f, comp).ToList());
                fileLists.Add(rarFileList.Where(f => SortRarList(f) == group).OrderBy(f => f, comp).ToList());
                i += fileLists.Last().Count - 1;
                result.Add(new FileName_Index(temp, fileLists.Count - 1));
            }
            return result;
        }

        /// <summary>
        /// 从rar文件中读取全部数据列表
        /// </summary>
        /// <param name="filePath">rar文件路径</param>
        /// <returns>rar文件中的文件列表</returns>
        public List<FileName_Index> ReadAllFromRar(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException();

            List<string> rarFileList = new List<string>();
            //isLongPic = false;
            List<FileName_Index> result = new List<FileName_Index>();
            Unrar unrar = new Unrar();
            archiveName = Path.GetFileName(filePath);
            try
            {
                if (null != pwdList && pwdList.ContainsKey(filePath))
                    unrar.Password = pwdList[filePath];
                unrar.Open(filePath, Unrar.OpenMode.List);
                while (unrar.ReadHeader())
                {
                    if (!unrar.CurrentFile.IsDirectory)
                    {
                        if (unrar.CurrentFile.FileName.EndsWith(".ccg"))
                        {
                            result.Add(new FileName_Index(unrar.CurrentFile.FileName, 0));
                        }
                        else
                        {
                            rarFileList.Add(unrar.CurrentFile.FileName);
                        }
                    }
                    unrar.Skip();
                }
                unrar.Close();
            }
            catch (Exception ex)
            {
                archivePath = "";
                throw new Exception(ex.Message);
            }

            archivePath = filePath;
            if (rarFileList.Count == 0) return result;

            string group = "";
            string temp = "";
            fileLists = new List<List<string>>();
            OrdinalComparer comp = new OrdinalComparer();
            rarFileList.OrderBy(f => SortRarList(f));
            for (int i = 0; i < rarFileList.Count; i++)
            {
                temp = rarFileList[i];
                group = SortRarList(temp);
                //fileLists.Add(rarFileList.Where(f => SortRarList(f) == group).OrderBy(f => SortFileNameLength(f)).ThenBy(f => f, comp).ToList());
                fileLists.Add(rarFileList.Where(f => SortRarList(f) == group).OrderBy(f => f, comp).ToList());
                i += fileLists.Last().Count - 1;
                result.Add(new FileName_Index(temp, fileLists.Count - 1));
            }
            return result;
        }

        /// <summary>
        /// 从RAR文件中获取数据
        /// </summary>
        /// <param name="fni">目标对象</param>
        /// <param name="time">播放时间间隔</param>
        /// <param name="isNoVariable">时间间隔是否固定: true则固定</param>
        /// <returns>(图片-时间间隔)的列表</returns>
        public List<PicData_PlayTime> ExtractRar(FileName_Index fni, out bool isNoVariable)
        {
            if (archivePath.Length == 0)
                throw new Exception("ArchivePath No Found!");
            if (!Directory.Exists(extractPath))
                throw new Exception("Unknown Path To Extract!");

            List<PicData_PlayTime> pdpt = new List<PicData_PlayTime>();
            //isLongPic = false;
            Unrar unrar = new Unrar();
            try
            {
                if (null != pwdList && pwdList.ContainsKey(archivePath))
                    unrar.Password = pwdList[archivePath];
                unrar.Open(archivePath, Unrar.OpenMode.Extract);
                unrar.DestinationPath = extractPath;
                if (fni.fileName.EndsWith(".ccg"))
                {
                    while (unrar.ReadHeader())
                    {
                        if (unrar.CurrentFile.FileName == fni.fileName)
                        {
                            unrar.Extract();
                            break;
                        }
                        unrar.Skip();
                    }
                    fileType = ReadFileType(Path.Combine(extractPath, fni.fileName));
                    pdpt = ReadFromCcg();
                    isNoVariable = (fileType == FileType.Normal || fileType == FileType.NoThumbnail || fileType == FileType.SmallSize || fileType == FileType.SmallSize_NoThumbnail);
                    fileType = FileType.Rar;
                }
                else
                {
                    DirectoryInfo di = new DirectoryInfo(extractPath);
                    di.Delete(true);
                    di.Create();
                    int count = fileLists[fni.index].Count;
                    while (unrar.ReadHeader())
                    {
                        if (fileLists[fni.index].Contains(unrar.CurrentFile.FileName))
                        {
                            unrar.Extract();
                            if (--count == 0) break;
                        }
                        else
                        {
                            unrar.Skip();
                        }
                    }
                    byte[] temp;
                    count = fileLists[fni.index].Count;
                    fileExtensionList = new List<string>();
                    string tempPath = "";
                    int extenLength = 0;
                    for (int i = 0;i < count; i++)
                    {
                        tempPath = Path.Combine(extractPath, FileLists[fni.index][i]);
                        using (FileStream fs = new FileStream(tempPath, FileMode.Open))
                        {
                            temp = new byte[fs.Length];
                            fs.Read(temp, 0, temp.Length);
                            pdpt.Add(new PicData_PlayTime(temp));
                            extenLength = tempPath.LastIndexOf('.');
                            fileExtensionList.Add(tempPath.Substring(extenLength + 1, tempPath.Length - extenLength - 1));
                        }
                    }
                    fps = 3;
                    playType = PlayType.Asc_None;
                    isNoVariable = true;
                }
                unrar.Close();
            }
            catch (Exception ex)
            {
                unrar.Close();
                fileType = FileType.Rar;
                throw ex;
            }
            return pdpt;
        }

        /// <summary>
        /// 对zip文件列表排序的条件
        /// </summary>
        /// <param name="f">当前元素</param>
        /// <returns>用于排列条件的部分</returns>
        private string SortZipList(string f)
        {
            int i = f.LastIndexOf('/');
            return (i == -1) ? "" : f.Substring(0, i);
        }

        /// <summary>
        /// 对rar文件列表排序的条件
        /// </summary>
        /// <param name="f">当前元素</param>
        /// <returns>用于排列条件的部分</returns>
        private string SortRarList(string f)
        {
            int i = f.LastIndexOf('\\');
            return (i == -1) ? "" : f.Substring(0, i);
        }

        /// <summary>
        /// 取文件名的长度来做最终排序
        /// </summary>
        /// <param name="f">当前元素</param>
        /// <returns>用于排列条件的部分</returns>
        public int SortFileNameLength(string f)
        {
            int i = f.LastIndexOf('.');
            return (i == -1) ? 0 : i;
        }

        /// <summary>
        /// 从流中读取图片后缀名
        /// </summary>
        /// <returns>图片的后缀名</returns>
        private string ReadFileExtension()
        {
            byte[] extension = new byte[3];
            basedata.Read(extension, 0, 3);
            return Encoding.UTF8.GetString(extension);
        }

        /// <summary>
        /// 从流中读取数据的长度
        /// </summary>
        /// <returns>数据的长度</returns>
        private int ReadDataLength()
        {
            byte[] source = new byte[4];
            if (basedata.Read(source, 0, 4) == 0)
                return 0;
            return (source[0] << 24) + (source[1] << 16) + (source[2] << 8) + source[3];
        }

        //--------------------------------------------------Write--------------------------------------------------

        /// <summary>
        /// 开始创建CCG文件
        /// </summary>
        /// <param name="ft">CCG文件类型</param>
        /// <param name="pt">CCG播放方式</param>
        public void BeginWrite(FileType ft, int pt)
        {
            if (ft == FileType.Error || ft == FileType.Rar || ft == FileType.Zip)
                throw new Exception("Error FileType!");
            ft_Output = ft;
            int _pt = 0;
            if (isFlip)
            {
                if (scale_X == -1) _pt += 128;
                if (scale_Y == -1) _pt += 64;
                if(_pt == 192)
                {
                    _pt = 0;
                    scale_X = scale_Y = 1;
                    isFlip = false;
                    angle += 180;
                    if (angle >= 360)
                    {
                        angle -= 360;
                        isRotate = (angle != 0);
                    }
                }
            }
            if (isRotate)
            {
                pt_Output = angle / 90;
                if (pt_Output == 3) pt_Output = 4;
                _pt += pt_Output << 3;
            }
            pt_Output = pt + _pt;
            isWriting = true;
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="fileName">文件名(包括完整路径)</param>
        /// <param name="fps">每秒帧数</param>
        /// <param name="data">(图片)数据</param>
        /// <param name="soundPath">音频文件路径</param>
        /// <param name="thumbnailImage">(缩略图)数据</param>
        public void WriteCcgFile(string fileName, int fps, PicData_PlayTime[] data, string soundPath = null, byte[] thumbnailImage = null)
        {
            if (!isWriting)
                throw new Exception("You Should BeginWrite First!");
            if (fps > 255 || fps < 0)
                throw new Exception("The fps Can Not Bigger Then 255, And Can Not Be A Minus!");
            if (null == data || data.Length == 0)
                throw new Exception("Pictrues Data Not Found!");
            if (fileExtensionList.Count != data.Length)
                throw new Exception("fileExtensionList Has Not Update!");

            try
            {
                basedata = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                WriteFileType();
                if (ft_Output == FileType.Normal
                    || ft_Output == FileType.LongPic
                    || ft_Output == FileType.Variable
                    || ft_Output == FileType.SmallSize
                    || ft_Output == FileType.SmallSize_Variable)
                {
                    if (null != thumbnailImage && thumbnailImage.Length > 0)
                        WriteThumbnailImage(thumbnailImage);
                    else
                    {
                        if (null == data[0].picData || data[0].picData.Length == 0)
                            throw new Exception("Can Not Create Thumbnail, Because The Data Is Empty!");
                        WriteThumbnailImage(data[0].picData);
                    }
                }
                
                basedata.WriteByte((byte)pt_Output);
                if((pt_Output & 0x07) < 5)
                {
                    if (soundPath == null || !File.Exists(soundPath))
                        throw new Exception("Sound File Not Found!");
                    WriteAudioData(soundPath);
                }

                if (ft_Output == FileType.LongPic || ft_Output == FileType.LongPic_NoThumbnail)
                {
                    basedata.WriteByte((byte)fps);
                    int length = data[0].picData.Length;
                    basedata.Write(ToBytes(length), 0, 4);
                    basedata.Write(data[0].picData, 0, length);
                    basedata.Write(Encoding.UTF8.GetBytes(fileExtensionList[0]), 0, 3);
                    byte[] le = new byte[2];
                    le[0] = (byte)(data[0].playTime >> 8);
                    le[1] = (byte)data[0].playTime;
                    basedata.Write(le, 0, 2);
                    basedata.Flush();
                    return;
                }

                if ((int)ft_Output % 2 == 0)
                {
                    basedata.WriteByte((byte)fps);
                    WritePicData(data);
                }
                else
                {
                    WriteVariablePicData(data);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                isWriting = false;
                Closebasedata();
            }
        }

        /// <summary>
        /// 写入文件头信息及文件类型信息
        /// </summary>
        private void WriteFileType()
        {
            basedata.Write(Encoding.UTF8.GetBytes("CPYCG"), 0, 5);
            basedata.WriteByte((byte)((int)ft_Output));
            basedata.Flush();
        }

        /// <summary>
        /// 写入音频文件的数据
        /// </summary>
        /// <param name="audioPath">音频文件路径</param>
        private void WriteAudioData(string audioPath)
        {
            byte[] temp = File.ReadAllBytes(audioPath);
            basedata.Write(ToBytes(temp.Length), 0, 4);
            basedata.Write(temp, 0, temp.Length);
            basedata.Flush();
        }

        /// <summary>
        /// 写入缩略图信息
        /// </summary>
        /// <param name="firstImage">作为缩略图的图片</param>
        private void WriteThumbnailImage(byte[] firstImage)
        {
            ImageConverter ic = new ImageConverter();
            Image initimage = new Bitmap(new MemoryStream(firstImage));
            Image newimage = null;
            if (initimage.Width >= initimage.Height && initimage.Width > 300)
            {
                newimage = new Bitmap(300, (int)(initimage.Height * 300 / initimage.Width));
            }
            else if (initimage.Height >= initimage.Width && initimage.Height > 300)
            {
                newimage = new Bitmap((int)(initimage.Width * 300 / initimage.Height), 300);
            }
            else
            {
                if(null != newimage) newimage.Dispose();
                initimage.Dispose();
                basedata.Write(ToBytes(firstImage.Length), 0, 4);
                basedata.Write(firstImage, 0, firstImage.Length);
                basedata.Flush();
                return;
            }
            //新建一个画板
            Graphics newg = Graphics.FromImage(newimage);
            newg.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            newg.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            newg.Clear(Color.Empty);
            newg.DrawImage(initimage, new Rectangle(0, 0, newimage.Width, newimage.Height), new Rectangle(0, 0, initimage.Width, initimage.Height), GraphicsUnit.Pixel);
            byte[] _temp = (byte[])ic.ConvertTo(newimage, typeof(byte[]));
            newg.Dispose();
            newimage.Dispose();
            initimage.Dispose();
            basedata.Write(ToBytes(_temp.Length), 0, 4);
            basedata.Write(_temp, 0, _temp.Length);
            basedata.Flush();
        }

        /// <summary>
        /// 写入图片数据(固定时间间隔)
        /// </summary>
        /// <param name="data">图片数据数组</param>
        private void WritePicData(PicData_PlayTime[] data)
        {
            int length = 0;
            for (int i = 0; i < data.Length; i++)
            {
                length = data[i].picData.Length;
                basedata.Write(ToBytes(length), 0, 4);
                basedata.Write(data[i].picData, 0, length);
                basedata.Write(Encoding.UTF8.GetBytes(fileExtensionList[i]), 0, 3);
            }
            basedata.Flush();
        }

        /// <summary>
        /// 写入图片数据(时间间隔可变)
        /// </summary>
        /// <param name="data">图片数据数组</param>
        private void WriteVariablePicData(PicData_PlayTime[] data)
        {
            int length = 0;
            for (int i = 0; i < data.Length; i++)
            {
                length = data[i].picData.Length;
                basedata.Write(ToBytes(length), 0, 4);
                basedata.Write(data[i].picData, 0, length);
                basedata.WriteByte((byte)data[i].playTime);
                basedata.Write(Encoding.UTF8.GetBytes(fileExtensionList[i]), 0, 3);
            }
            basedata.Flush();
        }

        /// <summary>
        /// 将数组长度转换成固定长度的byte数据
        /// </summary>
        /// <param name="source">数组长度</param>
        /// <returns>对应的byte数据</returns>
        private byte[] ToBytes(int source)
        {
            byte[] target = new byte[4];
            target[0] = (byte)(source >> 24);
            target[1] = (byte)(source >> 16);
            target[2] = (byte)(source >> 8);
            target[3] = (byte)source;
            return target;
        }

        //---------------------------------------------Close_Clear--------------------------------------------------

        /// <summary>
        /// 关闭流
        /// </summary>
        private void Closebasedata()
        {
            if (null != basedata)
                basedata.Close();
        }
    }

    public class OrdinalComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return string.CompareOrdinal(x.ToLowerInvariant(), y.ToLowerInvariant());
        }
    }
}
