﻿using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Threading;

using Microsoft.FlightSimulator.SimConnect;
using System.Collections.Generic;
using System.Windows.Data;
using System.Linq;
using System.Collections;

namespace Simvars
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    struct Struct1
    {
        // this is how you declare a fixed size string
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public String sValue;

        // other definitions can be added to this struct
        // ...
    };
    public enum DEFINITION
    {
        Dummy = 0
    };

    public enum REQUEST
    {
        Dummy = 0,
        Struct1
    };

    public enum COPY_ITEM
    {
        Name = 0,
        Value,
        Unit
    }

    public class ObjectID:IComparer
    {
        public uint ID { get; set; }         // Actually UInt16
        public uint GlobalID { get; set; }   // Actually UInt32

        public ObjectID(uint _ID, uint _GlobalID)
        {
            ID = _ID;
            GlobalID = _GlobalID;
        }

        public int Compare(object x, object y)
        {
            return (x as ObjectID).GlobalID.CompareTo((y as ObjectID).GlobalID);
        }

        public override string ToString() 
        {
            return GlobalID.ToString("X");
        }
    }

    public class SimvarRequest : ObservableObject
    {
        public DEFINITION eDef = DEFINITION.Dummy;
        public REQUEST eRequest = REQUEST.Dummy;

        public string sName { get; set; }
        public bool bIsString { get; set; }
        public double dValue
        {
            get { return m_dValue; }
            set { this.SetProperty(ref m_dValue, value); }
        }
        private double m_dValue = 0.0;
        public string sValue
        {
            get { return m_sValue; }
            set { this.SetProperty(ref m_sValue, value); }
        }
        private string m_sValue = null;

        public string sUnits { get; set; }

        public bool bPending = true;
        public bool bStillPending
        {
            get { return m_bStillPending; }
            set { this.SetProperty(ref m_bStillPending, value); }
        }
        private bool m_bStillPending = false;

    };

    public class SimvarsViewModel : BaseViewModel, IBaseSimConnectWrapper
    {
        #region IBaseSimConnectWrapper implementation

        /// User-defined win32 event
        public const int WM_USER_SIMCONNECT = 0x0402;

        /// Window handle
        private IntPtr m_hWnd = new IntPtr(0);

        /// SimConnect object
        private SimConnect m_oSimConnect = null;

        public bool bConnected
        {
            get { return m_bConnected; }
            private set { this.SetProperty(ref m_bConnected, value); }
        }
        private bool m_bConnected = false;

        private uint m_iCurrentDefinition = 0;
        private uint m_iCurrentRequest = 0;

        public int GetUserSimConnectWinEvent()
        {
            return WM_USER_SIMCONNECT;
        }

        public void ReceiveSimConnectMessage()
        {
            m_oSimConnect?.ReceiveMessage();
        }

        public void SetWindowHandle(IntPtr _hWnd)
        {
            m_hWnd = _hWnd;
        }

        public void Disconnect()
        {
            Console.WriteLine("Disconnect");

            m_oTimer.Stop();
            bOddTick = false;

            if (m_oSimConnect != null)
            {
                /// Dispose serves the same purpose as SimConnect_Close()
                m_oSimConnect.Dispose();
                m_oSimConnect = null;
            }

            sConnectButtonLabel = "Connect";
            bConnected = false;

            // Set all requests as pending
            foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            {
                oSimvarRequest.bPending = true;
                oSimvarRequest.bStillPending = true;
            }
        }
        #endregion

        #region UI bindings

        public string sConnectButtonLabel
        {
            get { return m_sConnectButtonLabel; }
            private set { this.SetProperty(ref m_sConnectButtonLabel, value); }
        }
        private string m_sConnectButtonLabel = "Connect";

        public bool bObjectIDSelectionEnabled
        {
            get { return m_bObjectIDSelectionEnabled; }
            set { this.SetProperty(ref m_bObjectIDSelectionEnabled, value); }
        }
        private bool m_bObjectIDSelectionEnabled = false;
        public SIMCONNECT_SIMOBJECT_TYPE eSimObjectType
        {
            get { return m_eSimObjectType; }
            set
            {
                ObjectID currentObjectID = iObjectIdRequest;
                this.SetProperty(ref m_eSimObjectType, value);
                //cbb_ObjectIds
                lObjectIDs = alltypevalue[m_eSimObjectType];
                ObjectView = (ListCollectionView)CollectionViewSource.GetDefaultView(lObjectIDs);
                ObjectView.SortDescriptions.Clear();
                ObjectView.SortDescriptions.Add(new System.ComponentModel.SortDescription("ID", System.ComponentModel.ListSortDirection.Ascending));
                ObjectView.Refresh();
                this.OnPropertyChanged("ObjectView");
                bObjectIDSelectionEnabled = (m_eSimObjectType != SIMCONNECT_SIMOBJECT_TYPE.USER_AIRCRAFT);
                ObjectID newObjectID = null;
                if (currentObjectID != null)
                    newObjectID = lObjectIDs.FirstOrDefault(o => o.Compare(o, currentObjectID) == 0);
                if (newObjectID != null)
                { 
                    iObjectIdRequest = newObjectID;
                }
                else
                {
                    if (lObjectIDs.Count > 0)
                        iObjectIdRequest = alltypevalue[m_eSimObjectType][0];
                    else
                        iObjectIdRequest = null;
                }
                ClearRequestsPendingState();
            }
        }

        private SIMCONNECT_SIMOBJECT_TYPE m_eSimObjectType = SIMCONNECT_SIMOBJECT_TYPE.USER;
        public ObservableCollection<ObjectID> lObjectIDs { get; private set; }
        public ListCollectionView ObjectView { get; set; }

        Dictionary<SIMCONNECT_SIMOBJECT_TYPE, ObservableCollection<ObjectID>> alltypevalue;


        public ObjectID iObjectIdRequest
        {
            get { return m_iObjectIdRequest; }
            set
            {
                this.SetProperty(ref m_iObjectIdRequest, value);
                ClearRequestsPendingState();
            }
        }
        private ObjectID m_iObjectIdRequest;

        public string[] aSimvarNames
        {
            get { return SimUtils.SimVars.Names; }
            set { this.SetProperty(ref m_aSimvarNames, value); }
        }
        private string[] m_aSimvarNames = null;

        public string[] aSimvarNamesFiltered
        {
            get
            {
                if (m_aSimvarNamesFiltered == null)
                {
                    m_aSimvarNamesFiltered = aSimvarNames;
                }
                return m_aSimvarNamesFiltered;
            }
            set { this.SetProperty(ref m_aSimvarNamesFiltered, value); }
        }
        private string[] m_aSimvarNamesFiltered = null;

        public string sSimvarRequest
        {
            get { return m_sSimvarRequest; }
            set { this.SetProperty(ref m_sSimvarRequest, value); }
        }
        private string m_sSimvarRequest = null;

        public string[] aUnitNames
        {
            get
            {
                if (m_aUnitNames == null)
                {
                    m_aUnitNames = SimUtils.Units.Names;
                    Array.Sort(m_aUnitNames);
                }
                return m_aUnitNames;
            }
            private set { }
        }
        private string[] m_aUnitNames;
        public string[] aUnitNamesFiltered
        {
            get
            {
                if (m_aUnitNamesFiltered == null)
                {
                    m_aUnitNamesFiltered = aUnitNames;
                }
                return m_aUnitNamesFiltered;
            }
            set { this.SetProperty(ref m_aUnitNamesFiltered, value); }
        }
        private string[] m_aUnitNamesFiltered = null;


        public string sUnitRequest
        {
            get { return m_sUnitRequest; }
            set { this.SetProperty(ref m_sUnitRequest, value); }
        }
        private string m_sUnitRequest = null;

        public string sSetValue
        {
            get { return m_sSetValue; }
            set { this.SetProperty(ref m_sSetValue, value); }
        }
        private string m_sSetValue = null;

        public ObservableCollection<SimvarRequest> lSimvarRequests { get; private set; }
        public SimvarRequest oSelectedSimvarRequest
        {
            get { return m_oSelectedSimvarRequest; }
            set { this.SetProperty(ref m_oSelectedSimvarRequest, value); }
        }
        private SimvarRequest m_oSelectedSimvarRequest = null;

        public uint[] aIndices
        {
            get { return m_aIndices; }
            private set { }
        }
        private readonly uint[] m_aIndices = new uint[100] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9,
                                                            10, 11, 12, 13, 14, 15, 16, 17, 18, 19,
                                                            20, 21, 22, 23, 24, 25, 26, 27, 28, 29,
                                                            30, 31, 32, 33, 34, 35, 36, 37, 38, 39,
                                                            40, 41, 42, 43, 44, 45, 46, 47, 48, 49,
                                                            50, 51, 52, 53, 54, 55, 56, 57, 58, 59,
                                                            60, 61, 62, 63, 64, 65, 66, 67, 68, 69,
                                                            70, 71, 72, 73, 74, 75, 76, 77, 78, 79,
                                                            80, 81, 82, 83, 84, 85, 86, 87, 88, 89,
                                                            90, 91, 92, 93, 94, 95, 96, 97, 98, 99 };
        public uint iIndexRequest
        {
            get { return m_iIndexRequest; }
            set { this.SetProperty(ref m_iIndexRequest, value); }
        }
        private uint m_iIndexRequest = 0;

        public bool bSaveValues
        {
            get { return m_bSaveValues; }
            set { this.SetProperty(ref m_bSaveValues, value); }
        }
        private bool m_bSaveValues = true;

        public bool bFSXcompatible
        {
            get { return m_bFSXcompatible; }
            set { this.SetProperty(ref m_bFSXcompatible, value); }
        }
        private bool m_bFSXcompatible = false;
        public bool bIsString
        {
            get { return m_bIsString; }
            set { this.SetProperty(ref m_bIsString, value); }
        }
        private bool m_bIsString = false;

        public bool bShowAllUnits
        {
            get { return m_bShowAllUnits; }
            set { this.SetProperty(ref m_bShowAllUnits, value); }
        }
        private bool m_bShowAllUnits = false;

        public bool bOddTick
        {
            get { return m_bOddTick; }
            set { this.SetProperty(ref m_bOddTick, value); }
        }
        private bool m_bOddTick = false;

        public ObservableCollection<string> lErrorMessages { get; private set; }


        public BaseCommand cmdToggleConnect { get; private set; }
        public BaseCommand cmdAddRequest { get; private set; }
        public BaseCommand cmdRemoveSelectedRequest { get; private set; }
        public BaseCommand cmdRemoveAllRequests { get; private set; }
        public BaseCommand cmdCopyNameSelectedRequest { get; private set; }
        public BaseCommand cmdCopyValueSelectedRequest { get; private set; }
        public BaseCommand cmdCopyUnitSelectedRequest { get; private set; }
        public BaseCommand cmdTrySetValue { get; private set; }
        public BaseCommand cmdLoadFiles { get; private set; }
        public BaseCommand cmdSaveFile { get; private set; }
        public BaseCommand cmdSaveFileWithValues { get; private set; }

        #endregion

        #region Real time

        private DispatcherTimer m_oTimer = new DispatcherTimer();

        #endregion

        public SimvarsViewModel()
        {

            alltypevalue = new Dictionary<SIMCONNECT_SIMOBJECT_TYPE, ObservableCollection<ObjectID>>();
            alltypevalue[SIMCONNECT_SIMOBJECT_TYPE.USER_AIRCRAFT] = new ObservableCollection<ObjectID>();
            alltypevalue[SIMCONNECT_SIMOBJECT_TYPE.USER_AIRCRAFT].Add(new ObjectID(0, 0)); // Magic number for the User aircraft
            alltypevalue[SIMCONNECT_SIMOBJECT_TYPE.ALL] = new ObservableCollection<ObjectID>();
            alltypevalue[SIMCONNECT_SIMOBJECT_TYPE.AIRCRAFT] = new ObservableCollection<ObjectID>();
            alltypevalue[SIMCONNECT_SIMOBJECT_TYPE.HELICOPTER] = new ObservableCollection<ObjectID>();
            alltypevalue[SIMCONNECT_SIMOBJECT_TYPE.BOAT] = new ObservableCollection<ObjectID>();
            alltypevalue[SIMCONNECT_SIMOBJECT_TYPE.GROUND] = new ObservableCollection<ObjectID>();
            alltypevalue[SIMCONNECT_SIMOBJECT_TYPE.HOT_AIR_BALLOON] = new ObservableCollection<ObjectID>();
            alltypevalue[SIMCONNECT_SIMOBJECT_TYPE.ANIMAL] = new ObservableCollection<ObjectID>();
            alltypevalue[SIMCONNECT_SIMOBJECT_TYPE.USER_AVATAR] = new ObservableCollection<ObjectID>();
            alltypevalue[SIMCONNECT_SIMOBJECT_TYPE.USER_CURRENT] = new ObservableCollection<ObjectID>();

            lObjectIDs = new ObservableCollection<ObjectID>();

            lSimvarRequests = new ObservableCollection<SimvarRequest>();

            lErrorMessages = new ObservableCollection<string>();

            cmdToggleConnect = new BaseCommand((p) => { ToggleConnect(); });
            cmdAddRequest = new BaseCommand((p) => { AddRequest((m_iIndexRequest == 0) ? m_sSimvarRequest : (m_sSimvarRequest + ":" + m_iIndexRequest), sUnitRequest, bIsString); });
            cmdRemoveSelectedRequest = new BaseCommand((p) => { RemoveSelectedRequest(); });
            cmdRemoveAllRequests = new BaseCommand((p) => { RemoveAllRequest(); });
            cmdCopyNameSelectedRequest = new BaseCommand((p) => { CopySelectedRequest(COPY_ITEM.Name); });
            cmdCopyValueSelectedRequest = new BaseCommand((p) => { CopySelectedRequest(COPY_ITEM.Value); });
            cmdCopyUnitSelectedRequest = new BaseCommand((p) => { CopySelectedRequest(COPY_ITEM.Unit); });
            cmdTrySetValue = new BaseCommand((p) => { TrySetValue(); });
            cmdLoadFiles = new BaseCommand((p) => { LoadFiles(); });
            cmdSaveFile = new BaseCommand((p) => { SaveFile(false, false); });
            cmdSaveFileWithValues = new BaseCommand((p) => { SaveFile(false, true); });

            m_oTimer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            m_oTimer.Tick += new EventHandler(OnTick);

            eSimObjectType = SIMCONNECT_SIMOBJECT_TYPE.USER_AIRCRAFT;
        }

        private void Connect()
        {
            Console.WriteLine("Connect");

            try
            {
                /// The constructor is similar to SimConnect_Open in the native API
                m_oSimConnect = new SimConnect("Simconnect - Simvar test", m_hWnd, WM_USER_SIMCONNECT, null, bFSXcompatible ? (uint)1 : 0);

                /// Listen to connect and quit msgs
                m_oSimConnect.OnRecvOpen += new SimConnect.RecvOpenEventHandler(SimConnect_OnRecvOpen);
                m_oSimConnect.OnRecvQuit += new SimConnect.RecvQuitEventHandler(SimConnect_OnRecvQuit);

                /// Listen to exceptions
                m_oSimConnect.OnRecvException += new SimConnect.RecvExceptionEventHandler(SimConnect_OnRecvException);

                /// Catch a simobject data request
                m_oSimConnect.OnRecvSimobjectDataBytype += new SimConnect.RecvSimobjectDataBytypeEventHandler(SimConnect_OnRecvSimobjectDataBytype);
            }
            catch (COMException ex)
            {
                Console.WriteLine("Connection to KH failed: " + ex.Message);
            }
        }

        private void SimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine("SimConnect_OnRecvOpen");
            Console.WriteLine("Connected to KH");

            sConnectButtonLabel = "Disconnect";
            bConnected = true;

            // Register pending requests
            foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            {
                if (oSimvarRequest.bPending)
                {
                    oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
                    oSimvarRequest.bStillPending = oSimvarRequest.bPending;
                }
            }

            m_oTimer.Start();
            bOddTick = false;
        }

        /// The case where the user closes game
        private void SimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine("SimConnect_OnRecvQuit");
            Console.WriteLine("KH has exited");

            Disconnect();
        }

        private void SimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            SIMCONNECT_EXCEPTION eException = (SIMCONNECT_EXCEPTION)data.dwException;
            Console.WriteLine("SimConnect_OnRecvException: " + eException.ToString());

            lErrorMessages.Add("SimConnect : " + eException.ToString());
        }

        private void SimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            Console.WriteLine("SimConnect_OnRecvSimobjectDataBytype");

            uint iRequest = data.dwRequestID;
            uint iObject = data.dwObjectID;

            if (!alltypevalue[eSimObjectType].Any(o => o.GlobalID == iObject))
            {
                alltypevalue[eSimObjectType].Add(new ObjectID(iObject >> 14, iObject));
            }
            foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            {
                if (iRequest == (uint)oSimvarRequest.eRequest && (iObjectIdRequest?.GlobalID == 0 || iObjectIdRequest?.GlobalID == iObject))
                {
                    if (oSimvarRequest.bIsString)
                    {
                        Struct1 result = (Struct1)data.dwData[0];
                        oSimvarRequest.dValue = 0;
                        oSimvarRequest.sValue = result.sValue;
                    }
                    else
                    {
                        double dValue = (double)data.dwData[0];
                        oSimvarRequest.dValue = dValue;
                        oSimvarRequest.sValue = dValue.ToString("F9");

                    }

                    oSimvarRequest.bPending = false;
                    oSimvarRequest.bStillPending = false;
                }
            }
        }

        // May not be the best way to achive regular requests.
        // See SimConnect.RequestDataOnSimObject
        private void OnTick(object sender, EventArgs e)
        {
            Console.WriteLine("OnTick");

            bOddTick = !bOddTick;

            foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            {
                if (!oSimvarRequest.bPending)
                {
                    m_oSimConnect?.RequestDataOnSimObjectType(oSimvarRequest.eRequest, oSimvarRequest.eDef, 0, m_eSimObjectType);
                    oSimvarRequest.bPending = true;
                }
                else
                {
                    oSimvarRequest.bStillPending = true;
                }
            }
        }

        private void ToggleConnect()
        {
            if (m_oSimConnect == null)
            {
                try
                {
                    Connect();
                }
                catch (COMException ex)
                {
                    Console.WriteLine("Unable to connect to KH: " + ex.Message);
                }
            }
            else
            {
                Disconnect();
            }
        }

        private void ClearRequestsPendingState()
        {
            foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
            {
                oSimvarRequest.bPending = false;
                oSimvarRequest.bStillPending = false;
            }
        }

        private bool RegisterToSimConnect(SimvarRequest _oSimvarRequest)
        {
            if (m_oSimConnect != null)
            {
                if (_oSimvarRequest.bIsString)
                {
                    /// Define a data structure containing string value
                    m_oSimConnect.AddToDataDefinition(_oSimvarRequest.eDef, _oSimvarRequest.sName, "", SIMCONNECT_DATATYPE.STRING256, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    /// IMPORTANT: Register it with the simconnect managed wrapper marshaller
                    /// If you skip this step, you will only receive a uint in the .dwData field.
                    m_oSimConnect.RegisterDataDefineStruct<Struct1>(_oSimvarRequest.eDef);
                }
                else
                {
                    /// Define a data structure containing numerical value
                    m_oSimConnect.AddToDataDefinition(_oSimvarRequest.eDef, _oSimvarRequest.sName, _oSimvarRequest.sUnits, SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                    /// IMPORTANT: Register it with the simconnect managed wrapper marshaller
                    /// If you skip this step, you will only receive a uint in the .dwData field.
                    m_oSimConnect.RegisterDataDefineStruct<double>(_oSimvarRequest.eDef);
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        private void AddRequest(string _sNewSimvarRequest, string _sNewUnitRequest, bool _bIsString)
        {
            Console.WriteLine("AddRequest");

            //string sNewSimvarRequest = _sOverrideSimvarRequest != null ? _sOverrideSimvarRequest : ((m_iIndexRequest == 0) ? m_sSimvarRequest : (m_sSimvarRequest + ":" + m_iIndexRequest));
            //string sNewUnitRequest = _sOverrideUnitRequest != null ? _sOverrideUnitRequest : m_sUnitRequest;
            SimvarRequest oSimvarRequest = new SimvarRequest
            {
                eDef = (DEFINITION)m_iCurrentDefinition,
                eRequest = (REQUEST)m_iCurrentRequest,
                sName = _sNewSimvarRequest,
                bIsString = _bIsString,
                sUnits = _bIsString ? null : _sNewUnitRequest
            };

            oSimvarRequest.bPending = !RegisterToSimConnect(oSimvarRequest);
            oSimvarRequest.bStillPending = oSimvarRequest.bPending;

            lSimvarRequests.Add(oSimvarRequest);

            ++m_iCurrentDefinition;
            ++m_iCurrentRequest;
        }

        private void RemoveSelectedRequest()
        {
            lSimvarRequests.Remove(oSelectedSimvarRequest);
        }
        private void RemoveAllRequest()
        {
            while (lSimvarRequests.Count > 0)
            {
                lSimvarRequests.RemoveAt(lSimvarRequests.Count - 1);
            }
        }

        private void CopySelectedRequest(COPY_ITEM item)
        {
            if (oSelectedSimvarRequest != null)
            {
                string value = null;
                switch (item)
                {
                    case COPY_ITEM.Name:
                        value = oSelectedSimvarRequest.sName;
                        break;
                    case COPY_ITEM.Value:
                        value = oSelectedSimvarRequest.sValue;
                        break;
                    case COPY_ITEM.Unit:
                        value = oSelectedSimvarRequest.sUnits;
                        break;
                }
                if (value != null)
                    Clipboard.SetText(value);
                else
                    Clipboard.SetText("");
            }
        }

        private void TrySetValue()
        {
            Console.WriteLine("TrySetValue");
            if (m_oSelectedSimvarRequest != null && m_sSetValue != null && iObjectIdRequest != null)
            {
                if (!m_oSelectedSimvarRequest.bIsString)
                {
                    double dValue = 0.0;
                    if (double.TryParse(m_sSetValue, NumberStyles.Any, null, out dValue))
                    {
                        m_oSimConnect.SetDataOnSimObject(m_oSelectedSimvarRequest.eDef, iObjectIdRequest.GlobalID, SIMCONNECT_DATA_SET_FLAG.DEFAULT, dValue);
                    }
                }
                else
                {
                    Struct1 sValueStruct = new Struct1()
                    {
                        sValue = m_sSetValue
                    };
                    m_oSimConnect.SetDataOnSimObject(m_oSelectedSimvarRequest.eDef, iObjectIdRequest.GlobalID, SIMCONNECT_DATA_SET_FLAG.DEFAULT, sValueStruct);
                }
            }
        }

        private void LoadFiles()
        {
            Microsoft.Win32.OpenFileDialog oOpenFileDialog = new Microsoft.Win32.OpenFileDialog();
            oOpenFileDialog.Multiselect = true;
            oOpenFileDialog.Filter = "Simvars files (*.simvars)|*.simvars";
            if (oOpenFileDialog.ShowDialog() == true)
            {
                foreach (string sFilename in oOpenFileDialog.FileNames)
                {
                    LoadFile(sFilename);
                }
            }
        }

        private void LoadFile(string _sFileName)
        {
            string[] aLines = System.IO.File.ReadAllLines(_sFileName);
            for (uint i = 0; i < aLines.Length; ++i)
            {
                // Format : Simvar,Unit
                string[] aSubStrings = aLines[i].Split(',');
                if (aSubStrings.Length >= 2) // format check
                {
                    // values check
                    string[] aSimvarSubStrings = aSubStrings[0].Split(':'); // extract Simvar name from format Simvar:Index
                    string sSimvarName = Array.Find(SimUtils.SimVars.Names, s => s == aSimvarSubStrings[0]);
                    string sUnitName = Array.Find(SimUtils.Units.Names, s => s == aSubStrings[1]);
                    bool bIsString = aSubStrings.Length > 2 && bool.Parse(aSubStrings[2]);
                    if (sSimvarName != null && (sUnitName != null || bIsString))
                    {
                        AddRequest(aSubStrings[0], sUnitName, bIsString);
                    }
                    else
                    {
                        if (sSimvarName == null)
                        {
                            lErrorMessages.Add("l." + i.ToString() + " Wrong Simvar name : " + aSubStrings[0]);
                        }
                        if (sUnitName == null)
                        {
                            lErrorMessages.Add("l." + i.ToString() + " Wrong Unit name : " + aSubStrings[1]);
                        }
                    }
                }
                else
                {
                    lErrorMessages.Add("l." + i.ToString() + " Bad input format : " + aLines[i]);
                    lErrorMessages.Add("l." + i.ToString() + " Must be : SIMVAR,UNIT");
                }
            }
        }

        private void SaveFile(bool _bWriteValues, bool bValues)
        {
            Microsoft.Win32.SaveFileDialog oSaveFileDialog = new Microsoft.Win32.SaveFileDialog();
            oSaveFileDialog.Filter = "Simvars files (*.simvars)|*.simvars";
            if (oSaveFileDialog.ShowDialog() == true)
            {
                using (StreamWriter oStreamWriter = new StreamWriter(oSaveFileDialog.FileName, false))
                {
                    foreach (SimvarRequest oSimvarRequest in lSimvarRequests)
                    {
                        // Format : Simvar,Unit
                        string sFormatedLine = oSimvarRequest.sName + "," + oSimvarRequest.sUnits + "," + oSimvarRequest.bIsString;
                        if (bValues)
                        {
                            sFormatedLine += ",  " + oSimvarRequest.dValue.ToString();
                        }
                        oStreamWriter.WriteLine(sFormatedLine);
                    }
                }
            }
        }

        public void SetTickSliderValue(int _iValue)
        {
            m_oTimer.Interval = new TimeSpan(0, 0, 0, 0, (int)(_iValue));
        }
    }

}
