using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace UnityEngine.UI.Windows.Extensions.Net {
    /// <summary>
    /// Package Socket
    /// Based on: git@github.com:laizhihuan/unity-3d-chat.git
    /// </summary>
    public class NetClient {
        private TcpClient client;
        private NetworkStream stream;
        private bool isHead;
        private int len;
        private short type;

		private string name = "net";

		private System.Action<bool> onResult;

        /// <summary>
        /// Initialize the network connection
        /// </summary>
		public void Connect(string host, int port, System.Action<bool> onResult = null) {
			this.onResult = onResult;
            client = new TcpClientDebug();
//			client.NoDelay = true;
			Debug.Log(name + " C: Connect(" + connected + ")");
			connected = false;
			client.BeginConnect(host, port, new AsyncCallback(this.DoСonnect), client);
            isHead = true;
        }

        public void SetName(string value) {
            this.name = value;
        }

		private void DoСonnect(IAsyncResult ar) {
            try {
                if (client != ar.AsyncState) {
                    Debug.Log(name + " changed");
					((TcpClient)ar.AsyncState).Close();
					if (this.onResult != null) this.onResult.Invoke(false);
                    return;
                }

                // Finish asynchronous connect
                client.EndConnect(ar);
				stream = client.GetStream();
				connected = true;

				if (this.onResult != null) this.onResult.Invoke(true);
            } catch(Exception e) {
				if (this.onResult != null) this.onResult.Invoke(false);
                Debug.Log(e);
            }
        }

        public bool Connected() {
            return connected;
        }

		public void SendMsgSilently(short type, byte[] msg) {
            if (connected == false) return;

			Log();
			SendMsg(type, msg);
        }

		public void SendMsg(short type, byte[] msg) {
			//TODO ?? unknown reason, better to remove
			if (client == null) return;
            // Message structure: the message body length + message body
            byte[] data = new byte[4 + msg.Length];
            IntToBytes((int)type << 16 | msg.Length & 0xFFFF) .CopyTo(data, 0);
            msg.CopyTo(data, 4);
            /*if (stream.CanWrite == true)*/ stream.Write(data, 0, data.Length);
//			stream.Flush();
        }

        public void ReceiveMsg() {
			if (client == null || connected == false || client.Connected == false) {
                return;
            }
            if (!stream.CanRead) {
                return;
            }
            // Read the length of the message body
            if (isHead) {
                if (client.Available < 4) {
                    return;
                }
                byte[] lenByte = new byte[4];
                stream.Read(lenByte, 0, 4);
                len = BytesToInt(lenByte, 0);
                type = (short)(len >> 16);
                len &= 0xFFFF;
                isHead = false;
            }
            // Read the message body content
            if (!isHead) {
                if (client.Available < len) {
                    return;
                }
                byte[] msgByte = new byte[len];
                stream.Read(msgByte, 0, len);
                isHead = true;
                len = 0;
                if (onRecMsg != null) {
                    // Process messages
                    onRecMsg(type, msgByte);
                }
            }
        }

        public static int BytesToInt(byte[] data, int offset) {
            int num = 0;
            for (int i = offset; i < offset + 4; i++) {
                num <<= 8;
                num |= (data[i] & 0xff);
            }
            return num;
        }

        public static byte[] IntToBytes(int num) {
            byte[] bytes = new byte[4];
            for (int i = 0; i < 4; i++) {
                bytes[i] = (byte)(num >> (24 - i * 8));
            }
            return bytes;
        }

        public void Close() {
			Debug.Log(name + " C: close(" + connected + ")");
			this.connected = false;
			if (this.client != null) {
				Log();
				var client = this.client;
				this.client = null;
				this.stream = null;
				client.GetStream().Close();
                client.Close();
            }
        }

		/**
		 * Temporarily, debug purposes
		 */
		public void Log() {
			var client = this.client;
            Debug.Log(name + " C: " + client.GetHashCode() + "(" + client.Connected + ")"
                + " S: " + stream.GetHashCode() + "(" + stream.CanRead + "/" + stream.CanWrite + ")"
                + " So: " + client.Client.GetHashCode() + "(" + client.Client.Connected + ")");
		}

        public delegate void OnRevMsg(short type, byte[] msg);

        public OnRevMsg onRecMsg;

        private bool connected;
    }

	/**
	 * Temporarily, debug purposes
	 */
	public class TcpClientDebug : TcpClient {
		public TcpClientDebug() {
			initialize2();
		}

		protected override void Dispose(bool disposing) {
			Debug.Log("TcpDisposed1");
			base.Dispose(disposing);
			Debug.Log("TcpDisposed2");
		}
		private void initialize2() {
			this.Client = new SocketDebug(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		}
	}

	public class SocketDebug : Socket {
		public SocketDebug(System.Net.Sockets.AddressFamily family, System.Net.Sockets.SocketType type, System.Net.Sockets.ProtocolType proto) : base(family, type, proto) {
		}

		protected override void Dispose(bool disposing) {
			Debug.Log("SoDisposed1");
			base.Dispose(disposing);
			Debug.Log("SoDisposed2: " + new StackTrace().ToString());
		}
	}
}