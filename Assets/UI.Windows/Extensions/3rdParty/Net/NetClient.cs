using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;

namespace UnityEngine.UI.Windows.Extensions.Net {
    /// <summary>
    /// Package Socket
    /// Based on: git@github.com:laizhihuan/unity-3d-chat.git
    /// </summary>
    public class NetClient {
        private TcpClient client;
        private System.Timers.Timer timerConnecting;
        private NetworkStream stream;
        private bool isHead;
        private int len;
        private short type;

		private string name = "net";

		private System.Action<bool> onResult;
        private bool active;

        private ConcurrentQueue<System.Action> tasks = new ConcurrentQueue<System.Action>();
        private Thread thread;


        private void ExecOnMainThread(System.Action task) {
            tasks.Enqueue(task);
        }

        /// <summary>
        /// Initialize the network connection
        /// </summary>
        //TODO onResult is called in a foreign thread
		public void Connect(string host, int port, System.Action<bool> onResult = null) {
			if (client != null) Close();
			this.onResult = onResult;

			client = new TcpClient();
//			client = new TcpClientDebug(name);
//			client.NoDelay = true;
			active = false;

            timerConnecting = new System.Timers.Timer(5000);
            timerConnecting.Elapsed += new System.Timers.ElapsedEventHandler(this.OnConnectTimeout);
            timerConnecting.Start();

            Debug.Log("Connecting " + name + ": " + host + ":" + port);
			BeginConnectThread(host, port);
//			BeginConnectNative(host, port);

            isHead = true;
        }

        private void BeginConnectNative(string host, int port) {
			try {
				client.BeginConnect(host, port, new AsyncCallback(this.DoСonnect), client);

			} catch(Exception e) {
                Debug.LogError(e);
                //Stop and dispose timer
                timerConnecting.Dispose();
                timerConnecting = null;

				if (this.onResult != null) this.onResult.Invoke(false);
                this.onResult = null;
            }
        }

        private void CloseThread() {
            if (thread.IsAlive == true) thread.Abort();
            thread = null;
        }

        private void BeginConnectThread(string host, int port) {
            if (thread != null) {
                Debug.LogError(name + " C: connect try, active: " + active + ", thread:" + thread != null + ")");
                CloseThread();
            }

            thread = new Thread(new ThreadStart(() => ConnectThread(host, port)));
            thread.Start();
        }

        private void ConnectThread(string host, int port) {
            try {
                client.Connect(host, port);

                //Stop and dispose timer
                timerConnecting.Dispose();
                timerConnecting = null;

                stream = client.GetStream();
                active = true;
                Debug.Log("Connected " + name);

                ExecOnMainThread(() => {
                    if (this.onResult != null) this.onResult.Invoke(true);
                    this.onResult = null;
                });

            } catch (Exception e) {
                //Stop and dispose timer
                timerConnecting.Dispose();
                timerConnecting = null;

                ExecOnMainThread(() => {
                    Debug.LogError(e);
                    if (this.onResult != null) this.onResult.Invoke(false);
                    this.onResult = null;
                });
            }
            thread = null;
        }

        public void SetName(string value) {
            this.name = value;
        }

		private void DoСonnect(IAsyncResult ar) {
            try {
                if (client != ar.AsyncState) {
                    Debug.LogWarning("Connecting " +  name + " changed. Double-connect call?");
					((TcpClient)ar.AsyncState).Close();
                    return;
                }

                //Stop and dispose timer
                timerConnecting.Dispose();
                timerConnecting = null;

                // Finish asynchronous connect
                client.EndConnect(ar);
				stream = client.GetStream();
				active = true;
                Debug.Log("Connected " + name);

                ExecOnMainThread(() => {
                    if (this.onResult != null) this.onResult.Invoke(true);
                    this.onResult = null;
                });

            } catch(Exception e) {
                ExecOnMainThread(() => {
                    Debug.LogError(e);
                    if (this.onResult != null) this.onResult.Invoke(false);
                    this.onResult = null;
                });
            }
        }

        private void OnConnectTimeout(object sender, System.Timers.ElapsedEventArgs e) {
            //Stop and dispose timer
            timerConnecting.Dispose();
            timerConnecting = null;

            //The connection timed out
            Debug.LogWarning("Сonnection " + name + " timed out!");

            //Close the socket
            Close();

            ExecOnMainThread(() => {
                if (this.onResult != null) this.onResult.Invoke(false);
                this.onResult = null;
            });
        }

        public bool Connected() {
            //MSDN: only reflects the state of the connection as of the most recent operation, you should attempt to send or receive a message to determine the current state.
            return client != null && active == true && client.Connected == true;
        }

        public static TcpState GetTcpConnectionState(TcpClient tcpClient)  {
            var array = IPGlobalProperties.GetIPGlobalProperties()
            .GetActiveTcpConnections();

            if ((array != null) && (array.Length > 0)) {
                var localEP = tcpClient.Client.LocalEndPoint;
                var remoteEP = tcpClient.Client.RemoteEndPoint;

                for (int n = 0; n < array.Length; n++) {
                    var tcp = array[n];
                    if (tcp.LocalEndPoint.Equals(localEP) && tcp.RemoteEndPoint.Equals(remoteEP))  {
                        Debug.Log("Found: " + tcp.State);
                        return tcp.State;
                    }
                }
            }

            return TcpState.Unknown;
        }
        public bool IsConnectedDeep() {
/*
            Debug.Log("ConnectedDeep"
            + " 1: " + IsConnectedDeep1()
            + " 2: " + IsConnectedDeep2()
            + " 3: " + IsConnectedDeep3()
//            + " 4: " + IsConnectedDeep4()
            + " 5: " + IsConnectedDeep5()
            );
*/
            return IsConnectedDeep3();
        }
        public bool IsConnectedDeep1() {
            return GetTcpConnectionState(client) == TcpState.Established;
        }

        public bool IsConnectedDeep4() {
            byte[] buffer = new byte[1];
            stream.Read(buffer, 0, 0);
            return client.Connected;
        }

        public bool IsConnectedDeep3() {
            try {
                return !(client.Client.Poll(1, SelectMode.SelectRead) && client.Client.Available == 0);
            } catch (SocketException) { return false; }
        }

        public bool IsConnectedDeep2() {
            // This is how you can determine whether a socket is still connected.
            bool blockingState = client.Client.Blocking;
            try {
                byte [] tmp = new byte[1];

                client.Client.Blocking = false;
//                client.Client.Send(tmp, 0, 0);
                client.Client.Receive(tmp, 0, 0);
                return true;
            } catch (SocketException e) {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035)) {
//                    Still Connected, but the Send would block
                    return true;
                } else {
                    Debug.LogError("Disconnected: error code " + e.NativeErrorCode);
                    return false;
                }
            } finally {
                client.Client.Blocking = blockingState;
            }
        }

        public bool IsConnectedDeep5() {
            // This is how you can determine whether a socket is still connected.
            bool blockingState = client.Client.Blocking;
            try {
                byte [] tmp = new byte[1];

                client.Client.Blocking = false;
                client.Client.Send(tmp, 0, 0);
//                client.Client.Receive(tmp, 0, 0);
                return true;
            } catch (SocketException e) {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035)) {
//                    Still Connected, but the Send would block
                    return true;
                } else {
                    Debug.LogError("Disconnected: error code " + e.NativeErrorCode);
                    return false;
                }
            } finally {
                client.Client.Blocking = blockingState;
            }
        }

		public void SendMsgSilently(short type, byte[] msg) {

			SendMsg(type, msg);

        }

		public void SendMsg(short type, byte[] msg) {

			if (this.Connected() == false) {
                Debug.LogError("SendMsg On Disconnected " + name);
                return;
            }

			try {
				// Message structure: the message body length + message body
	            byte[] data = new byte[4 + msg.Length];
	            IntToBytes((int)type << 16 | msg.Length & 0xFFFF) .CopyTo(data, 0);
	            msg.CopyTo(data, 4);
	            if (stream.CanWrite == true) stream.BeginWrite(data, 0, data.Length, new AsyncCallback(SendCallback), stream);
	//			stream.Flush();

			} catch (Exception e) {
                Debug.LogError(e);
			}

        }

        private static void SendCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.
                NetworkStream handler = (NetworkStream) ar.AsyncState;

                // Complete sending the data to the remote device.
                handler.EndWrite(ar);

            } catch (Exception e) {
                Debug.LogError(e);
            }
        }

        private int i = 0;

        public void ReceiveMsg() {
            try {
                ReceiveMsg2();
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        public void ReceiveMsg2() {

            System.Action act;
            while(tasks.TryDequeue(out act) == true) {
                act.Invoke();
            }

			if (!Connected()) {
                return;
            }
            if (!stream.CanRead) {
                return;
            }
            if (((++i & 0x1F) == 0) && !IsConnectedDeep()) {
                active = false;
                Debug.Log(name + " Deep Detected: closed");
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
            Debug.Log(name + " C: close, active: " + active + ", thread: " + (thread != null));

            if (thread != null) CloseThread();

			this.active = false;
			if (this.client != null) {
				Log();
				var client = this.client;
				this.client = null;
                if (this.stream != null) {
                    this.stream = null;
                    client.GetStream().Close();
                }
                client.Close();
            }
        }

		/**
		 * Temporarily, debug purposes
		 */
		public void Log() {
			//var client = this.client;
            //Debug.Log(name + " C: " + client.GetHashCode() + "(" + client.Connected + ")"
            //    + " S: " + stream.GetHashCode() + "(" + stream.CanRead + "/" + stream.CanWrite + ")"
            //    + " So: " + client.Client.GetHashCode() + "(" + client.Client.Connected + ")");
		}

        public delegate void OnRevMsg(short type, byte[] msg);

        public OnRevMsg onRecMsg;
    }

	/**
	 * Temporarily, debug purposes
	 */
	public class TcpClientDebug : TcpClient {
		private string name = "net";

		public TcpClientDebug(string name) {
			this.name = name;
			initialize2();
		}

		protected override void Dispose(bool disposing) {
//			Debug.Log(name + " TcpDisposed1");
			base.Dispose(disposing);
			Debug.Log(name + " TcpDisposed2");
		}
		private void initialize2() {
            if (Client != null) {
                Client.Close();
                Client = null;
            }
			this.Client = new SocketDebug(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp, name);
		}
	}

	public class SocketDebug : Socket {
		private string name = "net";

		public SocketDebug(System.Net.Sockets.AddressFamily family, System.Net.Sockets.SocketType type, System.Net.Sockets.ProtocolType proto, string name) : base(family, type, proto) {
			this.name = name;
		}

		protected override void Dispose(bool disposing) {
//			Debug.Log(name + " SoDisposed1");
			base.Dispose(disposing);
			Debug.Log(name + " SoDisposed2: " + new StackTrace().ToString());
		}
	}
}