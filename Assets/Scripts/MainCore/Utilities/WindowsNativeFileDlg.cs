using UnityEngine;
using System;
using System.Runtime.InteropServices;
 
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
public class OpenFileName
{
    #region Config Field
    public int structSize = 0;
    public IntPtr dlgOwner = IntPtr.Zero;
    public IntPtr instance = IntPtr.Zero;
    public String filter = null;
    public String customFilter = null;
    public int maxCustFilter = 0;
    public int filterIndex = 0;
    public String file = null;
    public int maxFile = 0;
    public String fileTitle = null;
    public int maxFileTitle = 0;
    public String initialDir = null;
    public String title = null;
    public int flags = 0;
    public short fileOffset = 0;
    public short fileExtension = 0;
    public String defExt = null;
    public IntPtr custData = IntPtr.Zero;
    public IntPtr hook = IntPtr.Zero;
    public String templateName = null;
    public IntPtr reservedPtr = IntPtr.Zero;
    public int reservedInt = 0;
    public int flagsEx = 0;
    #endregion
 
 
    #region Win32API WRAP
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
 
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    static extern bool GetOpenFileName([In, Out] LocalDialog dialog);  //这个方法名称必须为GetOpenFileName
 
    //[DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    //static extern bool GetSaveFileName([In, Out] LocalDialog dialog);  //这个方法名称必须为GetSaveFileName
 
    [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern IntPtr SHBrowseForFolder([In, Out] OpenFileName ofn);
 
    [DllImport("shell32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool SHGetPathFromIDList([In] IntPtr pidl, [In, Out] char[] fileName);
 
    #endregion
}
 
public class LocalDialog
{
    public String DisplayName = null;
    public String Title = null;
 
    //链接指定系统函数       打开文件对话框
    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
    //public static bool GetOFN([In, Out] OpenFileName ofn)
    //{
    //    return GetOpenFileName(ofn);
    //}
 
    //窗口置顶
    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();
 
    /// <summary>
    /// 调用window原生，获取存储的文件夹
    /// </summary>
    /// <param name="title">提示信息</param>
    /// <param name="callback">选择后的回调方法</param>
    public static string GetStorageFolderName(string title)
    {
        OpenFileName ofn = new OpenFileName();
        ofn.file = new string(new char[2000]); ; // 存放目录路径缓冲区
        ofn.title = title;// 标题
        IntPtr pidlPtr = OpenFileName.SHBrowseForFolder(ofn);
 
        char[] charArray = new char[2000];
        for (int i = 0; i < 2000; i++)
            charArray[i] = '\0';
 
        ofn.dlgOwner = LocalDialog.GetForegroundWindow(); //这一步将文件选择窗口置顶。
        OpenFileName.SHGetPathFromIDList(pidlPtr, charArray);
        string fullDirPath = new String(charArray);
 
        fullDirPath = fullDirPath.Substring(0, fullDirPath.IndexOf('\0'));

        return fullDirPath;
    }
    /// <summary>
    /// 调用window原生，获取选择的文件
    /// </summary>
    /// <param name="title"> 提示信息 </param>
    /// <param name="filter"> "所有格式(*;)\0*;" </param>
    /// <param name="callback">选择后的回调方法 </param>
    public static string GetLocalFileName(string title, string filter, string initPath = null)
    {
        OpenFileName ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = filter;
        ofn.file = new string(new char[2000]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = ofn.initialDir;
        ofn.title = title;
 
        //注意 一下项目不一定要全选 但是0x00000008项不要缺少
        //OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        ofn.dlgOwner = LocalDialog.GetForegroundWindow(); //这一步将文件选择窗口置顶。
        if (LocalDialog.GetOpenFileName(ofn))
        {
            return ofn.file;
        }

        return null;
    }
}
 