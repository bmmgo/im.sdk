/*
* 描述： 
* 创建人：张斌
* 创建时间：2017/8/7 14:17:19
*/
/*
*修改人：XXX
*修改时间：xxx
*修改内容：xxxxxxx
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IM.Test
{
    public class ReplayTxt
    {
        private static List<string> _txtList = new List<string>();

        static ReplayTxt()
        {
            using (var sr = new StreamReader("replay.txt", Encoding.UTF8))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    _txtList.Add(line);
                }
            }
        }

        public static string GetTxt()
        {
            return _txtList[new Random().Next(0, _txtList.Count - 1)];
        }
    }
}
