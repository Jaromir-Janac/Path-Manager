//#if DEBUG
using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
using VRage.Collections;
using Sandbox.ModAPI.Ingame;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.GUI.TextPanel;
using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
//#endif 
namespace IngameScript {
    public sealed partial class Program : MyGridProgram {
        bool _isVehicle = true;
        bool _isCC = false;
        Init _init;
        Recorder _recorder;
        Drawer _drawer;
        static Vector2 s_defaultVectorTexture = new Vector2(240, 14);
        static Vector2 s_defaultVectorText = new Vector2(5, 3);
        static Vector2 s_defaultVectorGap = new Vector2(0, 30);
        Vector2 _vectorDiff;
        Vector2 _vectorText = s_defaultVectorText;
        public Vector2 VectorText {
            get { return _vectorText; }
        }
        Vector2 _vectorTexture = s_defaultVectorTexture;
        public Vector2 VectorTexture {
            get { return _vectorTexture; }
        }
        MyIni _ini = new MyIni();
        MyIni _customData = new MyIni();
        MyIni _customDataMe = new MyIni();
        MyIni _iniMessage = new MyIni();
        List<MyIniKey> _iniKeysPaths = new List<MyIniKey>();
        public List<MyIniKey> IniKeysPaths {
            get { return _iniKeysPaths; }
        }
        List<MyIniKey> _iniKeys = new List<MyIniKey>();
        Vector3D _vectorLastWP;
        public Vector3D VectorLastWP {
            get { return _vectorLastWP; }
            set { _vectorLastWP = value; }
        }
        Vector3D _vectorNormal;
        Vector2 _vector2Compass = new Vector2(0, 0);
        public Vector2 Vector2Compass {
            get { return _vector2Compass; }
        }
        const string IniSectionLCD = "PathManager";
        const string IniKeyLCD = "Display";
        const string IniSectionPath = "Path list";
        const string IniSectionWaypoint = "Path & Waypoint list";
        const string IniSectionConfig = "Config";
        const string IniKeyPathNum = "WayNum";
        const string IniKeyDistanceStraight = "DistanceOnStraights";
        const string IniKeyDistanceTurns = "DistanceInTurns";
        const string IniKeyIsVehicle = "Vehicle";
        const string IniKeyIsCC = "Command Center";
        const string IniKeyBroadcastTag = "Broadcast Tag";
        const string True = "true";
        const string False = "false";
        const int DefIntValue = 0;
        RectangleF _rectangleF;
        IMyRemoteControl _remote;
        public IMyRemoteControl Remote {
            get { return _remote; }
        }
        IMyCockpit _cockpit;
        public IMyCockpit Cockpit {
            get { return _cockpit; }
        }
        IMyCameraBlock _camera;
        List<IMyTerminalBlock> _allNamedBlocks = new List<IMyTerminalBlock>();
        List<IMyTextSurface> _textSurfaces = new List<IMyTextSurface>();
        List<IMyTextSurfaceProvider> _textSurfaceProviders = new List<IMyTextSurfaceProvider>();
        RectangleF _viewport;
        List<RectangleF> _viewports = new List<RectangleF>();
        float _scale;
        StringBuilder _strBuild = new StringBuilder();
        string _pathName = "";
        public string PathName {
            get { return _pathName; }
        }
        int _pathNum = 0;
        int _j = 0;
        bool _isRecording = false;
        bool _isPaused = false;
        int _tick100counter = 0;
        bool _isTick100Counting = false;
        int _menuSelectNum = 1;
        int _listSelectNum = 1;
        int _waypointNum = 1;
        int _waypointCount = 0;
        public int WaypointCount {
            get { return _waypointCount; }
            set { _waypointCount = value; }
        }
        double _distance = 0;
        public double Distance {
            get { return _distance; }
            set { _distance = value; }
        }
        string[] _xyz;
        double _x;
        double _y;
        double _z;
        string[] _waypointList;
        string _customDataStr;
        int _keyValueInt;
        List<MyWaypointInfo> _remoteWaypoints = new List<MyWaypointInfo>();
        LcdMenuSelect _menuSelect = LcdMenuSelect.Main;
        LcdMainSelect _mainSelect = LcdMainSelect.Record;
        LcdStopSelect _stopSelect = LcdStopSelect.Continue;
        LcdPathSelect _pathSelect = LcdPathSelect.ReplaceInRemote;
        LcdRemoteSelect _remoteSelect = LcdRemoteSelect.Reverse;
        LcdDeleteSelect _deleteSelect = LcdDeleteSelect.Cancel;
        LcdDiscardSelect _discardSelect = LcdDiscardSelect.Cancel;
        LcdSettingSelect _settingSelect = LcdSettingSelect.ChangeStraights;
        LcdMove _moveSelect = LcdMove.None;
        enum LcdMenuSelect { Main, Record, Stop, List, Path, Remote, Settings, Delete, Discard, WarningRename, WarningRemote }
        enum LcdMainSelect { Record, List, Remote, Settings, Rename }
        enum LcdStopSelect { Continue, Save, Discard }
        enum LcdPathSelect { ReplaceInRemote, AddToRemote, Back, Delete }
        enum LcdRemoteSelect { Reverse, Clear, AddPath, Back }
        enum LcdDeleteSelect { Yes, Cancel }
        enum LcdDiscardSelect { Yes, Cancel }
        enum LcdSettingSelect { ChangeStraights, ChangeTurns, Back }
        enum LcdMove { None, Up, Down, Apply }
        IMyBroadcastListener _myBroadcastListener;
        string _broadCastTag = "Path Manager";
        const string SendMePathList = "SendMePathList";
        MyIGCMessage _myIGCMessage;
        bool _isRecieving = false;
        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update10 | UpdateFrequency.Update100;
            _init = new Init(this);
            _recorder = new Recorder(this);
            _drawer = new Drawer(this);
            Load();
            Initialize();
            _myBroadcastListener = IGC.RegisterBroadcastListener(_broadCastTag);
            _myBroadcastListener.SetMessageCallback(_broadCastTag);
        }
        public void Save() {
            _ini.Set(IniSectionConfig, IniKeyPathNum, _pathNum);
            _ini.Set(IniSectionConfig, IniKeyDistanceStraight, _recorder.DistanceOnStraights);
            _ini.Set(IniSectionConfig, IniKeyDistanceTurns, _recorder.DistanceInTurns);
            string customDataMe = Me.CustomData;
            _customDataMe.Clear();
            _customDataMe.TryParse(customDataMe);
            string vehicle = _customDataMe.Get(IniSectionConfig, IniKeyIsVehicle).ToString();
            string cc = _customDataMe.Get(IniSectionConfig, IniKeyIsCC).ToString();
            if (vehicle != "") {
                if (vehicle.ToLower() == True) {
                    _ini.Set(IniSectionConfig, IniKeyIsVehicle, True);
                }
                else if (vehicle.ToLower() == False) {
                    _ini.Set(IniSectionConfig, IniKeyIsVehicle, False);
                }
            }
            if (cc != "") {
                if (cc.ToLower() == True) {
                    _ini.Set(IniSectionConfig, IniKeyIsCC, True);
                }
                else if (cc.ToLower() == False) {
                    _ini.Set(IniSectionConfig, IniKeyIsCC, False);
                }
            }
            _ini.Set(IniSectionConfig, IniKeyBroadcastTag, _broadCastTag);
            Storage = _ini.ToString();
        }
        public void Load() {
            _ini.Clear();
            if (_ini.TryParse(Storage)) {
                if (_ini.Get(IniSectionConfig, IniKeyPathNum).ToInt32() != 0) {
                    _pathNum = _ini.Get(IniSectionConfig, IniKeyPathNum).ToInt32();
                }
                if (_ini.Get(IniSectionConfig, IniKeyDistanceStraight).ToInt32() != 0) {
                    _recorder.DistanceOnStraights = _ini.Get(IniSectionConfig, IniKeyDistanceStraight).ToInt32();
                }
                if (_ini.Get(IniSectionConfig, IniKeyDistanceTurns).ToInt32() != 0) {
                    _recorder.DistanceInTurns = _ini.Get(IniSectionConfig, IniKeyDistanceTurns).ToInt32();
                }
                if (_ini.Get(IniSectionConfig, IniKeyIsVehicle).ToString() != "") {
                    string value = _ini.Get(IniSectionConfig, IniKeyIsVehicle).ToString();
                    if (value == True) {
                        _isVehicle = true;
                    }
                    else if (value == False) {
                        _isVehicle = false;
                    }
                }
                if (_ini.Get(IniSectionConfig, IniKeyIsCC).ToString() != "") {
                    string value = _ini.Get(IniSectionConfig, IniKeyIsCC).ToString();
                    if (value == True) {
                        _isCC = true;
                    }
                    else if (value == False) {
                        _isCC = false;
                    }
                }
                if (_ini.Get(IniSectionConfig, IniKeyBroadcastTag).ToString() != "") {
                    _broadCastTag = _ini.Get(IniSectionConfig, IniKeyBroadcastTag).ToString();
                }
            }
            GetListOfPaths();
            CustomDataWrite();
        }
        public void Main(string argument, UpdateType updateType) {
            if ((updateType & (UpdateType.Trigger | UpdateType.Terminal)) != 0) {
                RunCommand(argument);
            }
            if ((updateType & UpdateType.Update100) != 0) {
                Continuum100();
            }
            if ((updateType & UpdateType.Update10) != 0) {
                Continuum10();
            }
            if ((updateType & UpdateType.IGC) != 0) {
                if (_isCC || _isRecieving) {
                    RecieveMessage();
                }
            }
        }
        void Initialize() {
            if (_isVehicle) {
                _remote = _init.GetRemoteControl();
                _cockpit = _init.GetCockpit();
                _allNamedBlocks = _init.GetMyTerminalBlocks();
                GetTextSurfaces();
                SetUpTextSurfaces();
            }
        }
        void RunCommand(string input) {
            string _input = input.ToLower();
            switch (_input) {
                case "up": {
                        _moveSelect = LcdMove.Up;
                        Move();
                    }
                    break;
                case "down": {
                        _moveSelect = LcdMove.Down;
                        Move();
                    }
                    break;
                case "apply": {
                        _moveSelect = LcdMove.Apply;
                        Move();
                    }
                    break;
                case "record start": {
                        if (!_isRecording && !_isPaused) {
                            RecordStart();
                        }
                    }
                    break;
                case "record end": {
                        if (_isRecording || _isPaused) {
                            RecordEnd();
                        }
                    }
                    break;
                case "record pause": {
                        if (_isRecording) {
                            RecordPause();
                        }
                    }
                    break;
                case "record continue": {
                        if (_isPaused) {
                            RecordContinue();
                        }
                    }
                    break;
                case "reverse waypoints": {
                        ReverseWaypoints();
                    }
                    break;
                case "clear waypoints": {
                        if (_remote != null) {
                            _remote.ClearWaypoints();
                        }
                    }
                    break;
                case "initialize": {
                        Initialize();
                    }
                    break;
                case "send paths": {
                        SendPathList();
                    }
                    break;
                case "retrieve paths": {
                        RetrievePathList();
                    }
                    break;
                case "recieve paths": {
                        _isRecieving = true;
                    }
                    break;
                case "rename paths": {
                        RenamePaths();
                    }
                    break;
            }
        }
        void Continuum100() {
            if (_isTick100Counting) {
                _tick100counter++;
            }
        }
        void Continuum10() {
            if (_isVehicle) {
                Drawing();
                if (_isRecording) {
                    WayRecording();
                }
                if (_isPaused) {
                    PositionToLastWP();
                }
            }

        }

        // ANTENNA COMMUNICATION
        void RecieveMessage() {
            while (_myBroadcastListener.HasPendingMessage) {
                _myIGCMessage = _myBroadcastListener.AcceptMessage();
                if (_myIGCMessage.Tag == _broadCastTag) {
                    if (_myIGCMessage.Data is string && _myIGCMessage.Data.ToString() != "") {
                        if (_myIGCMessage.Data.ToString() == SendMePathList) {
                            SendPathList();
                        }
                        else {
                            _iniMessage.TryParse(_myIGCMessage.Data.ToString());
                            IniParse(_iniMessage);
                            _isRecieving = false;
                        }
                    }
                }
            }
        }
        void SendPathList() {
            GetListOfPaths();
            _iniMessage.Clear();
            foreach (MyIniKey iniKey in _iniKeysPaths) {
                _iniMessage.Set(IniSectionPath, iniKey.Name, _ini.Get(IniSectionPath, iniKey.Name).ToString());
            }
            _iniMessage.Set(IniSectionConfig, IniKeyPathNum, _pathNum);
            IGC.SendBroadcastMessage(_broadCastTag, _iniMessage.ToString());
        }
        void RetrievePathList() {
            IGC.SendBroadcastMessage(_broadCastTag, SendMePathList);
            _isRecieving = true;
        }
        void IniParse(MyIni iniMSG) {
            GetListOfPaths();
            _iniKeys.Clear();
            iniMSG.GetKeys(IniSectionPath, _iniKeys);
            bool isSame;
            string key;
            string waypoints;
            string waypointsMSG;
            foreach (MyIniKey iniKeyMSG in _iniKeys) {
                waypointsMSG = iniMSG.Get(IniSectionPath, iniKeyMSG.Name).ToString();
                isSame = false;
                foreach (MyIniKey iniKey in _iniKeysPaths) {
                    waypoints = _ini.Get(IniSectionPath, iniKey.Name).ToString();
                    if (iniKey.Name == iniKeyMSG.Name) {
                        if (waypointsMSG != waypoints) {
                            key = iniKey.Name + $"_old";
                            _ini.Set(IniSectionPath, key, waypoints);
                        }
                        else if (waypointsMSG == waypoints) {
                            isSame = true;
                        }
                    }
                }
                if (!isSame) {
                    _ini.Set(IniSectionPath, iniKeyMSG.Name, waypointsMSG);
                    _pathNum++;
                }
            }
            Save();
            Load();
        }

        // PATH MANAGEMENT

        void GetListOfPaths() {
            _iniKeysPaths.Clear();
            _ini.GetKeys(IniSectionPath, _iniKeysPaths);
        }
        void CustomDataWrite() {
            Me.CustomData = "";
            _customDataMe.Clear();
            foreach (MyIniKey iniKey in _iniKeysPaths) {
                _customDataMe.Set(IniSectionPath, iniKey.Name, "rename");
            }
            _customDataMe.Set(IniSectionConfig, IniKeyIsVehicle, _isVehicle);
            _customDataMe.Set(IniSectionConfig, IniKeyIsCC, _isCC);
            _customDataMe.Set(IniSectionConfig, IniKeyBroadcastTag, _broadCastTag);
            _customDataMe.SetEndComment("Recompile me after changing values in [Config]");
            foreach (MyIniKey iniKey in _iniKeysPaths) {
                _customDataMe.Set(IniSectionWaypoint, iniKey.Name, _ini.Get(IniSectionPath, iniKey.Name).ToString());
            }
            Me.CustomData = _customDataMe.ToString();
            GetTextSurfaces();

        }
        void RenamePaths() {
            CustomDataParse();
            Save();
            Load();
            GetListOfPaths();
            CustomDataWrite();
        }
        void CustomDataParse() {
            GetListOfPaths();
            string customDataMe = Me.CustomData;
            int count = 0;
            bool isSame = false;
            int i;
            int j;
            int k;
            bool isRenamed = false;
            int charCount;
            string str;
            string strNew;
            _customDataMe.Clear();
            _customDataMe.TryParse(customDataMe);
            _iniKeys.Clear();
            _customDataMe.GetKeys(IniSectionPath, _iniKeys);
            for (i = 0; i < _iniKeysPaths.Count; i++) {
                k = 1;
                if (_iniKeys.Count >= i + 1 && _iniKeysPaths[i].Name == _iniKeys[i].Name) {
                    str = _customDataMe.Get(IniSectionPath, _iniKeysPaths[i].Name).ToString();
                }
                else {
                    str = "rename";
                    _customDataMe.Set(IniSectionPath, _iniKeysPaths[i].Name, str);
                }
                if (str.ToLower() != "rename" && str != "DELETE") {
                    if (i < _iniKeysPaths.Count - 1) {
                        for (j = i + 1; j < _iniKeysPaths.Count; j++) {
                            if (_customDataMe.ContainsKey(IniSectionPath, _iniKeysPaths[j].Name)) {
                                if (str == _customDataMe.Get(IniSectionPath, _iniKeysPaths[j].Name).ToString()) {
                                    strNew = str + $"_{k}";
                                    _customDataMe.Set(IniSectionPath, _iniKeysPaths[j].Name, strNew);
                                    k++;
                                }
                            }
                        }
                    }
                }
            }
            MyIniValue waypoints;
            string name;
            if (_iniKeys.Count != 0) {
                foreach (MyIniKey iniKey in _iniKeysPaths) {
                    name = _customDataMe.Get(IniSectionPath, iniKey.Name).ToString();
                    if (name != iniKey.Name && name != "rename" && name != null && name != "") {
                        i = 0;
                        charCount = name.Length;
                        do {
                            if (isSame) {
                                name += $"_{i}";
                                isSame = false;
                            }
                            foreach (MyIniKey key in _iniKeysPaths) {
                                if (name == key.Name) {
                                    isSame = true;
                                    if (name.Length > charCount) {
                                        name = name.Remove(charCount);
                                    }
                                }
                            }
                            i++;
                        } while (isSame);
                        waypoints = _ini.Get(IniSectionPath, iniKey.Name);
                        _ini.Delete(IniSectionPath, iniKey.Name);
                        if (name != "DELETE") {
                            _ini.Set(IniSectionPath, name, waypoints.ToString());
                        }
                        isRenamed = true;
                    }
                    count++;
                }
            }
            if (!isRenamed) {
                _menuSelect = LcdMenuSelect.WarningRename;
            }
        }

        // PATH Recording
        void RecordStart() {
            _cockpit = _init.GetCockpit();
            _waypointCount = 0;
            _recorder.StartRecording(_pathNum.ToString());
            _isRecording = true;
        }
        void WayRecording() {
            _recorder.CheckDistance();
        }
        void RecordContinue() {
            _cockpit = _init.GetCockpit();
            _isRecording = true;
        }
        void RecordPause() {
            _isRecording = false;
            _isPaused = true;
        }
        void RecordEnd() {
            _recorder.SaveWay();
            _strBuild.Clear();
            foreach (Vector3D vector in _recorder.VectorsRecorded) {
                _strBuild.AppendLine($"{vector.ToString("0.0000")};");
            }
            _ini.Set(IniSectionPath, _pathNum.ToString(), _strBuild.ToString());
            _pathNum++;
            _isRecording = false;
            Save();
            Load();
        }
        void PathAddToRemote() {
            foreach (string waypoint in _waypointList) {
                var vector = GetVector3D(waypoint);
                if (waypoint != "") {
                    _remote.AddWaypoint(vector, _waypointNum.ToString());
                    _waypointNum++;
                }
            }
        }
        void CreatePathFromRemote() {
            _remoteWaypoints.Clear();
            _remote.GetWaypointInfo(_remoteWaypoints);
            if (_remoteWaypoints.Count != 0) {
                _strBuild.Clear();
                foreach (MyWaypointInfo waypointInfo in _remoteWaypoints) {
                    Vector3D vector = waypointInfo.Coords;
                    _strBuild.AppendLine($"{vector.ToString("0.0000")};");
                }
                _ini.Set(IniSectionPath, _pathNum.ToString(), _strBuild.ToString());
                _pathNum++;
                Save();
                Load();
            }
        }
        void PathReplaceInRemote() {
            _remote.ClearWaypoints();
            _waypointNum = 1;
            PathAddToRemote();
        }
        void PathDelete() {
            _ini.Delete(IniSectionPath, _pathName);
            Save();
            Load();
        }
        void PathDiscard() {
            _recorder.DiscardWay();
        }
        void ReverseWaypoints() {
            _remoteWaypoints.Clear();
            _remote.GetWaypointInfo(_remoteWaypoints);
            _remoteWaypoints.Reverse();
            _remote.ClearWaypoints();
            foreach (MyWaypointInfo waypointInfo in _remoteWaypoints) {
                _remote.AddWaypoint(waypointInfo);
            }
        }
        void PositionToLastWP() {
            _vectorNormal = GetVectorNormalize(GetVectorDirectionLocal(GetVectorDirection(_remote.GetPosition(), _vectorLastWP)));
            _vector2Compass = new Vector2((float)_vectorNormal.X, (float)_vectorNormal.Y * -1);
        }
        Vector3D GetVector3D(string str) {
            var vector = new Vector3D();
            if (str != "") {
                _xyz = str.Split(':');
                _x = Convert.ToDouble(_xyz[1].Replace("X", "").Replace("Y", "").Replace("Z", "").Replace(" ", "").Replace("{", "").Replace("}", ""));
                _y = Convert.ToDouble(_xyz[2].Replace("X", "").Replace("Y", "").Replace("Z", "").Replace(" ", "").Replace("{", "").Replace("}", ""));
                _z = Convert.ToDouble(_xyz[3].Replace("X", "").Replace("Y", "").Replace("Z", "").Replace(" ", "").Replace("{", "").Replace("}", ""));
                vector = new Vector3D(_x, _y, _z);
            }
            return vector;
        }
        Vector3D GetVectorNormalize(Vector3D vector) {
            return vector / vector.Length();
        }
        Vector3D GetVectorDirection(Vector3D start, Vector3D end) {
            return end - start;
        }
        Vector3D GetVectorDirectionLocal(Vector3D vectorWorld) {
            return Vector3D.TransformNormal(vectorWorld, MatrixD.Transpose(_remote.WorldMatrix));
        }

        // CAMERA

        void GetCameraWapoint() {
            GetCamera();
            if (_camera != null) {
                //_camera.EnableRaycast = true;
                //_debug.PrintChat($"{_camera.Raycast(2000).HitPosition.Value}");
                //_camera.EnableRaycast = false;
            }
        }
        void GetCamera() {
            foreach (IMyTerminalBlock terminalBlock in _allNamedBlocks) {
                if (terminalBlock is IMyCameraBlock) {
                    _camera = terminalBlock as IMyCameraBlock;
                    if (_camera.IsActive) {
                        break;
                    }
                }
            }
        }

        // TEXT SURFACES

        void GetTextSurfaces() {
            if (_isVehicle) {
                _textSurfaces.Clear();
                _textSurfaceProviders.Clear();
                foreach (IMyTerminalBlock terminalBlock in _allNamedBlocks) {
                    if (terminalBlock is IMyTextSurfaceProvider) {
                        _textSurfaceProviders.Add(terminalBlock as IMyTextSurfaceProvider);
                    }
                }
                foreach (IMyTextSurfaceProvider textSurface in _textSurfaceProviders) {
                    int count = textSurface.SurfaceCount;
                    if (textSurface is IMyTextPanel) {
                        IMyTextPanel textPanel = textSurface as IMyTextPanel;
                        _textSurfaces.Add(textSurface.GetSurface(0));
                    }
                    else if (textSurface is IMyCockpit || textSurface is IMyProgrammableBlock) {
                        IMyTerminalBlock cockpitLcd = textSurface as IMyTerminalBlock;
                        _customDataStr = cockpitLcd.CustomData;
                        _customData.TryParse(_customDataStr);
                        if (!_customData.ContainsKey(IniSectionLCD, IniKeyLCD)) {
                            _customData.Set(IniSectionLCD, IniKeyLCD, DefIntValue);
                        }
                        _customData.Get(IniSectionLCD, IniKeyLCD).TryGetInt32(out _keyValueInt);
                        if (_keyValueInt >= count) {
                            _keyValueInt = count - 1;
                            _customData.Set(IniSectionLCD, IniKeyLCD, _keyValueInt);
                        }
                        cockpitLcd.CustomData = _customData.ToString();
                        _textSurfaces.Add(textSurface.GetSurface(_keyValueInt));
                    }
                }
            }
        }
        void SetUpTextSurfaces() {
            if (_isVehicle) {
                _viewports.Clear();
                foreach (IMyTextSurface textSurface in _textSurfaces) {
                    textSurface.ContentType = ContentType.SCRIPT;
                    textSurface.Script = "None";
                    textSurface.ScriptBackgroundColor = Color.Black;
                    _rectangleF = new RectangleF((textSurface.TextureSize - textSurface.SurfaceSize) / 2f,
                                        textSurface.SurfaceSize);
                    _viewports.Add(_rectangleF);
                }
            }
        }

        // DRAWING / MOVING

        void Drawing() {
            for (int i = 0; i < _textSurfaces.Count; i++) {
                var frame = _textSurfaces[i].DrawFrame();
                _viewport = _viewports[i];
                if (_viewport.Size.Y < 50) {
                    _scale = 0.35f;
                }
                else if (_viewport.Size.Y >= 50 && _viewport.Size.Y <= 74) {
                    _scale = 0.55f;
                }
                else if (_viewport.Size.Y > 74 && _viewport.Size.Y <= 128) {
                    _scale = 0.65f;
                }
                else if (_viewport.Size.Y > 128 && _viewport.Size.Y <= 320) {
                    _scale = 1f;
                }
                else if (_viewport.Size.Y > 75) {
                    _scale = 2f;
                }
                switch (_menuSelect) {
                    case LcdMenuSelect.Main: {
                            _drawer.DrawMainMenu(ref frame, _viewport, _scale); ;
                        }
                        break;
                    case LcdMenuSelect.Record: {
                            _drawer.DrawRecordMenu(ref frame, _viewport, _scale);
                        }
                        break;
                    case LcdMenuSelect.Stop: {
                            _j = 0;
                            if (_menuSelectNum > 2) {
                                _j = _menuSelectNum - 2;
                            }
                            _vectorDiff = s_defaultVectorGap * _j;
                            _drawer.DrawStopMenu(ref frame, _viewport, _scale, (_vectorText - _vectorDiff));
                        }
                        break;
                    case LcdMenuSelect.List: {
                            _j = 0;
                            if (_listSelectNum > 4) {
                                _j = _listSelectNum - 4;
                            }
                            _vectorDiff = s_defaultVectorGap * _j;
                            _drawer.DrawListMenu(ref frame, _viewport, _scale, (_vectorText - _vectorDiff));
                        }
                        break;
                    case LcdMenuSelect.Path: {
                            _j = 0;
                            if (_menuSelectNum > 2) {
                                _j = _menuSelectNum - 2;
                            }
                            _vectorDiff = s_defaultVectorGap * _j;
                            _drawer.DrawPathMenu(ref frame, _viewport, _scale, (_vectorText - _vectorDiff));
                        }
                        break;
                    case LcdMenuSelect.Remote: {
                            _drawer.DrawRemoteMenu(ref frame, _viewport, _scale);
                        }
                        break;
                    case LcdMenuSelect.Settings: {
                            _drawer.DrawSettingsMenu(ref frame, _viewport, _scale);
                        }
                        break;
                    case LcdMenuSelect.Delete: {
                            _drawer.DrawDeleteMenu(ref frame, _viewport, _scale);
                        }
                        break;
                    case LcdMenuSelect.Discard: {
                            _drawer.DrawDiscardMenu(ref frame, _viewport, _scale);
                        }
                        break;
                    case LcdMenuSelect.WarningRename: {
                            _isTick100Counting = true;
                            _drawer.DrawWarningRename(ref frame, _viewport, _scale);
                            if (_tick100counter > 5) {
                                _menuSelect = LcdMenuSelect.Main;
                                _isTick100Counting = false;
                            }
                        }
                        break;
                    case LcdMenuSelect.WarningRemote: {
                            _isTick100Counting = true;
                            _drawer.DrawWarningRemote(ref frame, _viewport, _scale);
                            if (_tick100counter > 5) {
                                _menuSelect = LcdMenuSelect.Main;
                                _isTick100Counting = false;
                            }
                        }
                        break;
                }
                frame.Dispose();
            }
        }
        void Move() {
            switch (_menuSelect) {
                case LcdMenuSelect.Main: {
                        _menuSelectNum = (int)_mainSelect;
                        if (_menuSelectNum > 0 && _moveSelect == LcdMove.Up) {
                            CursorUp();
                            _menuSelectNum--;
                            _mainSelect = (LcdMainSelect)_menuSelectNum;
                        }
                        else if (_menuSelectNum < 4 && _moveSelect == LcdMove.Down) {
                            CursorDown();
                            _menuSelectNum++;
                            _mainSelect = (LcdMainSelect)_menuSelectNum;
                        }
                        else if (_moveSelect == LcdMove.Apply) {
                            if (_menuSelectNum == 0) {
                                if (_remote != null) {
                                    _waypointCount = 0;
                                    _distance = 0;
                                    RecordStart();
                                    _menuSelect = LcdMenuSelect.Record;
                                    CursorDown();
                                    CursorDown();
                                    CursorDown();
                                }
                                else {
                                    _tick100counter = 0;
                                    _menuSelect = LcdMenuSelect.WarningRemote;
                                }
                            }
                            else if (_menuSelectNum == 1) {
                                _menuSelect = LcdMenuSelect.List;
                                _listSelectNum = 0;
                                CursorUp();
                            }
                            else if (_menuSelectNum == 2) {
                                if (_remote != null) {
                                    _menuSelect = LcdMenuSelect.Remote;
                                    _vectorTexture = s_defaultVectorTexture;
                                    _remoteSelect = LcdRemoteSelect.Reverse;
                                }
                                else {
                                    _tick100counter = 0;
                                    _menuSelect = LcdMenuSelect.WarningRemote;
                                }
                            }
                            else if (_menuSelectNum == 3) {
                                _menuSelect = LcdMenuSelect.Settings;
                                CursorUp();
                                _settingSelect = LcdSettingSelect.ChangeStraights;
                            }
                            else if (_menuSelectNum == 4) {
                                RenamePaths();
                                _tick100counter = 0;
                            }
                        }
                        _moveSelect = LcdMove.None;

                    }
                    break;
                case LcdMenuSelect.Record: {
                        if (_moveSelect == LcdMove.Apply) {
                            RecordPause();
                            _menuSelect = LcdMenuSelect.Stop;
                            _stopSelect = LcdStopSelect.Continue;
                            CursorUp();
                        }
                        _moveSelect = LcdMove.None;

                    }
                    break;
                case LcdMenuSelect.Stop: {
                        _menuSelectNum = (int)_stopSelect;
                        if (_menuSelectNum > 0 && _moveSelect == LcdMove.Up) {
                            if (_menuSelectNum <= 2) {
                                CursorUp();
                            }
                            _menuSelectNum--;
                            _stopSelect = (LcdStopSelect)_menuSelectNum;
                        }
                        else if (_menuSelectNum < 3 && _moveSelect == LcdMove.Down) {
                            if (_menuSelectNum < 2) {
                                CursorDown();
                            }
                            _menuSelectNum++;
                            _stopSelect = (LcdStopSelect)_menuSelectNum;
                        }
                        else if (_moveSelect == LcdMove.Apply) {
                            if (_menuSelectNum == 0) {
                                RecordContinue();
                                _menuSelect = LcdMenuSelect.Record;
                                CursorDown();
                            }
                            else if (_menuSelectNum == 1) {
                                RecordEnd();
                                _menuSelect = LcdMenuSelect.Main;
                                _vectorTexture = s_defaultVectorTexture;
                            }
                            else if (_menuSelectNum == 2) {
                                if (_waypointCount > 1) {
                                    _waypointCount--;
                                    _recorder.DeleteLastWaypoint();
                                }
                            }
                            else if (_menuSelectNum == 3) {
                                _menuSelect = LcdMenuSelect.Discard;
                                _discardSelect = LcdDiscardSelect.Cancel;
                                CursorUp();
                            }
                        }
                        _moveSelect = LcdMove.None;

                    }
                    break;
                case LcdMenuSelect.List: {
                        if (_listSelectNum > 0 && _moveSelect == LcdMove.Up) {
                            if (_listSelectNum <= 4) {
                                CursorUp();
                            }
                            _listSelectNum--;
                        }
                        else if (_listSelectNum < _iniKeysPaths.Count() && _moveSelect == LcdMove.Down) {
                            if (_listSelectNum < 4) {
                                CursorDown();
                            }
                            _listSelectNum++;
                        }
                        else if (_moveSelect == LcdMove.Apply) {
                            if (_listSelectNum == 0) {
                                _menuSelect = LcdMenuSelect.Main;
                                CursorDown();
                            }
                            else {
                                _waypointCount = 0;
                                _distance = 0;
                                _pathName = _iniKeysPaths[_listSelectNum - 1].Name;
                                _waypointList = _ini.Get(IniSectionPath, _pathName).ToString().Split(';');
                                var vectorStart = new Vector3D(0, 0, 0);
                                foreach (string waypoint in _waypointList) {
                                    if (waypoint != "") {
                                        _waypointCount++;
                                    }
                                    var vector = GetVector3D(waypoint);
                                    if (vectorStart.X != 0 && vector.X != 0) {
                                        _distance += Vector3D.Distance(vectorStart, vector);
                                    }
                                    if (vector.X != 0) {
                                        vectorStart = vector;
                                    }
                                }
                                _menuSelect = LcdMenuSelect.Path;
                                _vectorTexture = s_defaultVectorTexture;
                                CursorDown();
                                CursorDown();
                                _pathSelect = LcdPathSelect.ReplaceInRemote;
                            }
                        }
                        _moveSelect = LcdMove.None;
                    }
                    break;
                case LcdMenuSelect.Path: {
                        _menuSelectNum = (int)_pathSelect;
                        if (_menuSelectNum > 0 && _moveSelect == LcdMove.Up) {
                            if (_menuSelectNum <= 2) {
                                CursorUp();
                            }
                            _menuSelectNum--;
                            _pathSelect = (LcdPathSelect)_menuSelectNum;
                        }
                        else if (_menuSelectNum < 3 && _moveSelect == LcdMove.Down) {
                            if (_menuSelectNum < 2) {
                                CursorDown();
                            }
                            _menuSelectNum++;
                            _pathSelect = (LcdPathSelect)_menuSelectNum;
                        }
                        else if (_moveSelect == LcdMove.Apply) {
                            if (_menuSelectNum == 0) {
                                PathReplaceInRemote();
                                _menuSelect = LcdMenuSelect.Main;
                                _vectorTexture = s_defaultVectorTexture;
                                CursorDown();
                            }
                            else if (_menuSelectNum == 1) {
                                PathAddToRemote();
                                _menuSelect = LcdMenuSelect.Main;
                                _vectorTexture = s_defaultVectorTexture;
                                CursorDown();
                            }
                            else if (_menuSelectNum == 2) {
                                _menuSelect = LcdMenuSelect.Delete;
                                _deleteSelect = LcdDeleteSelect.Cancel;
                                CursorUp();
                            }
                            else if (_menuSelectNum == 3) {
                                _menuSelect = LcdMenuSelect.List;
                                _listSelectNum = 0;
                                _vectorTexture = s_defaultVectorTexture;
                                _menuSelectNum = 0;
                            }
                        }
                        _moveSelect = LcdMove.None;

                    }
                    break;
                case LcdMenuSelect.Remote: {
                        _menuSelectNum = (int)_remoteSelect;
                        if (_menuSelectNum > 0 && _moveSelect == LcdMove.Up) {
                            CursorUp();
                            _menuSelectNum--;
                            _remoteSelect = (LcdRemoteSelect)_menuSelectNum;
                        }
                        else if (_menuSelectNum < 3 && _moveSelect == LcdMove.Down) {
                            CursorDown();
                            _menuSelectNum++;
                            _remoteSelect = (LcdRemoteSelect)_menuSelectNum;
                        }
                        else if (_moveSelect == LcdMove.Apply) {
                            if (_menuSelectNum == 0) {
                                ReverseWaypoints();
                                _menuSelect = LcdMenuSelect.Main;
                                CursorDown();
                                CursorDown();
                            }
                            else if (_menuSelectNum == 1) {
                                _remote.ClearWaypoints();
                                _menuSelect = LcdMenuSelect.Main;
                                CursorDown();
                            }
                            else if (_menuSelectNum == 2) {
                                CreatePathFromRemote();
                                _menuSelect = LcdMenuSelect.Main;
                            }
                            else if (_menuSelectNum == 3) {
                                _menuSelect = LcdMenuSelect.Main;
                                CursorUp();
                            }
                        }
                        _moveSelect = LcdMove.None;
                    }
                    break;
                case LcdMenuSelect.Delete: {
                        _menuSelectNum = (int)_deleteSelect;
                        if (_menuSelectNum > 0 && _moveSelect == LcdMove.Up) {
                            CursorUp();
                            _menuSelectNum--;
                            _deleteSelect = (LcdDeleteSelect)_menuSelectNum;
                        }
                        else if (_menuSelectNum < 1 && _moveSelect == LcdMove.Down) {
                            CursorDown();
                            _menuSelectNum++;
                            _deleteSelect = (LcdDeleteSelect)_menuSelectNum;
                        }
                        else if (_moveSelect == LcdMove.Apply) {
                            if (_menuSelectNum == 0) {
                                PathDelete();
                                _menuSelect = LcdMenuSelect.List;
                                _listSelectNum = 0;
                                _vectorTexture = s_defaultVectorTexture;
                            }
                            else if (_menuSelectNum == 1) {
                                _menuSelect = LcdMenuSelect.Path;
                                _vectorTexture = s_defaultVectorTexture;
                                CursorDown();
                                CursorDown();
                                _pathSelect = LcdPathSelect.ReplaceInRemote;
                            }
                        }
                        _moveSelect = LcdMove.None;
                    }
                    break;
                case LcdMenuSelect.Discard: {
                        _menuSelectNum = (int)_discardSelect;
                        if (_menuSelectNum > 0 && _moveSelect == LcdMove.Up) {
                            CursorUp();
                            _menuSelectNum--;
                            _discardSelect = (LcdDiscardSelect)_menuSelectNum;
                        }
                        else if (_menuSelectNum < 1 && _moveSelect == LcdMove.Down) {
                            CursorDown();
                            _menuSelectNum++;
                            _discardSelect = (LcdDiscardSelect)_menuSelectNum;
                        }
                        else if (_moveSelect == LcdMove.Apply) {
                            if (_menuSelectNum == 0) {
                                PathDiscard();
                                _menuSelect = LcdMenuSelect.Main;
                                _mainSelect = LcdMainSelect.Record;
                                _vectorTexture = s_defaultVectorTexture;
                            }
                            else if (_menuSelectNum == 1) {
                                _menuSelect = LcdMenuSelect.Stop;
                                _vectorTexture = s_defaultVectorTexture;
                                CursorDown();
                                CursorDown();
                                _stopSelect = LcdStopSelect.Continue;
                            }
                        }
                        _moveSelect = LcdMove.None;
                    }
                    break;
                case LcdMenuSelect.Settings: {
                        _menuSelectNum = (int)_settingSelect;
                        if (_menuSelectNum > 0 && _moveSelect == LcdMove.Up) {
                            CursorUp();
                            _menuSelectNum--;
                            _settingSelect = (LcdSettingSelect)_menuSelectNum;
                        }
                        else if (_menuSelectNum < 2 && _moveSelect == LcdMove.Down) {
                            CursorDown();
                            _menuSelectNum++;
                            _settingSelect = (LcdSettingSelect)_menuSelectNum;
                        }
                        else if (_moveSelect == LcdMove.Apply) {
                            if (_menuSelectNum == 0) {
                                if (_recorder.DistanceOnStraights >= 500) {
                                    _recorder.DistanceOnStraights += 100;
                                }
                                else {
                                    _recorder.DistanceOnStraights += 50;
                                }
                                if (_recorder.DistanceOnStraights > 1000) {
                                    _recorder.DistanceOnStraights = 200;
                                }
                            }
                            else if (_menuSelectNum == 1) {
                                if (_recorder.DistanceInTurns >= 100) {
                                    _recorder.DistanceInTurns += 20;
                                }
                                else {
                                    _recorder.DistanceInTurns += 10;
                                }
                                if (_recorder.DistanceInTurns >= 200) {
                                    _recorder.DistanceInTurns = 20;
                                }
                            }
                            else if (_menuSelectNum == 2) {
                                _menuSelect = LcdMenuSelect.Main;
                                CursorUp();
                                Save();
                                Load();
                            }
                        }
                        _moveSelect = LcdMove.None;
                    }
                    break;
                case LcdMenuSelect.WarningRename: {
                        if (_moveSelect == LcdMove.Apply || _moveSelect == LcdMove.Down || _moveSelect == LcdMove.Up) {
                            _menuSelect = LcdMenuSelect.Main;
                        }
                    }
                    break;
                case LcdMenuSelect.WarningRemote: {
                        if (_moveSelect == LcdMove.Apply || _moveSelect == LcdMove.Down || _moveSelect == LcdMove.Up) {
                            _menuSelect = LcdMenuSelect.Main;
                        }
                    }
                    break;
            }
        }
        void CursorUp() {
            _vectorTexture -= s_defaultVectorGap;
        }
        void CursorDown() {
            _vectorTexture += s_defaultVectorGap;
        }



    }

}