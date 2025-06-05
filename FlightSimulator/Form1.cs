using SimListener;
using System.Numerics;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace FlightSimulator
{

    public partial class Form1 : Form
    {
        private List<string> DATA = new List<string>
                                    { "PLANE LATITUDE", "PLANE LONGITUDE", "PLANE ALTITUDE" };

        private SimListener.Connect connection;
        public Form1()
        {
            InitializeComponent();
        }

        private void Populate()
        {
            if (connection.IsConnected)
            {
              
                foreach (var req in connection.AircraftData())
                {
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
                        listView1.Items.AddRange(new ListViewItem[] { newitem });
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
                connectionStatus.Text = "Connected to Simulator";
                connectionStatus.BackColor = Color.LightGray;
                connectionStatus.ForeColor = Color.Green;

                KeyValuePair<string, string> Data = new KeyValuePair<string, string>("PLANE LATITUDE", "");
                List<string> Test = new() { Data.Key };
                string answer = connection.AddRequests(Test);

                if (answer != "OK")
                {
                    MessageBox.Show("Error adding requests: " + answer, "Request Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                timer1.Interval = 5000; // Update every 5 seconds
                timer1.Enabled = true;
                connectionStatus.Text = $"Connected to Simulator (Requests {answer})";
            }
            else
            {
                connectionStatus.Text = "Not Connected";
                connectionStatus.BackColor = Color.LightGray;
                connectionStatus.ForeColor = Color.Black;
                timer1.Enabled = false;
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            if (connection == null || !connection.IsConnected)
            {
                connectionStatus.Text = "Not Connected";
                connectionStatus.BackColor = Color.LightGray;
                connectionStatus.ForeColor = Color.Black;
                timer1.Enabled = false;
                return;
            }
            Populate();
        }
    }
} // End of namespace FlightSimulator
