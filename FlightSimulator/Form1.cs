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

        private SimListener.Connect connection;
        private Redis.Redis redisConnection;

        public Form1()
        {
            InitializeComponent();
            redisConnection = new Redis.Redis( Settings.Default.redis_server ,
                                               Settings.Default.redis_port ); // Initialize Redis connection
        }

        private void Populate()
        {
            if (connection.IsConnected)
            {
              
                foreach (var req in connection.AircraftData())
                {
                    if (req.Key == null || req.Value == null)
                    {
                        Debug.WriteLine("Received null key or value from AircraftData.");
                        continue;
                    }
                    Debug.WriteLine($"Key: {req.Key}, Value: {req.Value}");
                    redisConnection.write(req.Key, req.Value); // Write to Redis
                    bool add = true;   
                    foreach ( ListViewItem item in listView1.Items )
                    {
                        if (item.Text == req.Key)
                        {
                            item.SubItems.Clear();
                            item.Text = req.Key;
                            item.SubItems.Add(req.Value);
                            add= false;
                            break;
                        }
                    }
                    if (add)
                    {
                        ListViewItem newitem = new ListViewItem(req.Key, 0);
                        newitem.Text = req.Key;   
                        newitem.SubItems.Add(req.Value);
                        listView1.Items.Add(newitem);
                    }
                }
            }
        }
        private void Connect_Click(object sender, EventArgs e)
        {
            try
            {
                connection = new();
                connection.ConnectToSim();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error connecting to simulator: " + ex.Message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (connection.IsConnected)
            {
                connectionStatus.Image = Properties.Resources.plane_green;

                KeyValuePair<string, string> Data = new KeyValuePair<string, string>("PLANE LATITUDE", "");
                List<string> Test = new() { Data.Key };
                string answer = connection.AddRequests(Test);

                if (answer != "OK")
                {
                    MessageBox.Show("Error adding requests: " + answer, "Request Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                timer1.Interval = Settings.Default.clock_interval; // Set the timer interval from settings
                timer1.Enabled = true;
            }
            else
            {
                connectionStatus.Image = Properties.Resources.plane_red;
                timer1.Enabled = false;
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (connection == null || !connection.IsConnected)
            {
                connectionStatus.Image = Properties.Resources.plane_red;
                timer1.Enabled = false;
                return;
            }
            Populate();
        }
    }
} // End of namespace FlightSimulator
