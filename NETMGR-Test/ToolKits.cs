using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Moq;

/*
 * Helper tools for Unit Testing
 */

/*
 * 
 */
namespace NETMGR_Test
{
    static class RandomTool
    {
        private static Random random = new Random();
        public static string String(int len=-1)
        {
            if (len < 0) len = Int(0,100);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string (Enumerable.Repeat(chars, len).Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static int Int(int min=0,int max= int.MaxValue)
        {
            return random.Next(min, max);
        }
        public static byte[] ByteArray(int size=1024)
        {
            byte[] b = new byte[size];
            random.NextBytes(b);
            return b;
        }
    }
}
