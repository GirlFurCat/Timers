using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace TimerControl
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 通信主Socket
        /// </summary>
        Socket clinet;

        /// <summary>
        /// 下载Socket
        /// </summary>
        Socket DownloadClinet;

        /// <summary>
        /// 绑定地址
        /// </summary>
        string IpAddress = ConfigurationManager.AppSettings["Address"];

        /// <summary>
        /// 绑定主端口
        /// </summary>
        int Port = int.Parse(ConfigurationManager.AppSettings["Port"]);

        /// <summary>
        /// 绑定下载端口
        /// </summary>
        int DownloadPort = int.Parse(ConfigurationManager.AppSettings["DownloadPort"]);

        /// <summary>
        /// 添加委托以添加控件
        /// </summary>
        private delegate void UpdateFormControl();

        /// <summary>
        /// 现有连接列表
        /// </summary>
        private Dictionary<EndPoint, Socket> ConnectionList = new Dictionary<EndPoint, Socket>();

        /// <summary>
        /// 现有可下载文件的连接列表
        /// </summary>
        private Dictionary<EndPoint, Socket> DownloadConnList = new Dictionary<EndPoint, Socket>();

        /// <summary>
        /// 返回的信息列表
        /// </summary>
        private Dictionary<int, ReturnState> ReturnMessageList = new Dictionary<int, ReturnState>();

        /// <summary>
        /// 线程指示灯
        /// </summary>
        private Dictionary<int, AutoResetEvent> AutoResetEventList = new Dictionary<int, AutoResetEvent>();

        /// <summary>
        /// 新接入中心的连接
        /// </summary>
        private Socket NewSocket;

        /// <summary>
        /// 新接入中心可进行下载的连接
        /// </summary>
        private Socket NewDownloadSocket;

        /// <summary>
        /// 需要打开的路径
        /// </summary>
        private string OpenPath = string.Empty;

        /// <summary>
        /// 父路径
        /// </summary>
        private string ParentPath
        {
            get
            {
                int PathLevelNum = OpenPath.Split('\\').Where(x => !string.IsNullOrEmpty(x)).Count();
                string ReturnValue = string.Empty;
                List<string> PathList = OpenPath.Split('\\').Where(x => !string.IsNullOrEmpty(x)).ToList();
                for(int i = 0; i < PathList.Count - 1; i++)
                {
                    ReturnValue += PathList[i] + "\\";
                }
                return ReturnValue;
            }
        }

        /// <summary>
        /// 用于发送消息
        /// </summary>
        private TransmitMessage transmitMessage = new TransmitMessage();

        /// <summary>
        /// 当前窗体
        /// </summary>
        Form1 _this;

        string LogTextPath = "Timer_" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");

        public Form1()
        {
            InitializeComponent();
        }

        private void FilePath_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {

            }
        }

        /// <summary>
        /// 窗体加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                _this = this;

                //初始化Socket连接
                clinet = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                DownloadClinet = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //获取ip地址和端口
                IPEndPoint point = new IPEndPoint(IPAddress.Parse(IpAddress), Port);

                //绑定ip和端口
                clinet.Bind(point);
                point.Port = DownloadPort;
                DownloadClinet.Bind(point);

                //设置最大连接数
                clinet.Listen(50);
                DownloadClinet.Listen(50);

                //开启新线程监听
                Thread serverThread = new Thread(DelegeRefresh);
                serverThread.IsBackground = true;
                serverThread.Start();
            }
            catch (Exception ex)
            {
                //发生错误时记录Log并弹出错误
                Log(ex.StackTrace, ex.Message, "Form1_Load", true);
                MessageBoxShow(ex.Message);
            }
        }

        /// <summary>
        /// 委托当新连接设备接入时添加至列表
        /// </summary>
        private void DelegeRefresh()
        {
            try
            {
                while (true)
                {
                    //将AddConnection方法添加到委托中
                    UpdateFormControl updateFormControl = new UpdateFormControl(AddConnection);

                    //监听是否有新连接
                    NewSocket = clinet.Accept();
                    NewDownloadSocket = DownloadClinet.Accept();

                    //将新连接添加到页面显示和现有连接列表对象
                    _this.Invoke(updateFormControl);
                }
            }
            catch (Exception ex)
            {
                //发生错误时记录Log
                Log(ex.StackTrace, ex.Message, "DelegeRefresh", true);
            }
        }

        /// <summary>
        /// 将新连接添加到列表中
        /// </summary>
        private void AddConnection()
        {
            try
            {
                //将连接添加到页面显示
                ConnList.Invoke(new Action(() =>
                {
                    ConnList.Items.Add(NewSocket.RemoteEndPoint);
                }));

                //将连接添加到现有连接列表
                ConnectionList.Add(NewSocket.RemoteEndPoint, NewSocket);

                //判断是否已经将主连接添加到现有连接列表，未添加则无法添加可下载文件连接
                if (ConnectionList.Keys.Where(x => x == new IPEndPoint(IPAddress.Parse(NewDownloadSocket.RemoteEndPoint.ToString().Split('.')[0]), Port)).Count() > 0)
                {
                    DownloadConnList.Add(NewDownloadSocket.RemoteEndPoint, NewDownloadSocket);
                }
            }
            catch (Exception ex)
            {
                //发生错误时记录Log
                Log(ex.StackTrace, ex.Message, "AddConnection", true);
            }
        }

        /// <summary>
        /// 刷新页面
        /// </summary>
        private void RefreshPage()
        {
            try
            {
                //判断是否选择了远程主机
                if (ConnList.SelectedIndex != -1)
                {
                    //将已连接列表中的Socket赋值给当前连接对象
                    clinet = ConnectionList[(IPEndPoint)ConnList.SelectedItem];
                    OpenPath = string.Empty;

                    //初始化待发送的信息
                    transmitMessage = new TransmitMessage();

                    //向客户端发送信息
                    Clinet_Send(1);

                    //判断是否完成任务
                    if (ReturnMessageList[transmitMessage.StateCode].State)
                    {
                        //反序列化磁盘信息
                        List<HardDiskPartition> hardDisk = JsonConvert.DeserializeObject<List<HardDiskPartition>>(ReturnMessageList[transmitMessage.StateCode].Message);

                        //将获取到的磁盘信息写入页面列表
                        FileListView_hardDisk(hardDisk);

                        //清理已使用的数据
                        ReturnMessageList.Remove(transmitMessage.StateCode);

                        //清空列表
                        hardDisk.Clear();
                    }
                    else
                    {
                        MessageBoxShow(string.Format("客户端发生错误，错误信息：{0}", ReturnMessageList[transmitMessage.StateCode].Message));
                    }
                }
            }
            catch (Exception ex)
            {
                //发生错误时记录Log并弹出错误
                Log(ex.StackTrace, ex.Message, "AddConnection", true);
                MessageBoxShow(ex.Message);
            }
        }

        /// <summary>
        /// 接收信息
        /// </summary>
        /// <param name="value"></param>
        private void ReceiveMessages(object value)
        {
            try
            {
                //定义临时缓冲区
                byte[] recBuf = new byte[10000000];

                //接收返回的消息
                int length = clinet.Receive(recBuf);
                string reslut = Encoding.Default.GetString(recBuf, 0, length);

                //反序列化数据
                ReturnState returnMessage = JsonConvert.DeserializeObject<ReturnState>(reslut);

                //将返回的数据添加到返回数据列表对象中
                ReturnMessageList.Add(returnMessage.StateCode, returnMessage);

                //提示指示灯已接收数据
                AutoResetEventList[returnMessage.StateCode].Set();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 将获取到的磁盘信息添加到页面显示列表中
        /// </summary>
        /// <param name="hardDisks">磁盘信息</param>
        private void FileListView_hardDisk(List<HardDiskPartition> hardDisks)
        {
            //重新赋值页面列表列头
            FileListView.Columns[1].Text = "总空间";
            FileListView.Columns[2].Text = "剩余大小";
            FileListView.Columns[3].Text = "";

            //清理列表内的数据项
            FileListView.Items.Clear();

            //迭代获取到的磁盘信息
            hardDisks.ForEach(x =>
            {
                //初始化一个列表项
                ListViewItem listViewItem = new ListViewItem();

                //显示图片
                listViewItem.ImageIndex = (int)FileTypes.HardDisk;

                //名称
                listViewItem.Text = x.PartitionName;

                //总空间
                listViewItem.SubItems.Add(new ListViewItem.ListViewSubItem { Text = string.Format("{0}G", Convert.ToInt32(x.SumSpace)) });

                //剩余大小
                listViewItem.SubItems.Add(new ListViewItem.ListViewSubItem { Text = string.Format("{0}G", Convert.ToInt32(x.FreeSpace)) });

                //将列表项添加到列表中
                FileListView.Items.Add(listViewItem);
            });
        }

        /// <summary>
        /// 将获取到的文件夹和文件信息添加到页面显示列表中
        /// </summary>
        /// <param name="files"></param>
        private void FileListView_FolderAndFile(List<FileDetails> files)
        {
            //修改列头名称
            FileListView.Columns[1].Text = "时间";
            FileListView.Columns[2].Text = "类别";
            FileListView.Columns[3].Text = "大小";
            FileListView.Items.Clear();

            files.ForEach(x =>
            {
                //文件类型
                string Filetype = x.FileName.Split('.').Count() > 1 ? x.FileName.Split('.').Last() : "";

                //创建列表项
                ListViewItem listViewItem = new ListViewItem();

                //选择图片
                listViewItem.ImageIndex = VerifyFileType(Filetype, x.Size);

                //添加名称
                listViewItem.Text = x.FileName;

                //添加时间项
                listViewItem.SubItems.Add(new ListViewItem.ListViewSubItem { Text = string.Format("{0}", x.FileCreateDate) });

                //添加类别项
                listViewItem.SubItems.Add(new ListViewItem.ListViewSubItem { Text = string.Format("{0}", listViewItem.ImageIndex == 6 ? "未知文件" : Filetype.Length > 0 ? Filetype + "文件" : x.Size > 0 ? "未知文件" : "文件夹") });

                //添加大小项
                if (listViewItem.ImageIndex != 7)
                    listViewItem.SubItems.Add(new ListViewItem.ListViewSubItem { Text = string.Format("{0}KB", x.Size) });

                //将列表项添加至显示列表中
                FileListView.Items.Add(listViewItem);
            });
        }

        /// <summary>
        /// 记录Log
        /// </summary>
        /// <param name="StackTrace"></param>
        /// <param name="Message"></param>
        /// <param name="FunctionName"></param>
        /// <param name="IsHint"></param>
        public void Log(string StackTrace, string Message, string FunctionName, bool IsHint)
        {
            if (!System.IO.File.Exists(LogTextPath))
            {
                FileStream fs = new FileStream(LogTextPath, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs, Encoding.Default);
                sw.WriteLine("方法名：" + FunctionName);
                sw.WriteLine("堆栈踪迹：" + StackTrace);
                sw.WriteLine("错误信息：" + Message);
                sw.WriteLine("错误时间：" + DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"));
                sw.WriteLine("**************************************************");
                sw.WriteLine();
                sw.Flush();
                sw.Close();
                sw.Dispose();
                sw.Close();
            }
            else
            {
                StreamWriter sw = new StreamWriter(LogTextPath, true, Encoding.Default);
                sw.WriteLine("方法名：" + FunctionName);
                sw.WriteLine("堆栈踪迹：" + StackTrace);
                sw.WriteLine("错误信息：" + Message);
                sw.WriteLine("错误时间：" + DateTime.Now.ToString("yyy-MM-dd HH:mm:ss"));
                sw.WriteLine("**************************************************");
                sw.WriteLine();
                sw.Flush();
                sw.Close();
                sw.Dispose();
            }
            if (IsHint)
                MessageBox.Show("出现错误，请查看日志！");
        }

        /// <summary>
        /// 弹出提示信息
        /// </summary>
        /// <param name="Value">文本</param>
        private void MessageBoxShow(string Value)
        {
            _this.Invoke(new Action(() => {
                MessageBox.Show(Value);
            }));
        }

        /// <summary>
        /// 向clinet客户端发送信息
        /// </summary>
        /// <param name="StateCode"></param>
        private void Clinet_Send(int StateCode, string Value = "")
        {
            //定义需要传输的数据
            transmitMessage.StateCode = StateCode;
            transmitMessage.Value = Value;
            transmitMessage.Path = OpenPath;

            //删除旧的指示灯，并添加一个新的
            AutoResetEventList.Remove(transmitMessage.StateCode);
            AutoResetEventList.Add(transmitMessage.StateCode, new AutoResetEvent(false));

            //开启线程池接收信息
            ThreadPool.QueueUserWorkItem(ReceiveMessages);

            //将刷新信息发送至客户端
            clinet.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(transmitMessage)));

            //等待发送信息的线程完成
            AutoResetEventList[transmitMessage.StateCode].WaitOne();
        }

        /// <summary>
        /// 选择新的连接刷新页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnList_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshPage();
        }

        /// <summary>
        /// 双击显示列表项执行打开操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FileListView_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                //需要打开的路径
                OpenPath += FileListView.SelectedItems[0].Text.ToString() + "\\";
                //初始化待发送的信息
                transmitMessage = new TransmitMessage();

                //向客户端发送信息
                Clinet_Send(2);

                //判断是否完成任务
                if (ReturnMessageList[transmitMessage.StateCode].State)
                {
                    //反序列化文件信息
                    List<FileDetails> files = JsonConvert.DeserializeObject<List<FileDetails>>(ReturnMessageList[transmitMessage.StateCode].Message);

                    //将数据写入页面显示列表
                    FileListView_FolderAndFile(files);

                    //清理已使用的数据
                    ReturnMessageList.Remove(transmitMessage.StateCode);
                }
                else
                {
                    MessageBoxShow(string.Format("客户端发生错误，错误信息：{0}", ReturnMessageList[transmitMessage.StateCode].Message));
                }


            }
            catch (Exception ex)
            {
                Log(ex.StackTrace, ex.Message, "AddConnection", true);
                MessageBoxShow(ex.Message);
            }
        }

        /// <summary>
        /// 根据文件类型返回ImageList索引
        /// </summary>
        /// <param name="FileName">文件名</param>
        /// <param name="FileSize">文件大小</param>
        /// <returns></returns>
        private int VerifyFileType(string FileName, long FileSize)
        {
            string Suffix = FileName.ToUpper();
            switch (Suffix)
            {
                case "DOCX":
                    return 0;
                case "DOC":
                    return 0;
                case "XLSX":
                    return 1;
                case "XLS":
                    return 1;
                case "CVS":
                    return 1;
                case "TXT":
                    return 2;
                case "RAR":
                    return 3;
                case "ZIP":
                    return 3;
                case "SQL":
                    return 4;
                case "CS":
                    return 5;
                case "":
                    if (FileSize == 0)
                        return 7;
                    return 6;
                default:
                    return 6;
            }
        }

        /// <summary>
        /// 返回上一级目录
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpLevel_Click(object sender, EventArgs e)
        {
            try
            {
                //需要打开的路径
                OpenPath = ParentPath;
                //初始化待发送的信息
                transmitMessage = new TransmitMessage();

                //向客户端发送信息
                if (string.IsNullOrEmpty(OpenPath))
                {
                    //获取磁盘信息
                    Clinet_Send(1);
                }
                else
                {
                    //获取文件夹信息
                    Clinet_Send(2);
                }

                //判断是否完成任务
                if (ReturnMessageList[transmitMessage.StateCode].State)
                {
                    if (string.IsNullOrEmpty(OpenPath))
                    {
                        //反序列化磁盘信息
                        List<HardDiskPartition> hardDisk = JsonConvert.DeserializeObject<List<HardDiskPartition>>(ReturnMessageList[transmitMessage.StateCode].Message);

                        //将获取到的磁盘信息写入页面列表
                        FileListView_hardDisk(hardDisk);

                        //清理获取的磁盘信息
                        hardDisk.Clear();
                    }
                    else
                    {
                        //反序列化文件信息
                        List<FileDetails> files = JsonConvert.DeserializeObject<List<FileDetails>>(ReturnMessageList[transmitMessage.StateCode].Message);

                        //将数据写入页面显示列表
                        FileListView_FolderAndFile(files);

                        //清理获取的文件信息
                        files.Clear();
                    }

                    //清理已使用的数据
                    ReturnMessageList.Remove(transmitMessage.StateCode);
                }
                else
                {
                    MessageBoxShow(string.Format("客户端发生错误，错误信息：{0}", ReturnMessageList[transmitMessage.StateCode].Message));
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}