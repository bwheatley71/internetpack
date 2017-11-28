﻿using RemObjects.Elements.RTL;

namespace RemObjects.InternetPack.Shared.Base
{
	public abstract class TextReader: MarshalByRefObject, IDisposable
	{
		protected Encoding fEncoding;

		virtual void Dispose()
		{

		}

		public virtual void Flush()
		{

		}

		public virtual string ReadToEnd()
		{
			return "";
		}

		public abstract void Write(string Value);

        public abstract int Peek();

        public abstract String ReadLine();

        public abstract void Close();

        public abstract int Read(Char[] buffer, int index, int count);
	    
        public abstract int Read();
	}

	public class StreamReader : TextReader
	{
		protected Stream fStream;

		public StreamReader(Stream stream)
		{
			fStream = stream;
			fEncoding = Encoding.UTF8;
		}

		public override string ReadToEnd()
		{
			var lBytes = fStream.Length - fStream.Position;
			var lBuffer = new byte[lBytes];
			fStream.Read(lBuffer, 0, lBytes);

			return fEncoding.GetString(lBuffer, 0, lBytes);
		}

        public override int Peek()
        {
            var lOldPos = fStream.Position;
            var lByte = fStream.ReadByte();
            fStream.Position = lOldPos;

            return lByte;
        }

        public override string ReadLine()
        {

        }

        public override void Close()
        {
            fStream.Close();
        }

        public override int Read(Char[] buffer, int index, int count) 
        {
            return -1;
        }
	    
        public override int Read() 
        { 
            return fStream.ReadByte();
        } 
	}

	public class StreamWriter : StreamReader
	{
		public override void Flush()
		{
			fStream.Flush();
		}

		public virtual void WriteLine()
		{
			WriteLine("");
		}

		public virtual void WriteLine(string Value)
		{
			var lBuffer = fEncoding.GetBytes(Value + Environment.LineBreak);
			fStream.Write(lBuffer, lBuffer.Length);
		}

		public override void Write(string Value)
		{
			var lBuffer = fEncoding.GetBytes(Value);
			fStream.Write(lBuffer, lBuffer.Length);
		}
	}

	public class BinaryReader
	{
		 protected BinaryStream fBinStream;

		 public BinaryReader(Stream Value)
		 {
			fBinStream = new BinaryStream(Value);
		 }

		 public byte ReadByte()
		 {
			 return fBinStream.ReadByte();
		 }

		 public sbyte ReadSByte()
		 {
			 return (sbyte)fBinStream.ReadByte();
		 }

		 public byte[] Read(Int32 Length)
		 {
			 return fBinStream.Read(Length);
		 }

		 public Int32 PeekChar()
		 {
			 return fBinStream.PeekChar();
		 }
	}

	public class BinaryWriter
	{
		 protected BinaryStream fBinStream;

		 public BinaryWriter(Stream Value)
		 {
			fBinStream = new BinaryStream(Value);
		 }

		 public void Write(byte Value)
		 {
			 fBinStream.Write(Value);
		 }

		 public void Write(byte[] Value)
		 {
			Write(Value, 0, length(Value));
		 }

		 public void Write(byte[] Value, Int32 Offset, Int32 Count)
		 {
			fBinStream.Write(Value, Offset, Count);
		 }

		 public void Flush()
		 {
            
		 }
	}

    public class StringReader: TextReader
    {
        private String fData;
        private int fPos = 0;
        private int fLength;

        public StringReader(String s)        
        {
            fData = s;
            fLength = s.Length;
        }

	    public override void Dispose()
        {
            fData = null;
            base.Dispose();
        }

        public override void Close()        
        {
            Dispose();
        }

	    public override int Peek()        
        {

        }

	    public override int Read(Char[] buffer, int index, int count)
        {
        }

	    public override int Read()
        {

        }

	    public override String ReadToEnd()
        {

        }

	    public override String ReadLine()
        {

        }
    }

	#if echoes
	public class WrappedStream: System.IO.Stream
	{
		private Stream fStream;

		public WrappedStream(Stream Input)
		{
			fStream = Input;
		}

		public override void Flush()
		{
			fStream.Flush();
		}

		public override void Close()
		{
			fStream.Close();
		}

		public override Int32 Read(Byte[] buffer, Int32 offset, Int32 size)
		{
			return fStream.Read(buffer, offset, size);
		}

		public override void Write(Byte[] buffer, Int32 offset, Int32 size)
		{
			fStream.Write(buffer, offset, size);
		}

		public override Int64 Seek(Int64 offset, PlatformSeekOrigin origin)
		{
			return fStream.Seek(offset, (SeekOrigin)origin);
		}

		public override Boolean CanRead
		{
			get
			{
				return fStream.CanRead;
			}
		}

		public override Boolean CanSeek
		{
			get
			{
				return fStream.CanSeek;
			}
		}

		public override Boolean CanWrite
		{
			get
			{
				return fStream.CanWrite;
			}
		}

		public override void SetLength(Int64 length)
		{

		}

		public override Int64 Length
		{
			get
			{
			  return fStream.GetLength();
			}
		}

		public override Int64 Position
		{
			get
			{
			  return fStream.GetPosition();
			}

			set
			{
				fStream.SetPosition(value);
			}
		}
	}
	#endif
}