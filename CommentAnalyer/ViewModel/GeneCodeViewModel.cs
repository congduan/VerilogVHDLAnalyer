/*
 * Created by SharpDevelop.
 * User: Administrator
 * Date: 08/18/2014
 * Time: 18:08
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows;
using System.Windows.Input;

using CommentAnalyer.Commands;
using CommentAnalyer.View;

namespace CommentAnalyer.ViewModel
{
	/// <summary>
	/// Description of GeneCodeViewModel.
	/// </summary>
	public class GeneCodeViewModel
	{
		#region commands
		/// <summary>
		/// 生成代码
		/// </summary>
		private ICommand _GeneCodeCommand;
		public ICommand GeneCodeCommand
        {
            get
            {
                if (_GeneCodeCommand == null)
                    _GeneCodeCommand = new RelayCommand(param =>
                    {
                    	switch (param.ToString()) {
                			case "regheap":
                				//TODO 寄存器堆
                				MessageBox.Show("待完成...");
            					break;
            				case "ram":
            					//TODO RAM
            					MessageBox.Show("待完成...");
            					break;
            				case "fifo":
            					//TODO FIFO
            					MessageBox.Show("待完成...");
            					break;
            				case "lihua":
            					//TODO 例化
            					MessageBox.Show("待完成...");
            					break;
            				case "dapai":
                				//TODO 打拍
                				MessageBox.Show("待完成...");
                				break;
                			default:
                				
                				break;
                		}
                 	},
                    param =>
                    {
                        return true;
                    });
                return _GeneCodeCommand;
            }
        }
		
		#endregion
		
		
		public GeneCodeViewModel()
		{
			
		}
	}
}
