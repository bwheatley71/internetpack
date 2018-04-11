﻿/*---------------------------------------------------------------------------
  RemObjects Internet Pack for .NET
  (c)opyright RemObjects Software, LLC. 2003-2016. All rights reserved.
---------------------------------------------------------------------------*/

using RemObjects.InternetPack.CommandBased;
using RemObjects.InternetPack.Events;
using RemObjects.InternetPack.Messages;
using RemObjects.InternetPack.Shared.Base;

namespace RemObjects.InternetPack.Email
{
#if DESIGN
	[System.Drawing.ToolboxBitmap(typeof(RemObjects.InternetPack.Server), "Glyphs.Pop3Client.bmp")]
#endif
	public class Pop3Client : CommandBasedClient
	{
		public Pop3Client()
		{
			this.Port = 110;
			this.fMessages = new List<MailStatus> withCapacity(32);
		}

		#region Private fields
		private readonly List<MailStatus> fMessages;
		#endregion

		class MailStatus
		{
			public Boolean Retrieved;
			public MailMessage Message;
		}

		public String User { get; set; }

		public String Password { get; set; }

		public override void Open()
		{
			base.Open();

			String lResponse = this.CurrentConnection.ReadLine();
			this.SendLog(TransferDirection.Receive, lResponse);

			if (lResponse.StartsWith("+ERR"))
			{
				this.Close();

				throw new Exception(String.Format("Invalid connection reply: {0}", lResponse));
			}
		}

		public override void Close()
		{
			if (this.Connected)
				this.SendAndReceive("QUIT"); // we don't care about it's result value, we are gonna quit anyway

			base.Close();
		}

		public String SendAndReceive(String command)
		{
			this.SendLog(TransferDirection.Send, command);
			this.CurrentConnection.WriteLine(command);

			String lResponse = this.CurrentConnection.ReadLine();
			this.SendLog(TransferDirection.Receive, lResponse);

			return lResponse;
		}

		public Boolean SendAndCheck(String command)
		{
			String lResponse = this.SendAndReceive(command);

			return lResponse.StartsWith("+OK");
		}

		public void DeleteMessage(Int32 messageIndex, Int32 messageNumber)
		{
			String lCommand = "DELE ";

			if (messageNumber != -1)
				lCommand += messageNumber.ToString();
			else
				lCommand += messageIndex.ToString();

			String lResponse = this.SendAndReceive(lCommand);
			if (lResponse.StartsWith("-ERR"))
				throw new Exception(String.Format("Could not delete message at index {0}: {1}", messageIndex, lResponse));
		}

		public Int32 MessageCount
		{
			get
			{
				return this.fMessages.Count;
			}
		}

		public void ClearMessages()
		{
			this.fMessages.RemoveAll();
		}

		public MailMessage GetHeaders(Int32 messageIndex, Int32 messageNumber)
		{
			String lCommand = "TOP ";

			if (messageNumber == -1)
				lCommand += messageIndex.ToString();
			else
				lCommand += messageNumber.ToString();

			lCommand += " 1";

			String lResponse = SendAndReceive(lCommand);

			if (lResponse.StartsWith("-ERR"))
				throw new Exception(String.Format("Could not retrieve message at index {0}: {1}", messageIndex, lResponse));

			MailMessage lMessage = new MailMessage();
			MemoryStream lStream = new MemoryStream();

			using (StreamWriter writer = new StreamWriter(lStream))
			{
				while (lResponse != ".")
				{
					lResponse = this.CurrentConnection.ReadLine();
					writer.WriteLine(lResponse);
				}

				writer.Flush();

				lStream.Position = 0;
				lMessage.DecodeMessage(lStream);
			}

			return lMessage;
		}

		public MailMessage GetMessage(Int32 messageIndex, Int32 messageNumber)
		{
			if (this.fMessages[messageIndex - 1].Retrieved)
				return this.fMessages[messageIndex - 1].Message;

			String lCommand = "RETR ";

			if (messageNumber == -1)
				lCommand += messageIndex.ToString();
			else
				lCommand += messageNumber.ToString();

			String lResponse = this.SendAndReceive(lCommand);

			if (lResponse.StartsWith("-ERR"))
				throw new Exception(String.Format("Could not retrieve message at index {0}: {1}", messageIndex, lResponse));

			MemoryStream lStream = new MemoryStream();
			using (StreamWriter writer = new StreamWriter(lStream))
			{
				this.fMessages[messageIndex - 1].Retrieved = true;

				while (lResponse != ".")
				{
					lResponse = CurrentConnection.ReadLine();
					writer.WriteLine(lResponse);
				}

				writer.Flush();

				this.fMessages[messageIndex - 1].Message = new MailMessage();
				lStream.Position = 0;
				this.fMessages[messageIndex - 1].Message.DecodeMessage(lStream);
			}

			return this.fMessages[messageIndex - 1].Message;
		}

		public void Login()
		{
			if (!this.SendAndCheck("USER " + this.User))
				throw new Exception("Invalid user or password");

			if (!this.SendAndCheck("PASS " + this.Password))
				throw new Exception("Invalid user or password");
		}

		public void Stat()
		{
			this.fMessages.RemoveAll();

			String lResponse = SendAndReceive("STAT");
			if (lResponse.StartsWith("-ERR"))
				throw new Exception(String.Format("Could not retrieve mailbox status: {0}", lResponse));

			var lResponses = lResponse.Split(" ");
			Int32 lCount = Int32.Parse(lResponses[1]);

			for (Int32 i = 0; i < lCount; i++)
				this.fMessages.Add(new MailStatus());
		}
	}
}