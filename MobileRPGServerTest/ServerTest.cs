using MobileRPGServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;

namespace MobileRPGServerTest
{
    
    
    /// <summary>
    ///This is a test class for ServerTest and is intended
    ///to contain all ServerTest Unit Tests
    ///</summary>
	[TestClass()]
	public class ServerTest {
		private string ip = "24.10.219.59";
		private int port = 2001;

		private class player {
			public int id;
			public TcpClient client;
			public StreamWriter sender;

			public player(int id, TcpClient client, StreamWriter sender) {
				this.id = id;
				this.client = client;
				this.sender = sender;
			}
		}

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext {
			get {
				return testContextInstance;
			}
			set {
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		/// <summary>
		///A test for Run
		///</summary>
		[TestMethod()]
		public void RunTest() {
			//start server
			Server target = new Server();
			target.Run();

			int numOfUsers = 50;
			List<player> players = new List<player>();

			//create and login users
			for (int i = 0; i < numOfUsers; i++) {
				try {
					TcpClient client = new TcpClient();
					client.Connect(ip, port);
					StreamWriter sender = new StreamWriter(client.GetStream());
					players.Add(new player(i, client, sender));
				} catch {
					Assert.Fail("Player " + i + " did not connect.");
				}
			}

			//move users around
			Random r = new Random(665);
			foreach (player p in players) {

				string output = "<1><" + r.Next(0, 300) + "><0>";
				p.sender.WriteLine(output);
				p.sender.Flush();
			}
			foreach (player p in players) {
				string output = "<1><0><0>";
				p.sender.WriteLine(output);
				p.sender.Flush();
			}

			//disconnect users
			foreach (player p in players) {
				p.client.GetStream().Close();
				p.client.Close();
			}
			Assert.IsTrue(true);
		}
	}
}
