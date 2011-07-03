using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;


namespace MobileRPGServer {
	class PhysicsEngine {
		private Dictionary<int, Player> players;
		private float dt = 0.1F; //in seconds

		public PhysicsEngine(Dictionary<int, Player> players) {
			this.players = players;
		}

		public void initUpdatePhysics() {
			Timer timer = new Timer(dt * 1000);
			timer.Elapsed += new ElapsedEventHandler(UpdatePhysics);
			timer.Enabled = true;
		}

		private void UpdatePhysics(object source, ElapsedEventArgs e) {
			try {
				foreach (Player p in players.Values) {
					p.UpdatePhysics(dt);
				}
			} catch (Exception error) {
				Console.WriteLine("Error in Physicsupdate loop: " + error);
			}
		}
	}
}
