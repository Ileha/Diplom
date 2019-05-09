using System;
using System.Net.NetworkInformation;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace IOTClient
{
	public class Hardware
	{
		public static string GetID()
		{
			IEnumerable<String> macs = (
				from nic in NetworkInterface.GetAllNetworkInterfaces()
				select nic.GetPhysicalAddress().ToString()
			);
			StringBuilder sb = new StringBuilder();
			foreach (String mac in macs)
			{
				sb.Append(mac);
			}
			return CreateMD5(sb.ToString());
		}
		public static string CreateMD5(string input)
		{
			// Use input string to calculate MD5 hash
			using (MD5 md5 = MD5.Create())
			{
				byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
				byte[] hashBytes = md5.ComputeHash(inputBytes);

				// Convert the byte array to hexadecimal string
				StringBuilder sb = new StringBuilder();
				for (int i = 0; i < hashBytes.Length; i++)
				{
					sb.Append(hashBytes[i].ToString("X2"));
				}
				return sb.ToString();
			}
		}
	}
}
