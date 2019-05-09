using System;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;

namespace MyWebSocket
{
	/*
	* предоставляет сведения о соединении по TcpClient или по строке подключения
	*/
	public class Connect
	{
		private IPAddress _address;
		private int _port;
		private bool _isEncrypt;
		private IPEndPoint endPoint;

		private static Regex regex = new Regex(@"^(?<method>[a-z]{2,3})://(?<ip>[0-9\.]+):(?<port>\d+)$");

		public int port { 
			get {
				return _port;
			}
		}
		public IPAddress address {
			get {
				return _address;
			}
		}
		public bool isEncrypt {
			get { return _isEncrypt; }
		}
		public IPEndPoint EndPoint {
			get {
				return endPoint;
			}
		}

		public Connect(String url) {//url example ws://127.0.0.1:10001
			Match match = regex.Match(url);
			_address = IPAddress.Parse(match.Groups["ip"].Value);
			_port = Int32.Parse(match.Groups["port"].Value);
			endPoint = new IPEndPoint(_address, _port);
			string method = match.Groups["method"].Value;
			if (method == "ws") {
				_isEncrypt = false;
			}
			else if (method == "wsm")
			{
				_isEncrypt = true;
			}
			else {
				//error
			}
		}
		internal Connect(TcpClient client, bool enryption) {
			endPoint = (IPEndPoint)client.Client.RemoteEndPoint;
			_address = endPoint.Address;
			_port = endPoint.Port;
			_isEncrypt = enryption;
		}
	}
}
