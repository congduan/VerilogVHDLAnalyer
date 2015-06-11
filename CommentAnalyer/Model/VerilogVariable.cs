/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 2014-8-18
 * Time: 17:16
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using CommentAnalyer.StructsEnums;
using System.ComponentModel;

namespace CommentAnalyer.Model
{
	/// <summary>
	/// verilog变量
	/// </summary>
	public class VerilogVariable
	{
        /// <summary>
        /// 变量类型
        /// </summary>
        [DefaultValue(VariableType.Wire)]
		public VariableType VariableType{set;get;}

        /// <summary>
        /// 位宽
        /// </summary>
        [DefaultValue(1)]
        public int BitWidth { set; get; }

        /// <summary>
        /// 变量名称
        /// </summary>
        public string Name { set; get; }

        [DefaultValue(false)]
        public bool IsPort { set; get; }

        public VerilogVariable()
        {
            VariableType = StructsEnums.VariableType.Wire;
        }

        public VerilogVariable(VariableType type, string name, int bitwidth)
		{
            VariableType = type;
			Name = name;
			BitWidth = bitwidth==0?1:bitwidth;
		}
	}
}
