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
        public class Drawer {
            Program _program;
            static MySprite s_spriteText;
            static MySprite s_spriteTexture;
            MySprite _spriteText;
            MySprite _spriteTexture;
            Vector2 _vectorText;
            public Drawer(Program program) {
                _program = program;
                s_spriteText = program._sprites.SpriteText;
                s_spriteTexture = program._sprites.SpriteTexture;
                _vectorText = program._vectorText;
            }
            public void DrawMainMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale) {
                var positionText = (_vectorText + viewport.Position) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position;
                _spriteText = s_spriteText;
                _spriteTexture = s_spriteTexture;
                _spriteTexture.Position = positionTexture * scale;
                _spriteTexture.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = "Start Recording";
                frame.Add(_spriteText);
                frame.Add(_spriteTexture);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "List of Paths";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Remote Control";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Settings";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Rename Paths";
                frame.Add(_spriteText);
            }
            public void DrawRecordMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale) {
                var positionText = (_vectorText + viewport.Position) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position;
                _spriteText = s_spriteText;
                _spriteTexture = s_spriteTexture;
                _spriteTexture.Position = positionTexture * scale;
                _spriteTexture.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"WP recorded: {_program._waypointCount}";
                frame.Add(_spriteText);
                frame.Add(_spriteTexture);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"Distance: {_program._distance:0.0}m";
                frame.Add(_spriteText);
                positionText += (s_defaultVectorGap * 2) * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Pause Recording";
                frame.Add(_spriteText);
            }
            public void DrawStopMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale) {
                var positionText = (_vectorText + viewport.Position) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position;
                _spriteText = s_spriteText;
                _spriteTexture = s_spriteTexture;
                _spriteTexture.Position = positionTexture * scale;
                _spriteTexture.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Waypoints: {_program._waypointCount}";
                frame.Add(_spriteText);
                frame.Add(_spriteTexture);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"Distance: {_program._distance:0.0}m";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Continue";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Save";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Discard";
                frame.Add(_spriteText);
            }
            public void DrawListMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale, Vector2 vectorText) {
                var positionText = (vectorText + viewport.Position) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position;
                _spriteText = s_spriteText;
                _spriteTexture = s_spriteTexture;
                _spriteTexture.Position = positionTexture * scale;
                _spriteTexture.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Back";
                frame.Add(_spriteText);
                frame.Add(_spriteTexture);
                foreach (MyIniKey iniKey in _program._iniKeysPaths) {
                    positionText += s_defaultVectorGap * scale;
                    _spriteText.Position = positionText;
                    _spriteText.Data = iniKey.Name;
                    frame.Add(_spriteText);
                }
            }
            public void DrawPathMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale) {
                var positionText = (_vectorText + viewport.Position) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position;
                _spriteText = s_spriteText;
                _spriteTexture = s_spriteTexture;
                _spriteTexture.Position = positionTexture * scale;
                _spriteTexture.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Name: {_program._pathName}";
                frame.Add(_spriteText);
                frame.Add(_spriteTexture);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"WP:{_program._waypointCount},Distance:{_program._distance:0.0}m";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Replace in Remote";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Add to Remote";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "DELETE";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Back";
                frame.Add(_spriteText);
            }
            public void DrawRemoteMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale) {
                var positionText = (_vectorText + viewport.Position) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position;
                _spriteText = s_spriteText;
                _spriteTexture = s_spriteTexture;
                _spriteTexture.Position = positionTexture * scale;
                _spriteTexture.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Reverse waypoints";
                frame.Add(_spriteText);
                frame.Add(_spriteTexture);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"Clear waypoints";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"New path from WP";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"Back";
                frame.Add(_spriteText);
            }
            public void DrawDeleteMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale) {
                var positionText = (_vectorText + viewport.Position) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position;
                _spriteText = s_spriteText;
                _spriteTexture = s_spriteTexture;
                _spriteTexture.Position = positionTexture * scale;
                _spriteTexture.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Delete path:";
                frame.Add(_spriteText);
                frame.Add(_spriteTexture);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"{_program._pathName} ?";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Yes";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Cancel";
                frame.Add(_spriteText);
            }
            public void DrawDiscardMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale) {
                var positionText = (_vectorText + viewport.Position) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position;
                _spriteText = s_spriteText;
                _spriteTexture = s_spriteTexture;
                _spriteTexture.Position = positionTexture * scale;
                _spriteTexture.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Do you really want to";
                frame.Add(_spriteText);
                frame.Add(_spriteTexture);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"discard recorded path?";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"Yes";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"Cancel";
                frame.Add(_spriteText);
            }
            public void DrawSettingsMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale) {
                var positionText = (_vectorText + viewport.Position) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position;
                _spriteText = s_spriteText;
                _spriteTexture = s_spriteTexture;
                _spriteTexture.Position = positionTexture * scale;
                _spriteTexture.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Distance between WP";
                frame.Add(_spriteText);
                frame.Add(_spriteTexture);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"Straights:{_program._recorder.DistanceOnStraights}m,Turns:{_program._recorder.DistanceInTurns}m";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Distance on straights +";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Distance while turning +";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Back";
                frame.Add(_spriteText);
            }
            public void DrawRenameWarning(ref MySpriteDrawFrame frame, RectangleF viewport, float scale) {
                var positionText = (_vectorText + viewport.Position) * scale;
                _spriteText = s_spriteText;
                _spriteText.Position = positionText;
                _spriteText.Data = "Nothing to rename";
                _spriteText.RotationOrScale *= scale;
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                if (_program._iniKeysPaths.Count == 0) {
                    _spriteText.Data = "Record some paths,";
                    frame.Add(_spriteText);
                    positionText += s_defaultVectorGap * scale;
                    _spriteText.Position = positionText;
                }
                _spriteText.Data = "check custom data";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "of programable block";
                frame.Add(_spriteText);
            }
        }
    }
}
