using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace CommentAnalyer
{
    class FileHelper
    {
        private static void GetDirectorys(string strPath, ref List<string> lstDirect)  
        {  
            DirectoryInfo diFliles = new DirectoryInfo(strPath);  
            DirectoryInfo[] diArr = diFliles.GetDirectories();  
            //DirectorySecurity directorySecurity = null;  
            foreach (DirectoryInfo di in diArr)  
            {  
                try  
                {  
                    //directorySecurity = new DirectorySecurity(di.FullName, AccessControlSections.Access);  
                    //if (!directorySecurity.AreAccessRulesProtected)  
                    //{  
                    lstDirect.Add(di.FullName);  
                    GetDirectorys(di.FullName, ref lstDirect);  
                    //}  
                }  
                catch   
                {  
                    continue;  
                }  
            }  
        }  
        /// <summary>  
        /// 遍历当前目录及子目录  
        /// </summary>  
        /// <param name="strPath">文件路径</param>  
        /// <returns>所有文件</returns>  
        private static IList<FileInfo> GetFiles(string strPath)  
        {  
            List<FileInfo> lstFiles = new List<FileInfo>();  
            List<string> lstDirect = new List<string>();  
            lstDirect.Add(strPath);  
            DirectoryInfo diFliles = null;  
            GetDirectorys(strPath, ref lstDirect);  
            foreach (string str in lstDirect)  
            {  
                try  
                {  
                    diFliles = new DirectoryInfo(str);  
                    lstFiles.AddRange(diFliles.GetFiles());  
                }  
                catch   
                {  
                    continue;  
                }  
            }  
            return lstFiles;  
        }

        public ObservableCollection<FileListModel> filelist = new ObservableCollection<FileListModel>();
        /// <summary>
        /// 获取子目录所有文件
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public void ListFiles(FileSystemInfo info, List<string> fileFilters)
        {
            try
            {

                if (!info.Exists)
                    return;
                DirectoryInfo dir = info as DirectoryInfo;
                //不是目录
                if (dir == null)
                    return;
                FileSystemInfo[] files = dir.GetFileSystemInfos();
                for (int i = 0; i < files.Length; i++)
                {
                    FileInfo file = files[i] as FileInfo;
                    //是文件
                    if (file != null)
                    {
                        //Debug.WriteLine("搜索文件：" + file.FullName);
                        for (int j = 0; j < fileFilters.Count; j++)
                        {
                            if (file.FullName.Contains("."))
                            {
                                if (file.FullName.Substring(file.FullName.LastIndexOf(".")) == fileFilters[j].Substring(1, fileFilters[j].Length - 1))
                                {
                                    filelist.Add(new FileListModel(file.Name, file.FullName));
                                    break;
                                }
                            }
                        }
                    }
                    //对于子目录，进行递归调用
                    else
                    {
                        ListFiles(files[i], fileFilters);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message); 
            };
        }
    }      
}
