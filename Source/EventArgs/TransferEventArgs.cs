﻿/*---------------------------------------------------------------------------
  RemObjects Internet Pack for .NET
  (c)opyright RemObjects Software, LLC. 2003-2016. All rights reserved.
---------------------------------------------------------------------------*/

namespace RemObjects.InternetPack.Events
{
	public delegate void TransferStartEventHandler(Object sender, TransferStartEventArgs e);

	public delegate void TransferEndEventHandler(Object sender, TransferEndEventArgs e);

	public delegate void TransferProgressEventHandler(Object sender, TransferProgressEventArgs e);

	public enum TransferDirection
	{
		None,
		Send,
		Receive
	}

	public class TransferEventArgs
	{
		public TransferEventArgs(TransferDirection direction)
		{
			this.fDirection = direction;
		}

		public TransferDirection TransferDirection
		{
			get
			{
				return this.fDirection;
			}
		}
		private readonly TransferDirection fDirection;
	}

	public class TransferStartEventArgs : TransferEventArgs
	{
		public TransferStartEventArgs(TransferDirection direction, Int64 total)
			: base(direction)
		{
			this.fTotal = total;
		}

		public Int64 Total
		{
			get
			{
				return this.fTotal;
			}
		}
		private readonly Int64 fTotal;
	}

	public class TransferEndEventArgs : TransferEventArgs
	{
		public TransferEndEventArgs(TransferDirection direction)
			: base(direction)
		{
		}
	}

	public class TransferProgressEventArgs : TransferEventArgs
	{
		public TransferProgressEventArgs(TransferDirection direction, Int64 current)
			: base(direction)
		{
			this.fCurrent = current;
		}

		public Int64 Current
		{
			get
			{
				return this.fCurrent;
			}
		}
		private readonly Int64 fCurrent;
	}
}