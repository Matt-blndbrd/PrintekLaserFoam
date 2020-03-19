﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrintekLaserFoam.ComWrapper
{
	public enum WrapperType
	{ UsbSerial, Telnet, LaserWebESP8266, Emulator }

	public interface IComWrapper
	{
		void Configure(params object[] param);

		void Open();
		void Close(bool auto);

		bool IsOpen { get; }
		
		void Write(byte b);
        void Write(byte[] arr);
        void Write(string text);

		string ReadLineBlocking();

		bool HasData();
	}
}
