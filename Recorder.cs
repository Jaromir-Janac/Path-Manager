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
            double distance;
            double distanceOnStraights = 250;
            double distanceInTurns = 30;
            string wayName;
            Vector3 _moveIndicator;
            Vector2 _rotationIndicator;
            float rollIndicator;
            bool isTurning = false;
            public double DistanceOnStraights {
                get { return distanceOnStraights; }
                set { distanceOnStraights = value; }
            }
            public double DistanceInTurns {
                get { return distanceInTurns; }
                set { distanceInTurns = value; }
            }
            public string WayName { get { return wayName; } }
            List<Vector3D> vectorsRecorded = new List<Vector3D>();
            public List<Vector3D> VectorsRecorded { get { return vectorsRecorded; } }

            public Recorder(Program program) {
                _program = program;
            }
            public void StartRecording(string wayName) {
                vectorsRecorded.Clear();
                _vectorStart = _program._remote.GetPosition();
                this.wayName = wayName;
                vectorsRecorded.Add(_vectorStart);
                _program.waypointCount++;
            }
            public void CheckDistance() {
                _vectorEnd = _program._remote.GetPosition();
                distance = Vector3D.Distance(_vectorStart, _vectorEnd);
                _moveIndicator = _program._cockpit.MoveIndicator;
                _rotationIndicator = _program._cockpit.RotationIndicator;
                rollIndicator = _program._cockpit.RollIndicator;
                if (_moveIndicator.X != 0 || _moveIndicator.Y != 0 || _rotationIndicator.X != 0 || _rotationIndicator.Y != 0 || rollIndicator != 0) {
                    isTurning = true;
                }
                if ((distance > distanceInTurns && isTurning) || distance > distanceOnStraights) {
                    _program.distance += distance;
                    vectorsRecorded.Add(_vectorEnd);
                    _vectorStart = _vectorEnd;
                    _program.waypointCount++;
                    isTurning = false;
                }

            }
            public void SaveWay() {
                _vectorEnd = _program._remote.GetPosition();
                distance = Vector3D.Distance(_vectorStart, _vectorEnd);
                vectorsRecorded.Add(_vectorEnd);
                _program.waypointCount++;
                _program.distance += distance;
            }
            public void DiscardWay() {
                vectorsRecorded.Clear();
            }

        }
    }
}
