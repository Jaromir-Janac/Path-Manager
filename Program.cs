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
        Init _init;
        Recorder _recorder;
        Sprites _sprites;
        MySprite _spriteText;
        MySprite _spriteTexture;
        static Vector2 DefaultVectorTexture = new Vector2(128, 20);
        static Vector2 DefaultVectorText = new Vector2(12, 10);
        static Vector2 DefaultVectorGap = new Vector2(0, 30);
        Vector2 _vectorDiff;
        Vector2 _vectorTexture = DefaultVectorTexture;
        Vector2 _vectorText = DefaultVectorText;
        MyIni _ini = new MyIni();
        MyIni _customData = new MyIni();
        MyIni _customDataMe = new MyIni();
        List<MyIniKey> _iniKeysPaths = new List<MyIniKey>();
        List<MyIniKey> _iniKeys = new List<MyIniKey>();
        const string IniSectionLCD = "PathManager";
        const string IniKeyLCD = "Display";
        const string IniSectionPath = "PathList";
        const string IniSectionConfig = "Config";
        const string IniKeyWayNum = "WayNum";
        const string IniKeyDistanceStraight = "DistanceOnStraights";
        const string IniKeyDistanceTurns = "DistanceInTurns";
        const int DefIntValue = 0;
        RectangleF rectangleF;
        IMyRemoteControl _remote;
        IMyCockpit _cockpit;
        List<IMyTerminalBlock> _allNamedBlocks = new List<IMyTerminalBlock>();
        List<IMyTextSurface> _textSurfaces = new List<IMyTextSurface>();
        RectangleF _viewport;
        List<RectangleF> _viewports = new List<RectangleF>();
        StringBuilder _strBuild = new StringBuilder();
        string _wayName = "";
        int _wayNum;
        int _i = 0;
        bool _isRecording = false;
        int _tick100counter = 0;
        bool _isTick100Counting = false;
        int _menuSelectNum = 1;
        int _listSelectNum = 1;
        int _waypointCount = 0;
        int _waypointNum = 1;
        double _distance = 0;
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
        enum LcdMenuSelect {
            Main,
            Record,
            Stop,
            List,
            Path,
            Remote,
            Settings,
            Delete,
            Discard,
            RenameWarning
        }
        enum LcdMainSelect {
            Record,
            List,
            Remote,
            Settings,
            Rename
        }
        enum LcdStopSelect {
            Continue,
            Save,
            Discard
        }
        enum LcdPathSelect {
            ReplaceInRemote,
            AddToRemote,
            Back,
            Delete
        }
        enum LcdRemoteSelect {
            Reverse,
            Clear,
            AddPath,
            Back
        }
        enum LcdDeleteSelect {
            Yes,
            Cancel
        }
        enum LcdDiscardSelect {
            Yes,
            Cancel
        }
        enum LcdSettingSelect {
            ChangeStraights,
            ChangeTurns,
            Back
        }
        enum LcdMove {
            None,
            Up,
            Down,
            Apply
        }
        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update10 | UpdateFrequency.Update100;
            _init = new Init(this);
            _recorder = new Recorder(this);
            _sprites = new Sprites(this);
            Load();
            Initialize();
        }
        public void Save() {
            _ini.Set(IniSectionConfig, IniKeyWayNum, _wayNum);
            _ini.Set(IniSectionConfig, IniKeyDistanceStraight, _recorder.DistanceOnStraights);
            _ini.Set(IniSectionConfig, IniKeyDistanceTurns, _recorder.DistanceInTurns);
            Storage = _ini.ToString();
        }
        public void Load() {
            _ini.Clear();
            if (_ini.TryParse(Storage)) {
                if (_ini.Get(IniSectionConfig, IniKeyWayNum).ToInt32() != 0) {
                    _wayNum = _ini.Get(IniSectionConfig, IniKeyWayNum).ToInt32();
                }
                if (_ini.Get(IniSectionConfig, IniKeyDistanceStraight).ToInt32() != 0) {
                    _recorder.DistanceOnStraights = _ini.Get(IniSectionConfig, IniKeyDistanceStraight).ToInt32();
                }
                if (_ini.Get(IniSectionConfig, IniKeyDistanceTurns).ToInt32() != 0) {
                    _recorder.DistanceInTurns = _ini.Get(IniSectionConfig, IniKeyDistanceTurns).ToInt32();
                }
                GetListOfPaths();
                CustomDataWrite();
            }
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
        }
        void Initialize() {
            _remote = _init.GetRemoteControl();
            _cockpit = _init.GetCockpit();
            _allNamedBlocks = _init.GetMyTerminalBlocks();
            GetTextSurfaces();
            SetUpTextSurfaces();
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
                        RecordStart();
                    }
                    break;
                case "record end": {
                        RecordEnd();
                    }
                    break;
                case "record pause": {
                        RecordPause();
                    }
                    break;
                case "record continue": {
                        RecordContinue();
                    }
                    break;
                case "reverse waypoints": {
                        ReverseWaypoints();
                    }
                    break;
                case "clear waypoints": {
                        _remote.ClearWaypoints();
                    }
                    break;
                case "initialize": {
                        Initialize();
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
            Drawing();
            if (_isRecording) {
                WayRecording();
            }
        }
        void GetListOfPaths() {
            _iniKeysPaths.Clear();
            _ini.GetKeys(IniSectionPath, _iniKeysPaths);
        }
        void RenamePaths() {
            CustomDataParse();
            Save();
            Load();
        }
        void CustomDataWrite() {
            Me.CustomData = "";
            _customDataMe.Clear();
            _customDataMe.AddSection(IniSectionPath);
            foreach (MyIniKey iniKey in _iniKeysPaths) {
                _customDataMe.Set(IniSectionPath, iniKey.Name, "rename");
            }
            Me.CustomData = _customDataMe.ToString();
            GetTextSurfaces();
        }
        void CustomDataParse() {
            string customDataMe = Me.CustomData;
            int count = 0;
            _customDataMe.Clear();
            _customDataMe.TryParse(customDataMe);
            _iniKeys.Clear();
            _customDataMe.GetKeys(IniSectionPath, _iniKeys);
            MyIniValue waypoints;
            string name;
            GetListOfPaths();
            if (_iniKeys.Count != 0) {
                foreach (MyIniKey iniKey in _iniKeysPaths) {
                    name = _customDataMe.Get(IniSectionPath, _iniKeys[count].Name).ToString();
                    if (name != iniKey.Name && name != "rename" && name != null && name != "") {
                        waypoints = _ini.Get(IniSectionPath, iniKey.Name);
                        _ini.Delete(IniSectionPath, iniKey.Name);
                        _ini.Set(IniSectionPath, name, waypoints.ToString());
                    }
                    else {
                        _menuSelect = LcdMenuSelect.RenameWarning;
                    }
                    count++;
                }
            }
            else {
                _menuSelect = LcdMenuSelect.RenameWarning;
            }
        }
        void WayRecording() {
            _recorder.CheckDistance();
        }
        void RecordStart() {
            _cockpit = _init.GetCockpit();
            _waypointCount = 0;
            _recorder.StartRecording(_wayNum.ToString());
            _isRecording = true;
        }
        void RecordContinue() {
            _cockpit = _init.GetCockpit();
            _isRecording = true;
        }
        void RecordPause() {
            _isRecording = false;
        }
        void RecordEnd() {
            _recorder.SaveWay();
            _strBuild.Clear();
            foreach (Vector3D vector in _recorder.VectorsRecorded) {
                _strBuild.AppendLine($"{vector.ToString("0.0000")};");
            }
            _ini.Set(IniSectionPath, _wayNum.ToString(), _strBuild.ToString());
            _wayNum++;
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
                _ini.Set(IniSectionPath, _wayNum.ToString(), _strBuild.ToString());
                _wayNum++;
                Save();
                Load();
            }

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
        void PathReplaceInRemote() {
            _remote.ClearWaypoints();
            _waypointNum = 1;
            PathAddToRemote();
        }
        void PathDelete() {
            _ini.Delete(IniSectionPath, _wayName);
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
        void GetTextSurfaces() {
            _textSurfaces.Clear();
            foreach (IMyTextSurfaceProvider textSurface in _allNamedBlocks) {
                int count = textSurface.SurfaceCount;
                if (textSurface is IMyTextPanel) {
                    _textSurfaces.Add(textSurface.GetSurface(0));
                }
                else if (textSurface is IMyTerminalBlock) {
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
        void SetUpTextSurfaces() {
            _viewports.Clear();
            foreach (IMyTextSurface textSurface in _textSurfaces) {
                textSurface.ContentType = ContentType.SCRIPT;
                textSurface.Script = "None";
                textSurface.ScriptBackgroundColor = Color.Black;
                rectangleF = new RectangleF((textSurface.TextureSize - textSurface.SurfaceSize) / 2f,
                                    textSurface.SurfaceSize);
                _viewports.Add(rectangleF);
            }
        }
        void Drawing() {
            for (int i = 0; i < _textSurfaces.Count; i++) {
                var frame = _textSurfaces[i].DrawFrame();
                _viewport = _viewports[i];
                switch (_menuSelect) {
                    case LcdMenuSelect.Main: {
                            DrawMainMenu(ref frame, _viewport, _vectorTexture, _vectorText);
                        }
                        break;
                    case LcdMenuSelect.Record: {
                            DrawRecordMenu(ref frame, _viewport, _vectorTexture, _vectorText);
                        }
                        break;
                    case LcdMenuSelect.Stop: {
                            DrawStopMenu(ref frame, _viewport, _vectorTexture, _vectorText);
                        }
                        break;
                    case LcdMenuSelect.List: {
                            _i = 0;
                            if (_listSelectNum > 4) {
                                _i = _listSelectNum - 4;
                            }
                            _vectorDiff = DefaultVectorGap * _i;
                            DrawListMenu(ref frame, _viewport, _vectorTexture, _vectorText - _vectorDiff);
                        }
                        break;
                    case LcdMenuSelect.Path: {
                            _i = 0;
                            if (_menuSelectNum > 2) {
                                _i = _menuSelectNum - 2;
                            }
                            _vectorDiff = DefaultVectorGap * _i;
                            DrawPathMenu(ref frame, _viewport, _vectorTexture, _vectorText - _vectorDiff);
                        }
                        break;
                    case LcdMenuSelect.Remote: {
                            DrawRemoteMenu(ref frame, _viewport, _vectorTexture, _vectorText);
                        }
                        break;
                    case LcdMenuSelect.Settings: {
                            DrawSettingsMenu(ref frame, _viewport, _vectorTexture, _vectorText);
                        }
                        break;
                    case LcdMenuSelect.Delete: {
                            DrawDeleteMenu(ref frame, _viewport, _vectorTexture, _vectorText);
                        }
                        break;
                    case LcdMenuSelect.Discard: {
                            DrawDiscardMenu(ref frame, _viewport, _vectorTexture, _vectorText);
                        }
                        break;
                    case LcdMenuSelect.RenameWarning: {
                            _isTick100Counting = true;
                            DrawRenameWarning(ref frame, _viewport, _vectorText);
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
                                _waypointCount = 0;
                                _distance = 0;
                                RecordStart();
                                _menuSelect = LcdMenuSelect.Record;
                                CursorDown();
                                CursorDown();
                                CursorDown();
                            }
                            else if (_menuSelectNum == 1) {
                                _menuSelect = LcdMenuSelect.List;
                                _listSelectNum = 0;
                                CursorUp();
                            }
                            else if (_menuSelectNum == 2) {
                                _menuSelect = LcdMenuSelect.Remote;
                                _vectorTexture = DefaultVectorTexture;
                                _remoteSelect = LcdRemoteSelect.Reverse;
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
                            CursorUp();
                            _menuSelectNum--;
                            _stopSelect = (LcdStopSelect)_menuSelectNum;
                        }
                        else if (_menuSelectNum < 2 && _moveSelect == LcdMove.Down) {
                            CursorDown();
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
                                _vectorTexture = DefaultVectorTexture;
                            }
                            else if (_menuSelectNum == 2) {
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
                                _wayName = _iniKeysPaths[_listSelectNum - 1].Name;
                                _waypointList = _ini.Get(IniSectionPath, _wayName).ToString().Split(';');
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
                                _vectorTexture = DefaultVectorTexture;
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
                                _vectorTexture = DefaultVectorTexture;
                                CursorDown();
                            }
                            else if (_menuSelectNum == 1) {
                                PathAddToRemote();
                                _menuSelect = LcdMenuSelect.Main;
                                _vectorTexture = DefaultVectorTexture;
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
                                _vectorTexture = DefaultVectorTexture;
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
                                _vectorTexture = DefaultVectorTexture;
                            }
                            else if (_menuSelectNum == 1) {
                                _menuSelect = LcdMenuSelect.Path;
                                _vectorTexture = DefaultVectorTexture;
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
                                _vectorTexture = DefaultVectorTexture;
                            }
                            else if (_menuSelectNum == 1) {
                                _menuSelect = LcdMenuSelect.Stop;
                                _vectorTexture = DefaultVectorTexture;
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
                case LcdMenuSelect.RenameWarning: {
                        if (_moveSelect == LcdMove.Apply || _moveSelect == LcdMove.Down || _moveSelect == LcdMove.Up) {
                            _menuSelect = LcdMenuSelect.Main;
                        }
                    }
                    break;
            }
        }
        void CursorUp() {
            _vectorTexture -= DefaultVectorGap;
        }
        void CursorDown() {
            _vectorTexture += DefaultVectorGap;
        }
        public void DrawMainMenu(ref MySpriteDrawFrame frame, RectangleF viewport, Vector2 vectorTexture, Vector2 vectorText) {
            var positionText = vectorText + viewport.Position;
            var positionTexture = vectorTexture + viewport.Position;
            _spriteText = _sprites.SpriteText;
            _spriteTexture = _sprites.SpriteTexture;
            _spriteTexture.Position = positionTexture;
            _spriteText.Position = positionText;
            _spriteText.Data = "Start Recording";
            frame.Add(_spriteText);
            frame.Add(_spriteTexture);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "List of Paths";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Remote Control";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Settings";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Rename Paths";
            frame.Add(_spriteText);
        }
        public void DrawRecordMenu(ref MySpriteDrawFrame frame, RectangleF viewport, Vector2 vectorTexture, Vector2 vectorText) {
            var positionText = vectorText + viewport.Position;
            var positionTexture = vectorTexture + viewport.Position;
            _spriteText = _sprites.SpriteText;
            _spriteTexture = _sprites.SpriteTexture;
            _spriteTexture.Position = positionTexture;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Waypoints recorded: {_waypointCount}";
            frame.Add(_spriteText);
            frame.Add(_spriteTexture);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Total distance: {_distance:0.0}m";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap * 2;
            _spriteText.Position = positionText;
            _spriteText.Data = "Pause Recording";
            frame.Add(_spriteText);
        }
        public void DrawStopMenu(ref MySpriteDrawFrame frame, RectangleF viewport, Vector2 vectorTexture, Vector2 vectorText) {
            var positionText = vectorText + viewport.Position;
            var positionTexture = vectorTexture + viewport.Position;
            _spriteText = _sprites.SpriteText;
            _spriteTexture = _sprites.SpriteTexture;
            _spriteTexture.Position = positionTexture;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Waypoints: {_waypointCount}";
            frame.Add(_spriteText);
            frame.Add(_spriteTexture);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Distance: {_distance:0.0}m";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Continue";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Save";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Discard";
            frame.Add(_spriteText);
        }
        public void DrawListMenu(ref MySpriteDrawFrame frame, RectangleF viewport, Vector2 vectorTexture, Vector2 vectorText) {
            var positionText = vectorText + viewport.Position;
            var positionTexture = vectorTexture + viewport.Position;
            _spriteText = _sprites.SpriteText;
            _spriteTexture = _sprites.SpriteTexture;
            _spriteTexture.Position = positionTexture;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Back";
            frame.Add(_spriteText);
            frame.Add(_spriteTexture);
            foreach (MyIniKey iniKey in _iniKeysPaths) {
                positionText += DefaultVectorGap;
                _spriteText.Position = positionText;
                _spriteText.Data = iniKey.Name;
                frame.Add(_spriteText);
            }
        }
        public void DrawPathMenu(ref MySpriteDrawFrame frame, RectangleF viewport, Vector2 vectorTexture, Vector2 vectorText) {
            var positionText = vectorText + viewport.Position;
            var positionTexture = vectorTexture + viewport.Position;
            _spriteText = _sprites.SpriteText;
            _spriteTexture = _sprites.SpriteTexture;
            _spriteTexture.Position = positionTexture;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Name: {_wayName}";
            frame.Add(_spriteText);
            frame.Add(_spriteTexture);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"WP: {_waypointCount}, Distance: {_distance:0.0}m";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Replace in Remote";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Add to Remote";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "DELETE";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Back";
            frame.Add(_spriteText);
        }
        public void DrawRemoteMenu(ref MySpriteDrawFrame frame, RectangleF viewport, Vector2 vectorTexture, Vector2 vectorText) {
            var positionText = vectorText + viewport.Position;
            var positionTexture = vectorTexture + viewport.Position;
            _spriteText = _sprites.SpriteText;
            _spriteTexture = _sprites.SpriteTexture;
            _spriteTexture.Position = positionTexture;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Reverse waypoints";
            frame.Add(_spriteText);
            frame.Add(_spriteTexture);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Clear waypoints";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"New path from waypoints";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Back";
            frame.Add(_spriteText);
        }
        public void DrawDeleteMenu(ref MySpriteDrawFrame frame, RectangleF viewport, Vector2 vectorTexture, Vector2 vectorText) {
            var positionText = vectorText + viewport.Position;
            var positionTexture = vectorTexture + viewport.Position;
            _spriteText = _sprites.SpriteText;
            _spriteTexture = _sprites.SpriteTexture;
            _spriteTexture.Position = positionTexture;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Delete path:";
            frame.Add(_spriteText);
            frame.Add(_spriteTexture);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"{_wayName} ?";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Yes";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Cancel";
            frame.Add(_spriteText);
        }
        public void DrawDiscardMenu(ref MySpriteDrawFrame frame, RectangleF viewport, Vector2 vectorTexture, Vector2 vectorText) {
            var positionText = vectorText + viewport.Position;
            var positionTexture = vectorTexture + viewport.Position;
            _spriteText = _sprites.SpriteText;
            _spriteTexture = _sprites.SpriteTexture;
            _spriteTexture.Position = positionTexture;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Do you really want to";
            frame.Add(_spriteText);
            frame.Add(_spriteTexture);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"discard recorded path?";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Yes";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Cancel";
            frame.Add(_spriteText);
        }
        public void DrawSettingsMenu(ref MySpriteDrawFrame frame, RectangleF viewport, Vector2 vectorTexture, Vector2 vectorText) {
            var positionText = vectorText + viewport.Position;
            var positionTexture = vectorTexture + viewport.Position;
            _spriteText = _sprites.SpriteText;
            _spriteTexture = _sprites.SpriteTexture;
            _spriteTexture.Position = positionTexture;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Distance between WP";
            frame.Add(_spriteText);
            frame.Add(_spriteTexture);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Straights:{_recorder.DistanceOnStraights}, Turning:{_recorder.DistanceInTurns}";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Distance on straights +";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Distance while turning +";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Back";
            frame.Add(_spriteText);
        }
        public void DrawRenameWarning(ref MySpriteDrawFrame frame, RectangleF viewport, Vector2 vectorText) {
            var positionText = vectorText + viewport.Position;
            _spriteText = _sprites.SpriteText;
            _spriteTexture = _sprites.SpriteTexture;
            _spriteText.Position = positionText;
            _spriteText.Data = "Nothing to rename";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            if (_iniKeysPaths.Count == 0) {
                _spriteText.Data = "Record some paths, then";
                frame.Add(_spriteText);
                positionText += DefaultVectorGap;
                _spriteText.Position = positionText;
            }
            _spriteText.Data = "Check custom data";
            frame.Add(_spriteText);
            positionText += DefaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "of programable block";
            frame.Add(_spriteText);
        }


    }

}