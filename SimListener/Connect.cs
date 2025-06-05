using Microsoft.FlightSimulator.SimConnect;

using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Diagnostics;



namespace SimListener
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct ResultStructure
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string sValue;
    };

    public class Connect
    {
        #region Public
        public bool IsConnected => m_oSimConnect is not null;
        #endregion

        #region Private
        private const int WM_USER_SIMCONNECT = 0x0402;
        private SimConnect? m_oSimConnect;
        private readonly ObservableCollection<SimListener> lSimvarRequests;
        private string AircaftLoaded = "Unknown";
        private uint m_iCurrentDefinition = 0;
        private uint m_iCurrentRequest = 0;
        #endregion Private

        #region Private Methods
        private ErrorCodes InternalAddRequest(string _sNewSimvarRequest, string _sNewUnitRequest, bool _bIsString)
        {

            if (!ValidateRequest(_sNewSimvarRequest))
            {
                Debug.WriteLine($"Request is not valid -> {_sNewSimvarRequest}");
                return ErrorCodes.INVALID_DATA_REQUEST;
            }

            Debug.WriteLine($"Request is valid -> {_sNewSimvarRequest}");

            SimListener oSimvarRequest = new()
            {
                eDef = (Definition)m_iCurrentDefinition,
                eRequest = (Request)m_iCurrentRequest,
                Parameter = _sNewSimvarRequest,
                bIsString = _bIsString,
                Measure = _sNewUnitRequest
            };

            if (lSimvarRequests.Contains<SimListener>(oSimvarRequest))
            {
                return ErrorCodes.OK;
            }

            oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
            oSimvarRequest.bStillPending = oSimvarRequest.bPending;

            Console.WriteLine($"Adding Request: -> {oSimvarRequest.Parameter}");
            lSimvarRequests?.Add(oSimvarRequest);

            ++m_iCurrentDefinition;
            ++m_iCurrentRequest;

            return ErrorCodes.OK;
        }
        private void ReceiveSimConnectMessage()
        {
            try
            {
                m_oSimConnect?.ReceiveMessage();
            }
            catch
            {
                m_oSimConnect = null;
            }

        }
        private bool RegisterToSimConnect(SimListener _oSimvarRequest)
        {
            if (m_oSimConnect != null)
            {
                if (_oSimvarRequest.bIsString)
                {
                    m_oSimConnect.AddToDataDefinition(_oSimvarRequest.eDef, _oSimvarRequest.Parameter, "", SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    m_oSimConnect.RegisterDataDefineStruct<ResultStructure>(_oSimvarRequest.eDef);
                }
                else
                {
                    m_oSimConnect.AddToDataDefinition(_oSimvarRequest.eDef, _oSimvarRequest.Parameter, _oSimvarRequest.Measure, SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    m_oSimConnect.RegisterDataDefineStruct<double>(_oSimvarRequest.eDef);
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            if (sender is null)
            {
                throw new ArgumentNullException(nameof(sender));
            }

            _ = InternalAddRequest("PLANE LATITUDE", "degrees", false);
            _ = InternalAddRequest("PLANE LONGITUDE", "degrees", false);
            _ = InternalAddRequest("AIRSPEED TRUE", "knots", false);
            _ = InternalAddRequest("PLANE ALTITUDE", "feet", false);
            _ = InternalAddRequest("PLANE HEADING DEGREES TRUE", "degrees", false);


            // Register pending requests
            if (lSimvarRequests != null)
            {
                foreach (SimListener oSimvarRequest in lSimvarRequests)
                {
                    if (oSimvarRequest.bPending)
                    {
                        oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
                        oSimvarRequest.bStillPending = oSimvarRequest.bPending;
                    }
                }
            }
        }
        private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            m_oSimConnect = null;
        }
        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            m_oSimConnect = null;
        }
        private void SimConnect_OnRecvEvent(SimConnect sender, SIMCONNECT_RECV_SYSTEM_STATE data)
        {
            Debug.WriteLine($"Request recieved -> {data.szString}");

            if ((Requests)data.dwRequestID == Requests.AIRCRAFT_LOADED)
            {
                AircaftLoaded = data.szString;
            }
        }
        private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {

            uint iRequest = data.dwRequestID;
            uint iObject = data.dwObjectID;
            
            Debug.WriteLine($"Data recieved -> {iObject}");

            if (iObject > 1)
            {
                if (lSimvarRequests != null)
                {
                    foreach (SimListener oSimvarRequest in lSimvarRequests)
                    {
                        if (iRequest == (uint)oSimvarRequest.eRequest)
                        {
                            if (oSimvarRequest.bIsString)
                            {
                                ResultStructure result = (ResultStructure)data.dwData[0];

                                oSimvarRequest.Value = result.sValue;
                            }
                            else
                            {
                                oSimvarRequest.Value = ((double)data.dwData[0]).ToString();

                            }
                            Debug.WriteLine($"Data recieved -> {oSimvarRequest.Parameter} = {oSimvarRequest.Value}");

                            oSimvarRequest.bPending = false;
                            oSimvarRequest.bStillPending = false;
                        }
                    }
                }
            }
        }
        #endregion

        #region Public Methods
        public void ConnectToSim()
        {
            if (m_oSimConnect is null)
            {
                try
                {
                    /// The constructor is similar to SimConnect_Open in the native API
                    m_oSimConnect = new SimConnect("SimListener", (IntPtr)null, WM_USER_SIMCONNECT, null, 0); 

                    if (m_oSimConnect is not null)
                    {
                        Debug.WriteLine("SimConnect connected successfully.");

                        /// Listen to connect and quit msgs
                        m_oSimConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
                        m_oSimConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);
                        m_oSimConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);
                        m_oSimConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);
                        m_oSimConnect.OnRecvSystemState += new SimConnect.RecvSystemStateEventHandler(SimConnect_OnRecvEvent);

                        m_oSimConnect.SubscribeToSystemEvent(Event.RECUR_1SEC, "1sec");
                    }

                }
                catch (COMException)
                {
                    // TODO: Handle the exception properly, e.g., log it or notify the user
                }
            }
        }
        public void Disconnect()
        {
            m_oSimConnect?.Dispose();

            // Set all requests as pending
            if (lSimvarRequests != null)
            {
                foreach (SimListener oSimvarRequest in lSimvarRequests)
                {
                    oSimvarRequest.bPending = true;
                    oSimvarRequest.bStillPending = true;
                }
            }
        }
        public Dictionary<string, string> AircraftData()
        {
            ReceiveSimConnectMessage();

            Dictionary<string, string> ReturnValue = new()
            {
                { "Connected"     , IsConnected.ToString() },
                { "AircaftLoaded" , AircaftLoaded }
            };

            m_oSimConnect?.RequestSystemState(Requests.AIRCRAFT_LOADED, "AircraftLoaded");

            if (lSimvarRequests != null)
            {
                foreach (SimListener oSimvarRequest in lSimvarRequests)
                {
                    if (oSimvarRequest.Parameter is not null)
                    {
                        if (!ReturnValue.ContainsKey(oSimvarRequest.Parameter))
                        {
                            ReturnValue.Add(oSimvarRequest.Parameter, "");
                            oSimvarRequest.bPending = false;
                        }

                        if (oSimvarRequest.Value is not null)
                        {
                            ReturnValue[oSimvarRequest.Parameter] = oSimvarRequest.Value;
                        }
                    }

                    if (!oSimvarRequest.bPending)
                    {
                        m_oSimConnect?.RequestDataOnSimObjectType(oSimvarRequest.eRequest, oSimvarRequest.eDef, 0, SIMCONNECT_SIMOBJECT_TYPE.USER);

                        oSimvarRequest.bPending = true;
                    }
                    else
                    {
                        oSimvarRequest.bStillPending = true;
                    }

                }
            }

            return ReturnValue;
        }
        public ErrorCodes AddRequest(string _sNewSimvarRequest)
        {
            return InternalAddRequest(_sNewSimvarRequest, "", true);
        }
        public string AddRequests(List<string> Outputs)
        {
            if (Outputs is not null)
            {
                if (Outputs.Any())
                {
                    foreach (string output in Outputs)
                    {
                        var rval = AddRequest(output);
                        if (rval != ErrorCodes.OK)
                        {
                            return output;
                        }
                    }
                }
            }
            return "OK";
        }
        private static bool ValidateRequest(string request)
        {
            return request != null && SimVars.Names.Contains(request);
        }
        #endregion

        #region Constructor
        /// <summary>Initializes a new instance of the <see cref="T:SimListener.Connect" /> class.</summary>
        public Connect()
        {
            lSimvarRequests = new ObservableCollection<SimListener>();
        }

        #endregion
    }
}