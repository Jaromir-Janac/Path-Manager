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
        public class Recorder {
            Program _program;
            Vector3D _vectorStart;
            Vector3D _vectorEnd;
            double _distance;
            double _distanceOnStraights = 250;
            double _distanceInTurns = 30;
            string _wayName;
            Vector3 _moveIndicator;
            Vector2 _rotationIndicator;
            float _rollIndicator;
            bool _isTurning = false;
            public double DistanceOnStraights {
                get { return _distanceOnStraights; }
                set { _distanceOnStraights = value; }
            }
            public double DistanceInTurns {
                get { return _distanceInTurns; }
                set { _distanceInTurns = value; }
            }
            public string WayName { get { return _wayName; } }
            List<Vector3D> _vectorsRecorded = new List<Vector3D>();
            public List<Vector3D> VectorsRecorded { get { return _vectorsRecorded; } }

            public Recorder(Program program) {
                _program = program;
            }
            public void StartRecording(string wayName) {
                _vectorsRecorded.Clear();
                _vectorStart = _program.Remote.GetPosition();
                _wayName = wayName;
                _vectorsRecorded.Add(_vectorStart);
                _program.WaypointCount++;
                _program.VectorLastWP = _vectorStart;
            }
            public void DeleteLastWaypoint() {
                _vectorEnd = _vectorsRecorded[_program.WaypointCount];
                _vectorStart = _vectorsRecorded[_program.WaypointCount - 1];
                _program.VectorLastWP = _vectorStart;
                _vectorsRecorded.RemoveAt(_program.WaypointCount);
                if (_program.WaypointCount > 1) {
                    _distance = Vector3D.Distance(_vectorStart, _vectorEnd);
                    _program.Distance -= _distance;
                }
                else {
                    _program.Distance = 0;
                }
            }
            public void CheckDistance() {
                _vectorEnd = _program.Remote.GetPosition();
                _distance = Vector3D.Distance(_vectorStart, _vectorEnd);
                _moveIndicator = _program.Cockpit.MoveIndicator + _program.Remote.MoveIndicator;
                _rotationIndicator = _program.Cockpit.RotationIndicator + _program.Remote.RotationIndicator;
                _rollIndicator = _program.Cockpit.RollIndicator + _program.Remote.RollIndicator;
                if (_moveIndicator.X != 0 || _moveIndicator.Y != 0 || _rotationIndicator.X != 0 || _rotationIndicator.Y != 0 || _rollIndicator != 0) {
                    _isTurning = true;
                }
                if ((_distance > _distanceInTurns && _isTurning) || _distance > _distanceOnStraights) {
                    _program.Distance += _distance;
                    _vectorsRecorded.Add(_vectorEnd);
                    _vectorStart = _vectorEnd;
                    _program.WaypointCount++;
                    _isTurning = false;
                    _program.VectorLastWP = _vectorStart;
                }
            }
            public void SaveWay() {
                _vectorEnd = _program.Remote.GetPosition();
                _distance = Vector3D.Distance(_vectorStart, _vectorEnd);
                _vectorsRecorded.Add(_vectorEnd);
                _program.WaypointCount++;
                _program.Distance += _distance;
            }
            public void DiscardWay() {
                _vectorsRecorded.Clear();
            }

        }
    }
}
