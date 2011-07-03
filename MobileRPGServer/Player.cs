using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace MobileRPGServer {
	class Player {
		public Socket Sock { get; set; }
		public byte[] Input { get; set; }

		public int Id { get; set; }
		public String Name { get; set; }
		public int ModelID { get; set; }
		public int Level { get; set; }
		public float PosX { get; set; }
		public float PosY { get; set; }
		public float VelocityX { get; set; }
		public float VelocityY { get; set; }
		public int Direction { get; set; }

		private Random r = new Random();

		public Player(Socket socket) {
			this.Sock = socket;
			this.Input = new byte[1024];

			this.Id = r.Next(0, 1000);
			this.PosX = r.Next(0, 600);
			this.PosY = r.Next(0, 400);			
		}

		public void UpdatePhysics(float secondsPassed) {
			this.PosX += this.VelocityX * secondsPassed;
			this.PosY += this.VelocityY * secondsPassed;
		}
	}
}
