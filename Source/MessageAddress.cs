﻿/*---------------------------------------------------------------------------
  RemObjects Internet Pack for .NET
  (c)opyright RemObjects Software, LLC. 2003-2016. All rights reserved.
---------------------------------------------------------------------------*/

namespace RemObjects.InternetPack.Messages
{
	public class MessageAddress
	{
		public MessageAddress()
		{
			this.Name = "";
			this.Address = "";
		}

		public String Name { get; set; }

		public String Address { get; set; }

		public void Clear()
		{
			this.Name = "";
			this.Address = "";
		}

		public Boolean IsSet()
		{
			return (this.Name.Length != 0) || (this.Address.Length != 0);
		}

		[ToString]
        public override String ToString()
		{
			return String.Format("{0} <{1}>", this.Name, this.Address);
		}

		public void FromString(String input)
		{
			Int32 i = input.IndexOf('<');
			if (i == -1)
			{
				this.Address = input;
				this.Name = "";
			}
			else
			{
				this.Name = input.Substring(0, i).Trim();
				input = input.Substring(i + 1);
				i = input.IndexOf('>');

				if (i != -1)
					input = input.Substring(0, i);

				this.Address = input;
			}
		}

		public static MessageAddress ParseAddress(String address)
		{
			MessageAddress lAddress = new MessageAddress();
			lAddress.FromString(address);

			return lAddress;
		}
	}

	public class MessageAddresses
	{
		private List<MessageAddress> fData;
        
        public MessageAddress Add(String name, String address)
		{
			MessageAddress item = new MessageAddress();
			item.Name = name;
			item.Address = address;

			fData.Add(item);

			return item;
		}

		public MessageAddress Add(String address)
		{
			MessageAddress item = new MessageAddress();
			item.FromString(address);

			fData.Add(item);

			return item;
		}

        public MessageAddress Add(MessageAddress address)
        {
            fData.Add(address);

            return address;
        }

        [ToString]
		public override String ToString()
		{
			StringBuilder lResult = new StringBuilder();
			Boolean lIsFirstItem = true;

			foreach (MessageAddress address in fData)
			{
				if (!lIsFirstItem)
				{
					lResult.Append(", ");
				}
				else
				{
					lIsFirstItem = false;
				}

				lResult.Append(address.ToString());
			}

			return lResult.ToString();
		}

		public static MessageAddresses ParseAddresses(ISequence<String> addresses)
		{
			MessageAddresses lAddresses = new MessageAddresses();

			foreach (String address in addresses)
			{
				lAddresses.Add(MessageAddress.ParseAddress(address));
			}

			return lAddresses;
		}

        public MessageAddress this[Int32 Index]
        {
            get
            {
                return fData[Index];
            }
            set
            {
                fData[Index] = value;
            }
        }

        public int Count
        {
            get
            {
                return fData.Count;
            }
        }
	}
}