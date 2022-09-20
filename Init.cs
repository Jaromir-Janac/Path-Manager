using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript {
    partial class Program {
        public class Init {
            Program _program;
            List<IMyCockpit> _cockpits = new List<IMyCockpit>();
            List<IMyRemoteControl> _remotes = new List<IMyRemoteControl>();
            List<IMyShipController> _shipControllers = new List<IMyShipController>();
            List<IMyTerminalBlock> _allGroupedBlocks = new List<IMyTerminalBlock>();
            public List<IMyShipController> ShipControllers { get { return _shipControllers; } }
            IMyRemoteControl _remoteControl;
            IMyCockpit _cockpit;
            bool _isCockpit = false;
            bool _isRemote = false;
            string _blocksName = "PathManager";
            public string BlocksName { get; set; }
            public Init(Program program) {
                _program = program;
            }
            public List<IMyTerminalBlock> GetMyTerminalBlocks() {
                _allGroupedBlocks.Clear();
                _program.GridTerminalSystem.SearchBlocksOfName(_blocksName, _allGroupedBlocks);
                return _allGroupedBlocks;
            }
            public IMyCockpit GetCockpit() {
                _program.GridTerminalSystem.GetBlocksOfType(_cockpits, cockpit => cockpit.IsSameConstructAs(_program.Me));
                foreach (IMyCockpit cockpit in _cockpits) {
                    _shipControllers.Add(cockpit);
                    if (cockpit.IsUnderControl) {
                        _cockpit = cockpit;
                        _isCockpit = true;
                    }
                }
                if (!_isCockpit && _cockpits.Count > 0) {
                    _cockpit = _cockpits[0];
                    _isCockpit = true;
                }
                if (!_isCockpit) {
                    _program.Echo("No cockpit detected.");
                }
                return _cockpit;
            }
            public IMyRemoteControl GetRemoteControl() {
                _program.GridTerminalSystem.GetBlocksOfType(_remotes, remote => remote.IsSameConstructAs(_program.Me));
                foreach (IMyRemoteControl remote in _remotes) {
                    _shipControllers.Add(remote);
                    if (remote.IsUnderControl) {
                        _remoteControl = remote;
                        _isRemote = true;
                    }
                }
                if (!_isRemote && _remotes.Count > 0) {
                    { _remoteControl = _remotes[0]; }
                }
                else {
                    throw new Exception("\nNo remote control found.\nRecompile me after there is one.\n");
                }
                return _remoteControl;
            }
        }
    }
}
