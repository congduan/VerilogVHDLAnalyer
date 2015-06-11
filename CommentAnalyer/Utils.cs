using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;

namespace CommentAnalyer
{
    class Utils
    {
        /// <summary>
        /// 将值写入配置文件app.config
        /// </summary>
        /// <param name="key">键值</param>
        /// <param name="value">配置值</param>
        public static void WriteToAppConfig(string key, string value)
        {
            Configuration cfa = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (!cfa.AppSettings.Settings.AllKeys.Contains(key))
                cfa.AppSettings.Settings.Add(key, value);
            else
                cfa.AppSettings.Settings[key].Value = value;
            cfa.Save();
        }

        /// <summary>
        /// 从app.config配置文件读取对应键的值
        /// </summary>
        /// <param name="key">键值</param>
        /// <returns>配置值</returns>
        public static string ReadFromAppConfig(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        /// <summary>
        /// KMP算法查找字符串：貌似没有系统的快
        /// </summary>
        /// <param name="operateStr">操作字符串</param>
        /// <param name="findStr">要查找的字符串</param>
        /// <returns>字符串第一次出现的位置索引</returns>       
        public static int StringIndexOf_KMP(string operateStr, string findStr)
        {
            int i = 0, j = 0, v;
            int[] nextVal = GetNextVal(findStr);

            while (i < operateStr.Length && j < findStr.Length)
            {
                if (j == -1 || operateStr[i] == findStr[j])
                {
                    i++;
                    j++;
                }
                else
                {
                    j = nextVal[j];
                }
            }

            if (j >= findStr.Length)
                v = i - findStr.Length;
            else
                v = -1;

            return v;
        }

        private static int[] GetNextVal(string t)
        {
            int j = 0, k = -1;
            int[] nextVal = new int[t.Length];

            nextVal[0] = -1;

            while (j < t.Length - 1)
            {
                if (k == -1 || t[j] == t[k])
                {
                    j++;
                    k++;
                    if (t[j] != t[k])
                    {
                        nextVal[j] = k;
                    }
                    else
                    {
                        nextVal[j] = nextVal[k];
                    }
                }
                else
                {
                    k = nextVal[k];
                }
            }

            return nextVal;
        }
    }
}
