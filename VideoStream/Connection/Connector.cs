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

        protected ConnectionType connectionType;

        protected Stream connection;
        protected byte[] buffer = new byte[App.BUFFER_SIZE];

        public virtual void Connect(ConnectionType connectionType)
        {
            this.connectionType = connectionType;
        }

        public abstract void Read();
        public abstract void Write<T>(T data);

        public abstract void Start();
        public abstract void Stop();

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
