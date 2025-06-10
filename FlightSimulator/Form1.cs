using FlightSimulator.Properties;
using SimListener;
using System.Diagnostics;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FlightSimulator
{

    public partial class Form1 : Form
    {
        private List<string> DATA = new List<string>
                                    { "PLANE LATITUDE", "PLANE LONGITUDE", "PLANE ALTITUDE" };

        private SimListener.Connect simConnection;
        private Redis.Redis redisConnection;

        public Form1()
        {
            InitializeComponent();
            redisConnection = new Redis.Redis( Settings.Default.redis_server ,
                                               Settings.Default.redis_port ); // Initialize Redis connection
            simConnection = new SimListener.Connect( 5000 ); // Initialize SimEvents connection

            simConnection.OnConnect += c_SimConnected;       // Subscribe to SimConnected event
            simConnection.OnDisconnect += c_SimDisconnected; // Subscribe to SimDisconnected event  
        }

        void c_SimConnected(object sender, EventArgs e)
        {
            connectionStatus.Image = Properties.Resources.plane_green;
        }
        void c_SimDisconnected(object sender, EventArgs e)
        {
            connectionStatus.Image = Properties.Resources.plane_red;
        }
    }
} // End of namespace FlightSimulator
