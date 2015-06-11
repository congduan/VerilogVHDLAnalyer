/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 08/13/2014
 * Time: 20:57
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using CommentAnalyer.StructsEnums;

namespace CommentAnalyer.Model
{

	/// <summary>
	/// 输入输出端口
	/// </summary>
    public class InputOutputPort : VerilogVariable
	{
        /// <summary>
        /// 端口类型
        /// </summary>
        public PortType PortType { set; get; }
		
		public InputOutputPort()
		{
            PortType = StructsEnums.PortType.Input;
            Name = "input1";
            BitWidth = 1;

            IsPort = true;
		}
		
		/// <summary>
		/// 构造函数
		/// </summary>
		/// <param name="type"></param>
		/// <param name="name"></param>
		public InputOutputPort(PortType type,string name,int bitwidth)
		{
			PortType = type;
			Name = name;
			BitWidth = bitwidth==0?1:bitwidth;

            IsPort = true;
		}
	}
}
