using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VideoStream
{
    public abstract class Connector
    {
        public enum DataType { Text, Image };
        public enum ConnectionType { Read, Write, Both };

        protected Stream connection;
        protected byte[] buffer = new byte[App.BUFFER_SIZE];

        protected abstract void Connect(ConnectionType connectionType);
        protected abstract void Read();
        protected abstract void Write();

        protected abstract void Start();
        protected abstract void Stop();

        protected byte[] Encode<T>(T toEncode, DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Text: return Parser.EncodeText(toEncode.ToString());
            }
            return null;
        }

        protected T Decode<T>(byte[] toDecode, DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Text:return (T)(object)Parser.DecodeText(toDecode);
            }
            return default(T);
        }
    }
}
