using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace MobileRPGServer {
	class Server {
		private TcpListener server;
		private Dictionary<int, Player> players;
		private static int port = 2001;
		private static double version = 0.11;
		public enum IOType { Login, Move, AddPlayer, RemovePlayer };

		static void Main(string[] args) {
			Server server = new Server();
			server.Run();
		}

		public void Run() {
			players = new Dictionary<int, Player>();
			string command = "";
			server = new TcpListener(IPAddress.Any, port);
			server.Start();
			Console.WriteLine("Mobile RPG Server Version " + version + " started. (Port: " + port + ")");
			server.BeginAcceptSocket(OnConnect, null);
			PhysicsEngine physicsEngine = new PhysicsEngine(this.players);
			Thread physicsThread = new Thread(new ThreadStart(physicsEngine.initUpdatePhysics));
			physicsThread.Start();
			while (command != "quit") 
				command = Console.ReadLine();
		}

		/// <summary>
		/// Logs user into game
		/// </summary>
		private void OnConnect(IAsyncResult asyn) {
			Player p = new Player(server.EndAcceptSocket(asyn));
			players.Add(p.Id, p);
			p.Sock.BeginReceive(p.Input, 0, p.Input.Length, SocketFlags.None, OnRecieveInput, p); ;
			server.BeginAcceptSocket(OnConnect, null);
			AuthenticateLogin(p);
			Console.WriteLine("New connection established. " + p.Sock.RemoteEndPoint + " (id: " + p.Id + ")");
		}

		private void LogoutUser(IAsyncResult async) {

			Player player = (Player)async.AsyncState;
			player.Sock.Shutdown(SocketShutdown.Both);
			player.Sock.Close();
			players.Remove(player.Id);

			byte[] output = System.Text.Encoding.UTF8.GetBytes(formatOutput((int)IOType.RemovePlayer, player.Id));
			foreach (Player p in players.Values) {
				try {
					p.Sock.BeginSend(output, 0, output.Length, SocketFlags.None, OnDataSent, p.Sock);
				} catch (Exception e) {	}
			}
		}

		/// <summary>
		/// Handles input from player
		/// </summary>
		private void OnRecieveInput(IAsyncResult async) {
			try {
				Player p = (Player)async.AsyncState;
				int bytesReceived = p.Sock.EndReceive(async);
				string input = System.Text.Encoding.UTF8.GetString(p.Input, 0, bytesReceived);

				if (input.Length == 0) {
					LogoutUser(async);
					return;
				}

				List<String> response = processInput(input);

				var inputType = int.Parse(response[0]);
				switch (inputType) {
					case (int)IOType.Move: CommandMove(p, float.Parse(response[1]), float.Parse(response[2]));
						Console.WriteLine("Move received: (" + float.Parse(response[1]) + ") (" + float.Parse(response[2]) + ")");
						break;

				}

				p.Sock.BeginReceive(p.Input, 0, p.Input.Length, SocketFlags.None, OnRecieveInput, p);
			} catch (SocketException se) {
				LogoutUser(async);
			} catch (Exception e) {
				Console.WriteLine(e);
			}
		}
		private void AuthenticateLogin(Player player) {
			//send player data
			byte[] output = System.Text.Encoding.UTF8.GetBytes(formatOutput((int)IOType.Login, player.Id, player.PosX, player.PosY, player.ModelID));
			player.Sock.BeginSend(output, 0, output.Length, SocketFlags.None, OnDataSent, player.Sock);
			output = System.Text.Encoding.UTF8.GetBytes(formatOutput((int)IOType.AddPlayer, player.Id, player.PosX, player.PosY, player.ModelID));
			foreach (Player p in players.Values) {
				if (p.Id != player.Id) {
					//log player in for other players
					p.Sock.BeginSend(output, 0, output.Length, SocketFlags.None, OnDataSent, p.Sock);
					//display other players for this player
					byte[] otherPlayerData = System.Text.Encoding.UTF8.GetBytes(formatOutput((int)IOType.AddPlayer, p.Id, p.PosX, p.PosY, p.ModelID));
					player.Sock.BeginSend(otherPlayerData, 0, otherPlayerData.Length, SocketFlags.None, OnDataSent, player.Sock);
				}
			}
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="player"></param>
		/// <param name="data"></param>
		private void CommandMove(Player player, float xVelocity, float yVelocity) {
			player.VelocityX = xVelocity;
			player.VelocityY = yVelocity;
			byte[] output = System.Text.Encoding.UTF8.GetBytes(formatOutput((int)IOType.Move, player.Id, xVelocity, yVelocity));
			foreach (Player p in players.Values) {
				if (p.Id != player.Id)
					p.Sock.BeginSend(output, 0, output.Length, SocketFlags.None, OnDataSent, p.Sock);
			}
		}

		/// <summary>
		/// ends the send after message has been sent
		/// </summary>
		/// <param name="async"></param>
		private void OnDataSent(IAsyncResult async) {
			((Socket)async.AsyncState).EndSend(async);
		}

		private string formatOutput(params Object[] args) {
			string output = "";
			foreach (Object o in args)
				output += "<" + o + ">";
			output += "\n";
			return output;
		}

		private List<string> processInput(string input) {
			input = input.Replace("\r", "").Replace("\n", "");
			string pattern = "<([^>]*)>";
			Regex rgx = new Regex(pattern);
			MatchCollection matches = rgx.Matches(input);
			List<string> response = new List<string>();
			foreach (Match m in matches) {
				response.Add(m.Groups[1].Value);
			}
			return response;
		}
	}
}
