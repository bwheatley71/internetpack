﻿/*---------------------------------------------------------------------------
  RemObjects Internet Pack for .NET
  (c)opyright RemObjects Software, LLC. 2003-2016. All rights reserved.
---------------------------------------------------------------------------*/

using RemObjects.InternetPack.Events;

namespace RemObjects.InternetPack.CommandBased
{
	public abstract class CommandBasedClient : Client
	{
		protected CommandBasedClient()
		{
		}

		#region Properties
		public Connection CurrentConnection
		{
			get
			{
				return this.fCurrentConnection;
			}
		}
		private Connection fCurrentConnection;

		public String LastResponseText
		{
			get
			{
				return this.fLastResponseText;
			}
		}
		private String fLastResponseText;

		public Int32 LastResponseNo
		{
			get
			{
				return this.fLastResponseNo;
			}
		}
		private Int32 fLastResponseNo;

		public Boolean Connected
		{
			get
			{
				return (this.fCurrentConnection != null) && (this.fCurrentConnection.Connected);
			}
		}
		#endregion

		protected Boolean WaitForResponse(params Int32[] validResponses)
		{
			String lDataString = fCurrentConnection.ReadLine();

			this.SendLog(TransferDirection.Receive, lDataString);

			String[] lResp;
			Boolean lMultline;
			Int32 i1 = lDataString.IndexOf('-');
			Int32 i2 = lDataString.IndexOf(' ');

			if (i1 == -1 || ((i1 > i2) && (i2 != -1)))
			{
				lResp = new String[2];
				lResp[0] = lDataString.Substring(0, i2);
				lResp[1] = lDataString.Substring(i2 + 1);
				lMultline = false;
			}
			else
			{
				lResp = new String[2];
				lResp[0] = lDataString.Substring(0, i1);
				lResp[1] = lDataString.Substring(i1 + 1);
				lMultline = true;
			}

			try
			{
				this.fLastResponseNo = Convert.ToInt32(lResp[0]);
			}
			catch
			{
				throw new Exception("Invalid response from server");
			}

			if (lMultline)
			{
				String lStopSign = lResp[0] + ' ';
				StringBuilder lFullResponse = new StringBuilder(lResp[1]);

				while (true)
				{
					lDataString = fCurrentConnection.ReadLine();
					SendLog(TransferDirection.Receive, lDataString);

					if (lDataString.StartsWith(lStopSign))
					{
						lFullResponse.Append(lDataString.Substring(lStopSign.Length));
						break;
					}

					lFullResponse.Append(lDataString);
					lFullResponse.Append('\n');
				}
				fLastResponseText = lFullResponse.ToString();
			}
			else
			{
				fLastResponseText = lResp[1];
			}

			for (Int32 i = 0; i < validResponses.Length; i++)
			{

				if (fLastResponseNo == validResponses[i])
					return true;
			}

			return false;
		}

		protected Boolean SendAndWaitForResponse(String command, params Int32[] validResponses)
		{
			if (!this.Connected)
				throw new Exception("Connection not open");

			this.SendLog(TransferDirection.Send, command);
			this.fCurrentConnection.WriteLine(command);

			return this.WaitForResponse(validResponses);
		}

		public virtual void Open()
		{
			if (this.fCurrentConnection != null)
			{
				if (this.fCurrentConnection.Connected)
					this.fCurrentConnection.Close();
				this.fCurrentConnection = null;
			}

			this.fCurrentConnection = this.Connect();
		}

		public virtual void Close()
		{
			if (this.fCurrentConnection != null)
			{
				this.fCurrentConnection.Close();
				this.fCurrentConnection = null;
			}
		}

		#region Log handling
		public event ClientLogEvent OnLog;

		protected virtual void SendLog(TransferDirection direction, String message)
		{
			if (this.OnLog != null)
				this.OnLog(this, new ClientLogArgs(direction, message));
		}

		protected virtual void SendLog(TransferDirection direction, String format, params Object[] parameters)
		{
			if (this.OnLog != null)
				this.OnLog(this, new ClientLogArgs(direction, format, parameters));
		}
		#endregion
	}

	#region Delegates and arguments
	public class ClientLogArgs
	{
		public ClientLogArgs(TransferDirection direction, String message)
		{
			this.Direction = direction;

			if (message.StartsWith("PASS"))
				message = "PASS (hidden)";

			this.Text = message;
		}

		public ClientLogArgs(TransferDirection direction, String format, params Object[] parameters)
			: this(direction, String.Format(format, parameters))
		{
		}

		#region Properties
		public TransferDirection Direction { get; set; }

		public String Text { get; set; }
		#endregion
	}

	public delegate void ClientLogEvent(Object sender, ClientLogArgs e);
	#endregion

	#region Exceptions
#if FULLFRAMEWORK
	[System.Serializable()]
#endif
	public class CmdResponseException : Exception
	{
		public CmdResponseException(String error, Int32 responseCode, String responseText) :
			base(String.Format("{0}: {1} {2}", error, responseCode, responseText))
		{
		}
	}
	#endregion
}