using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimerControl
{
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
        /// <summary>
        /// 可用空间
        /// </summary>
        public double FreeSpace { get; set; }
        
        /// <summary>
        /// 已用空间
        /// </summary>
        public double UseSpace { get; set; }
        
        /// <summary>
        /// 总空间
        /// </summary>
        public double SumSpace { get; set; }
        
        /// <summary>
        /// 盘符
        /// </summary>
        public string PartitionName { get; set; }
        
        /// <summary>
        /// 是否是系统盘
        /// </summary>
        public bool IsPrimary { get; set; }

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
    /// 显示文件图片
    /// </summary>
    enum FileTypes
    {
        Docx_Doc,
        Xlsx_Xls_CVS,
        Txt,
        Rar_Zip,
        Sql,
        Cs,
        Null,
        Folder,
        HardDisk
    }
}
