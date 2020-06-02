using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoStream
{
    public static class Parser
    {

        public static byte[] EncodeText(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        public static string DecodeText(byte[] data)
        {
            return Encoding.ASCII.GetString(data);
        }

        public static byte[] EncodeImage()
        {
            return null;
        }


    }
}
