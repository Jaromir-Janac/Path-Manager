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
        static Vector2 s_defaultVectorTexture = new Vector2(128, 20);
        static Vector2 s_defaultVectorText = new Vector2(12, 10);
        static Vector2 s_defaultVectorGap = new Vector2(0, 30);
        Vector2 _vectorDiff;
        Vector2 _vectorTexture = s_defaultVectorTexture;
        Vector2 _vectorText = s_defaultVectorText;
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
        string pathName = "";
        int pathNum;
        int j = 0;
        bool isRecording = false;
        int tick100counter = 0;
        bool isTick100Counting = false;
        int menuSelectNum = 1;
        int listSelectNum = 1;
        int waypointCount = 0;
        int waypointNum = 1;
        double distance = 0;
        string[] xyz;
        double x;
        double y;
        double z;
        string[] waypointList;
        string customDataStr;
        int keyValueInt;
        List<MyWaypointInfo> remoteWaypoints = new List<MyWaypointInfo>();
        LcdMenuSelect menuSelect = LcdMenuSelect.Main;
        LcdMainSelect mainSelect = LcdMainSelect.Record;
        LcdStopSelect stopSelect = LcdStopSelect.Continue;
        LcdPathSelect pathSelect = LcdPathSelect.ReplaceInRemote;
        LcdRemoteSelect remoteSelect = LcdRemoteSelect.Reverse;
        LcdDeleteSelect deleteSelect = LcdDeleteSelect.Cancel;
        LcdDiscardSelect discardSelect = LcdDiscardSelect.Cancel;
        LcdSettingSelect settingSelect = LcdSettingSelect.ChangeStraights;
        LcdMove moveSelect = LcdMove.None;
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
            _ini.Set(IniSectionConfig, IniKeyWayNum, pathNum);
            _ini.Set(IniSectionConfig, IniKeyDistanceStraight, _recorder.DistanceOnStraights);
            _ini.Set(IniSectionConfig, IniKeyDistanceTurns, _recorder.DistanceInTurns);
            Storage = _ini.ToString();
        }
        public void Load() {
            _ini.Clear();
            if (_ini.TryParse(Storage)) {
                if (_ini.Get(IniSectionConfig, IniKeyWayNum).ToInt32() != 0) {
                    pathNum = _ini.Get(IniSectionConfig, IniKeyWayNum).ToInt32();
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
                        moveSelect = LcdMove.Up;
                        Move();
                    }
                    break;
                case "down": {
                        moveSelect = LcdMove.Down;
                        Move();
                    }
                    break;
                case "apply": {
                        moveSelect = LcdMove.Apply;
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
            if (isTick100Counting) {
                tick100counter++;
            }
        }
        void Continuum10() {
            Drawing();
            if (isRecording) {
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
            GetListOfPaths();
            string customDataMe = Me.CustomData;
            int count = 0;
            string nameCurrent;
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
                if (str != "rename") {
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
                                nameCurrent = key.Name;
                                if (name == nameCurrent) {
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
                        _ini.Set(IniSectionPath, name, waypoints.ToString());
                        isRenamed = true;
                    }
                    count++;
                }
            }
            if (!isRenamed) {
                menuSelect = LcdMenuSelect.RenameWarning;
            }
        }
        void WayRecording() {
            _recorder.CheckDistance();
        }
        void RecordStart() {
            _cockpit = _init.GetCockpit();
            waypointCount = 0;
            _recorder.StartRecording(pathNum.ToString());
            isRecording = true;
        }
        void RecordContinue() {
            _cockpit = _init.GetCockpit();
            isRecording = true;
        }
        void RecordPause() {
            isRecording = false;
        }
        void RecordEnd() {
            _recorder.SaveWay();
            _strBuild.Clear();
            foreach (Vector3D vector in _recorder.VectorsRecorded) {
                _strBuild.AppendLine($"{vector.ToString("0.0000")};");
            }
            _ini.Set(IniSectionPath, pathNum.ToString(), _strBuild.ToString());
            pathNum++;
            isRecording = false;
            Save();
            Load();
        }
        void PathAddToRemote() {
            foreach (string waypoint in waypointList) {
                var vector = GetVector3D(waypoint);
                if (waypoint != "") {
                    _remote.AddWaypoint(vector, waypointNum.ToString());
                    waypointNum++;
                }
            }
        }
        void CreatePathFromRemote() {
            remoteWaypoints.Clear();
            _remote.GetWaypointInfo(remoteWaypoints);
            if (remoteWaypoints.Count != 0) {
                _strBuild.Clear();
                foreach (MyWaypointInfo waypointInfo in remoteWaypoints) {
                    Vector3D vector = waypointInfo.Coords;
                    _strBuild.AppendLine($"{vector.ToString("0.0000")};");
                }
                _ini.Set(IniSectionPath, pathNum.ToString(), _strBuild.ToString());
                pathNum++;
                Save();
                Load();
            }

        }
        Vector3D GetVector3D(string str) {
            var vector = new Vector3D();
            if (str != "") {
                xyz = str.Split(':');
                x = Convert.ToDouble(xyz[1].Replace("X", "").Replace("Y", "").Replace("Z", "").Replace(" ", "").Replace("{", "").Replace("}", ""));
                y = Convert.ToDouble(xyz[2].Replace("X", "").Replace("Y", "").Replace("Z", "").Replace(" ", "").Replace("{", "").Replace("}", ""));
                z = Convert.ToDouble(xyz[3].Replace("X", "").Replace("Y", "").Replace("Z", "").Replace(" ", "").Replace("{", "").Replace("}", ""));
                vector = new Vector3D(x, y, z);
            }
            return vector;
        }
        void PathReplaceInRemote() {
            _remote.ClearWaypoints();
            waypointNum = 1;
            PathAddToRemote();
        }
        void PathDelete() {
            _ini.Delete(IniSectionPath, pathName);
            Save();
            Load();
        }
        void PathDiscard() {
            _recorder.DiscardWay();
        }
        void ReverseWaypoints() {
            remoteWaypoints.Clear();
            _remote.GetWaypointInfo(remoteWaypoints);
            remoteWaypoints.Reverse();
            _remote.ClearWaypoints();
            foreach (MyWaypointInfo waypointInfo in remoteWaypoints) {
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
                    customDataStr = cockpitLcd.CustomData;
                    _customData.TryParse(customDataStr);
                    if (!_customData.ContainsKey(IniSectionLCD, IniKeyLCD)) {
                        _customData.Set(IniSectionLCD, IniKeyLCD, DefIntValue);
                    }
                    _customData.Get(IniSectionLCD, IniKeyLCD).TryGetInt32(out keyValueInt);
                    if (keyValueInt >= count) {
                        keyValueInt = count - 1;
                        _customData.Set(IniSectionLCD, IniKeyLCD, keyValueInt);
                    }
                    cockpitLcd.CustomData = _customData.ToString();
                    _textSurfaces.Add(textSurface.GetSurface(keyValueInt));
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
                switch (menuSelect) {
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
                            j = 0;
                            if (listSelectNum > 4) {
                                j = listSelectNum - 4;
                            }
                            _vectorDiff = s_defaultVectorGap * j;
                            DrawListMenu(ref frame, _viewport, _vectorTexture, _vectorText - _vectorDiff);
                        }
                        break;
                    case LcdMenuSelect.Path: {
                            j = 0;
                            if (menuSelectNum > 2) {
                                j = menuSelectNum - 2;
                            }
                            _vectorDiff = s_defaultVectorGap * j;
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
                            isTick100Counting = true;
                            DrawRenameWarning(ref frame, _viewport, _vectorText);
                            if (tick100counter > 5) {
                                menuSelect = LcdMenuSelect.Main;
                                isTick100Counting = false;
                            }
                        }
                        break;
                }
                frame.Dispose();
            }
        }
        void Move() {
            switch (menuSelect) {
                case LcdMenuSelect.Main: {
                        menuSelectNum = (int)mainSelect;
                        if (menuSelectNum > 0 && moveSelect == LcdMove.Up) {
                            CursorUp();
                            menuSelectNum--;
                            mainSelect = (LcdMainSelect)menuSelectNum;
                        }
                        else if (menuSelectNum < 4 && moveSelect == LcdMove.Down) {
                            CursorDown();
                            menuSelectNum++;
                            mainSelect = (LcdMainSelect)menuSelectNum;
                        }
                        else if (moveSelect == LcdMove.Apply) {
                            if (menuSelectNum == 0) {
                                waypointCount = 0;
                                distance = 0;
                                RecordStart();
                                menuSelect = LcdMenuSelect.Record;
                                CursorDown();
                                CursorDown();
                                CursorDown();
                            }
                            else if (menuSelectNum == 1) {
                                menuSelect = LcdMenuSelect.List;
                                listSelectNum = 0;
                                CursorUp();
                            }
                            else if (menuSelectNum == 2) {
                                menuSelect = LcdMenuSelect.Remote;
                                _vectorTexture = s_defaultVectorTexture;
                                remoteSelect = LcdRemoteSelect.Reverse;
                            }
                            else if (menuSelectNum == 3) {
                                menuSelect = LcdMenuSelect.Settings;
                                CursorUp();
                                settingSelect = LcdSettingSelect.ChangeStraights;
                            }
                            else if (menuSelectNum == 4) {
                                RenamePaths();
                                tick100counter = 0;
                            }
                        }
                        moveSelect = LcdMove.None;

                    }
                    break;
                case LcdMenuSelect.Record: {
                        if (moveSelect == LcdMove.Apply) {
                            RecordPause();
                            menuSelect = LcdMenuSelect.Stop;
                            stopSelect = LcdStopSelect.Continue;
                            CursorUp();
                        }
                        moveSelect = LcdMove.None;

                    }
                    break;
                case LcdMenuSelect.Stop: {
                        menuSelectNum = (int)stopSelect;
                        if (menuSelectNum > 0 && moveSelect == LcdMove.Up) {
                            CursorUp();
                            menuSelectNum--;
                            stopSelect = (LcdStopSelect)menuSelectNum;
                        }
                        else if (menuSelectNum < 2 && moveSelect == LcdMove.Down) {
                            CursorDown();
                            menuSelectNum++;
                            stopSelect = (LcdStopSelect)menuSelectNum;
                        }
                        else if (moveSelect == LcdMove.Apply) {
                            if (menuSelectNum == 0) {
                                RecordContinue();
                                menuSelect = LcdMenuSelect.Record;
                                CursorDown();
                            }
                            else if (menuSelectNum == 1) {
                                RecordEnd();
                                menuSelect = LcdMenuSelect.Main;
                                _vectorTexture = s_defaultVectorTexture;
                            }
                            else if (menuSelectNum == 2) {
                                menuSelect = LcdMenuSelect.Discard;
                                discardSelect = LcdDiscardSelect.Cancel;
                                CursorUp();
                            }
                        }
                        moveSelect = LcdMove.None;

                    }
                    break;
                case LcdMenuSelect.List: {
                        if (listSelectNum > 0 && moveSelect == LcdMove.Up) {
                            if (listSelectNum <= 4) {
                                CursorUp();
                            }
                            listSelectNum--;
                        }
                        else if (listSelectNum < _iniKeysPaths.Count() && moveSelect == LcdMove.Down) {
                            if (listSelectNum < 4) {
                                CursorDown();
                            }
                            listSelectNum++;
                        }
                        else if (moveSelect == LcdMove.Apply) {
                            if (listSelectNum == 0) {
                                menuSelect = LcdMenuSelect.Main;
                                CursorDown();
                            }
                            else {
                                waypointCount = 0;
                                distance = 0;
                                pathName = _iniKeysPaths[listSelectNum - 1].Name;
                                waypointList = _ini.Get(IniSectionPath, pathName).ToString().Split(';');
                                var vectorStart = new Vector3D(0, 0, 0);
                                foreach (string waypoint in waypointList) {
                                    if (waypoint != "") {
                                        waypointCount++;
                                    }
                                    var vector = GetVector3D(waypoint);
                                    if (vectorStart.X != 0 && vector.X != 0) {
                                        distance += Vector3D.Distance(vectorStart, vector);
                                    }
                                    if (vector.X != 0) {
                                        vectorStart = vector;
                                    }
                                }
                                menuSelect = LcdMenuSelect.Path;
                                _vectorTexture = s_defaultVectorTexture;
                                CursorDown();
                                CursorDown();
                                pathSelect = LcdPathSelect.ReplaceInRemote;
                            }
                        }
                        moveSelect = LcdMove.None;
                    }
                    break;
                case LcdMenuSelect.Path: {
                        menuSelectNum = (int)pathSelect;
                        if (menuSelectNum > 0 && moveSelect == LcdMove.Up) {
                            if (menuSelectNum <= 2) {
                                CursorUp();
                            }
                            menuSelectNum--;
                            pathSelect = (LcdPathSelect)menuSelectNum;
                        }
                        else if (menuSelectNum < 3 && moveSelect == LcdMove.Down) {
                            if (menuSelectNum < 2) {
                                CursorDown();
                            }
                            menuSelectNum++;
                            pathSelect = (LcdPathSelect)menuSelectNum;
                        }
                        else if (moveSelect == LcdMove.Apply) {
                            if (menuSelectNum == 0) {
                                PathReplaceInRemote();
                                menuSelect = LcdMenuSelect.Main;
                                _vectorTexture = s_defaultVectorTexture;
                                CursorDown();
                            }
                            else if (menuSelectNum == 1) {
                                PathAddToRemote();
                                menuSelect = LcdMenuSelect.Main;
                                _vectorTexture = s_defaultVectorTexture;
                                CursorDown();
                            }
                            else if (menuSelectNum == 2) {
                                menuSelect = LcdMenuSelect.Delete;
                                deleteSelect = LcdDeleteSelect.Cancel;
                                CursorUp();
                            }
                            else if (menuSelectNum == 3) {
                                menuSelect = LcdMenuSelect.List;
                                listSelectNum = 0;
                                _vectorTexture = s_defaultVectorTexture;
                                menuSelectNum = 0;
                            }
                        }
                        moveSelect = LcdMove.None;

                    }
                    break;
                case LcdMenuSelect.Remote: {
                        menuSelectNum = (int)remoteSelect;
                        if (menuSelectNum > 0 && moveSelect == LcdMove.Up) {
                            CursorUp();
                            menuSelectNum--;
                            remoteSelect = (LcdRemoteSelect)menuSelectNum;
                        }
                        else if (menuSelectNum < 3 && moveSelect == LcdMove.Down) {
                            CursorDown();
                            menuSelectNum++;
                            remoteSelect = (LcdRemoteSelect)menuSelectNum;
                        }
                        else if (moveSelect == LcdMove.Apply) {
                            if (menuSelectNum == 0) {
                                ReverseWaypoints();
                                menuSelect = LcdMenuSelect.Main;
                                CursorDown();
                                CursorDown();
                            }
                            else if (menuSelectNum == 1) {
                                _remote.ClearWaypoints();
                                menuSelect = LcdMenuSelect.Main;
                                CursorDown();
                            }
                            else if (menuSelectNum == 2) {
                                CreatePathFromRemote();
                                menuSelect = LcdMenuSelect.Main;
                            }
                            else if (menuSelectNum == 3) {
                                menuSelect = LcdMenuSelect.Main;
                                CursorUp();
                            }
                        }
                        moveSelect = LcdMove.None;
                    }
                    break;
                case LcdMenuSelect.Delete: {
                        menuSelectNum = (int)deleteSelect;
                        if (menuSelectNum > 0 && moveSelect == LcdMove.Up) {
                            CursorUp();
                            menuSelectNum--;
                            deleteSelect = (LcdDeleteSelect)menuSelectNum;
                        }
                        else if (menuSelectNum < 1 && moveSelect == LcdMove.Down) {
                            CursorDown();
                            menuSelectNum++;
                            deleteSelect = (LcdDeleteSelect)menuSelectNum;
                        }
                        else if (moveSelect == LcdMove.Apply) {
                            if (menuSelectNum == 0) {
                                PathDelete();
                                menuSelect = LcdMenuSelect.List;
                                listSelectNum = 0;
                                _vectorTexture = s_defaultVectorTexture;
                            }
                            else if (menuSelectNum == 1) {
                                menuSelect = LcdMenuSelect.Path;
                                _vectorTexture = s_defaultVectorTexture;
                                CursorDown();
                                CursorDown();
                                pathSelect = LcdPathSelect.ReplaceInRemote;
                            }
                        }
                        moveSelect = LcdMove.None;
                    }
                    break;
                case LcdMenuSelect.Discard: {
                        menuSelectNum = (int)discardSelect;
                        if (menuSelectNum > 0 && moveSelect == LcdMove.Up) {
                            CursorUp();
                            menuSelectNum--;
                            discardSelect = (LcdDiscardSelect)menuSelectNum;
                        }
                        else if (menuSelectNum < 1 && moveSelect == LcdMove.Down) {
                            CursorDown();
                            menuSelectNum++;
                            discardSelect = (LcdDiscardSelect)menuSelectNum;
                        }
                        else if (moveSelect == LcdMove.Apply) {
                            if (menuSelectNum == 0) {
                                PathDiscard();
                                menuSelect = LcdMenuSelect.Main;
                                mainSelect = LcdMainSelect.Record;
                                _vectorTexture = s_defaultVectorTexture;
                            }
                            else if (menuSelectNum == 1) {
                                menuSelect = LcdMenuSelect.Stop;
                                _vectorTexture = s_defaultVectorTexture;
                                CursorDown();
                                CursorDown();
                                stopSelect = LcdStopSelect.Continue;
                            }
                        }
                        moveSelect = LcdMove.None;
                    }
                    break;
                case LcdMenuSelect.Settings: {
                        menuSelectNum = (int)settingSelect;
                        if (menuSelectNum > 0 && moveSelect == LcdMove.Up) {
                            CursorUp();
                            menuSelectNum--;
                            settingSelect = (LcdSettingSelect)menuSelectNum;
                        }
                        else if (menuSelectNum < 2 && moveSelect == LcdMove.Down) {
                            CursorDown();
                            menuSelectNum++;
                            settingSelect = (LcdSettingSelect)menuSelectNum;
                        }
                        else if (moveSelect == LcdMove.Apply) {
                            if (menuSelectNum == 0) {
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
                            else if (menuSelectNum == 1) {
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
                            else if (menuSelectNum == 2) {
                                menuSelect = LcdMenuSelect.Main;
                                CursorUp();
                                Save();
                                Load();
                            }
                        }
                        moveSelect = LcdMove.None;
                    }
                    break;
                case LcdMenuSelect.RenameWarning: {
                        if (moveSelect == LcdMove.Apply || moveSelect == LcdMove.Down || moveSelect == LcdMove.Up) {
                            menuSelect = LcdMenuSelect.Main;
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
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "List of Paths";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Remote Control";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Settings";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
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
            _spriteText.Data = $"Waypoints recorded: {waypointCount}";
            frame.Add(_spriteText);
            frame.Add(_spriteTexture);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Total distance: {distance:0.0}m";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap * 2;
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
            _spriteText.Data = $"Waypoints: {waypointCount}";
            frame.Add(_spriteText);
            frame.Add(_spriteTexture);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Distance: {distance:0.0}m";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Continue";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Save";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
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
                positionText += s_defaultVectorGap;
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
            _spriteText.Data = $"Name: {pathName}";
            frame.Add(_spriteText);
            frame.Add(_spriteTexture);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"WP: {waypointCount}, Distance: {distance:0.0}m";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Replace in Remote";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Add to Remote";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "DELETE";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
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
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Clear waypoints";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"New path from waypoints";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
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
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"{pathName} ?";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Yes";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
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
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"discard recorded path?";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Yes";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
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
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = $"Straights:{_recorder.DistanceOnStraights}, Turning:{_recorder.DistanceInTurns}";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Distance on straights +";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "Distance while turning +";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
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
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            if (_iniKeysPaths.Count == 0) {
                _spriteText.Data = "Record some paths, then";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap;
                _spriteText.Position = positionText;
            }
            _spriteText.Data = "check custom data";
            frame.Add(_spriteText);
            positionText += s_defaultVectorGap;
            _spriteText.Position = positionText;
            _spriteText.Data = "of programable block";
            frame.Add(_spriteText);
        }


    }

}