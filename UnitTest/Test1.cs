/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2014-8-14
 * Time: 12:24
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace UnitTest
{
	[TestFixture]
	public class Test1
	{
		[Test]
		public void TestMethod()
		{
			string text = "weq12/23/34gdhgs12-23=34";
			string pattern = @"12.23.34";
            Match m = Regex.Match(text, pattern);   // 匹配正则表达式
            if (m.Success)   // 匹配成功
            {
            	Console.WriteLine("匹配成功!:"+m.Captures[1]);
            }
            else
            {
            	Console.WriteLine("匹配失败!");
            }
			
		}
	}
}
