using System.Diagnostics;

namespace SimEvents
{
    public class SimEvents : SimListener.Connect
    {
        private System.Timers.Timer connectTimer;
       
        public SimEvents()
        {
            connectTimer = new System.Timers.Timer(5000); // Set the timer interval to 5 seconds
            connectTimer.Elapsed += OnTimedEvent;
            connectTimer.AutoReset = true; // Repeat the event
            connectTimer.Enabled = true; // Start the timer
        }

        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            Debug.WriteLine("The Elapsed event was raised at {0:HH:mm:ss.fff}", e.SignalTime);
            try
            {   
                this.ConnectToSim(); // Attempt to connect to the simulator
            }
            catch (SimListener.UnableToConnect ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
