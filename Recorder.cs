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
            List<Vector3D> vectorsRecorded = new List<Vector3D>();
            public List<Vector3D> VectorsRecorded { get { return vectorsRecorded; } }

            public Recorder(Program program) {
                _program = program;
            }
            public void StartRecording(string wayName) {
                vectorsRecorded.Clear();
                _vectorStart = _program._remote.GetPosition();
                _wayName = wayName;
                vectorsRecorded.Add(_vectorStart);
                _program._waypointCount++;
            }
            public void CheckDistance() {
                _vectorEnd = _program._remote.GetPosition();
                _distance = Vector3D.Distance(_vectorStart, _vectorEnd);
                _moveIndicator = _program._cockpit.MoveIndicator;
                _rotationIndicator = _program._cockpit.RotationIndicator;
                _rollIndicator = _program._cockpit.RollIndicator;
                if (_moveIndicator.X != 0 || _moveIndicator.Y != 0 || _rotationIndicator.X != 0 || _rotationIndicator.Y != 0 || _rollIndicator != 0) {
                    _isTurning = true;
                }
                if ((_distance > _distanceInTurns && _isTurning) || _distance > _distanceOnStraights) {
                    _program._distance += _distance;
                    vectorsRecorded.Add(_vectorEnd);
                    _vectorStart = _vectorEnd;
                    _program._waypointCount++;
                    _isTurning = false;
                }

            }
            public void SaveWay() {
                _vectorEnd = _program._remote.GetPosition();
                _distance = Vector3D.Distance(_vectorStart, _vectorEnd);
                vectorsRecorded.Add(_vectorEnd);
                _program._waypointCount++;
                _program._distance += _distance;
            }
            public void DiscardWay() {
                vectorsRecorded.Clear();
            }

        }
    }
}
