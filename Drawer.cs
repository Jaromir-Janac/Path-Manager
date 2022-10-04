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
            static MySprite s_spriteCursor;
            static MySprite s_spriteCircleLG;
            static MySprite s_spriteCircleSM;
            MySprite _spriteText;
            MySprite _spriteCursor;
            MySprite _spriteCircleLG;
            MySprite _spriteCircleSM;
            Vector2 _vectorText;
            Vector2 _vectorPositionCompass;
            static Vector2 s_vectorGap = new Vector2(0, 1);
            static Vector2 s_vectorGapCompass = new Vector2(185, 25);
            public Drawer(Program program) {
                _program = program;
                s_spriteText = program._sprites.SpriteText;
                s_spriteCursor = program._sprites.SpriteCursor;
                s_spriteCircleLG = program._sprites.SpriteCircleLG;
                s_spriteCircleSM = program._sprites.SpriteCircleSM;
                _vectorText = program._vectorText;
            }
            public void DrawMainMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale) {
                var positionText = (_vectorText + viewport.Position + s_vectorGap) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position + s_vectorGap;
                _spriteText = s_spriteText;
                _spriteCursor = s_spriteCursor;
                _spriteCursor.Position = positionTexture * scale;
                _spriteCursor.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = "Start Recording";
                frame.Add(_spriteText);
                frame.Add(_spriteCursor);
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
                var positionText = (_vectorText + viewport.Position + s_vectorGap) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position + s_vectorGap;
                _spriteText = s_spriteText;
                _spriteCursor = s_spriteCursor;
                _spriteCursor.Position = positionTexture * scale;
                _spriteCursor.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"WP recorded: {_program._waypointCount}";
                frame.Add(_spriteText);
                frame.Add(_spriteCursor);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"Distance: {_program._distance:0.0}m";
                frame.Add(_spriteText);
                positionText += (s_defaultVectorGap * 2) * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Pause Recording";
                frame.Add(_spriteText);
            }
            public void DrawStopMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale, Vector2 vectorText) {
                var positionText = (vectorText + viewport.Position + s_vectorGap) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position + s_vectorGap;
                _spriteText = s_spriteText;
                _spriteCursor = s_spriteCursor;
                _spriteCircleLG = s_spriteCircleLG;
                _spriteCircleSM = s_spriteCircleSM;
                _vectorPositionCompass = _program._vector2Compass * 20;
                if (_program._vectorNormal.Z <= 0) {
                    _spriteCircleSM.Data = "Circle";
                }
                else {
                    _spriteCircleSM.Data = "CircleHollow";
                }
                _spriteCircleLG.Size *= scale;
                _spriteCircleSM.Size *= scale;
                _spriteCircleLG.Position = (positionText + s_vectorGapCompass) * scale;
                _spriteCircleSM.Position = (positionText + s_vectorGapCompass + _vectorPositionCompass) * scale;
                _spriteCursor.Position = positionTexture * scale;
                _spriteCursor.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Waypoints: {_program._waypointCount}";
                frame.Add(_spriteText);
                frame.Add(_spriteCursor);
                frame.Add(_spriteCircleLG);
                frame.Add(_spriteCircleSM);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = $"Distance: {_program._distance:0}m";
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
                _spriteText.Data = "Delete last WP";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Discard";
                frame.Add(_spriteText);
            }
            public void DrawListMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale, Vector2 vectorText) {
                var positionText = (vectorText + viewport.Position + s_vectorGap) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position + s_vectorGap;
                _spriteText = s_spriteText;
                _spriteCursor = s_spriteCursor;
                _spriteCursor.Position = positionTexture * scale;
                _spriteCursor.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Back";
                frame.Add(_spriteText);
                frame.Add(_spriteCursor);
                foreach (MyIniKey iniKey in _program._iniKeysPaths) {
                    positionText += s_defaultVectorGap * scale;
                    _spriteText.Position = positionText;
                    _spriteText.Data = iniKey.Name;
                    frame.Add(_spriteText);
                }
            }
            public void DrawPathMenu(ref MySpriteDrawFrame frame, RectangleF viewport, float scale, Vector2 vectorText) {
                var positionText = (vectorText + viewport.Position + s_vectorGap) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position + s_vectorGap;
                _spriteText = s_spriteText;
                _spriteCursor = s_spriteCursor;
                _spriteCursor.Position = positionTexture * scale;
                _spriteCursor.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Name: {_program._pathName}";
                frame.Add(_spriteText);
                frame.Add(_spriteCursor);
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
                var positionText = (_vectorText + viewport.Position + s_vectorGap) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position + s_vectorGap;
                _spriteText = s_spriteText;
                _spriteCursor = s_spriteCursor;
                _spriteCursor.Position = positionTexture * scale;
                _spriteCursor.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Reverse waypoints";
                frame.Add(_spriteText);
                frame.Add(_spriteCursor);
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
                var positionText = (_vectorText + viewport.Position + s_vectorGap) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position + s_vectorGap;
                _spriteText = s_spriteText;
                _spriteCursor = s_spriteCursor;
                _spriteCursor.Position = positionTexture * scale;
                _spriteCursor.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Delete path:";
                frame.Add(_spriteText);
                frame.Add(_spriteCursor);
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
                var positionText = (_vectorText + viewport.Position + s_vectorGap) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position + s_vectorGap;
                _spriteText = s_spriteText;
                _spriteCursor = s_spriteCursor;
                _spriteCursor.Position = positionTexture * scale;
                _spriteCursor.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Do you really want to";
                frame.Add(_spriteText);
                frame.Add(_spriteCursor);
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
                var positionText = (_vectorText + viewport.Position + s_vectorGap) * scale;
                var positionTexture = _program._vectorTexture + viewport.Position + s_vectorGap;
                _spriteText = s_spriteText;
                _spriteCursor = s_spriteCursor;
                _spriteCursor.Position = positionTexture * scale;
                _spriteCursor.Size *= scale;
                _spriteText.Position = positionText;
                _spriteText.RotationOrScale *= scale;
                _spriteText.Data = $"Distance between WP";
                frame.Add(_spriteText);
                frame.Add(_spriteCursor);
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
            public void DrawWarningRename(ref MySpriteDrawFrame frame, RectangleF viewport, float scale) {
                var positionText = (_vectorText + viewport.Position + s_vectorGap) * scale;
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
            public void DrawWarningRemote(ref MySpriteDrawFrame frame, RectangleF viewport, float scale) {
                var positionText = (_vectorText + viewport.Position + s_vectorGap) * scale;
                _spriteText = s_spriteText;
                _spriteText.Position = positionText;
                _spriteText.Data = "Remote Control NOT found";
                _spriteText.RotationOrScale *= scale;
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "Add one to your grid";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "and recompile the script";
                frame.Add(_spriteText);
                positionText += s_defaultVectorGap * scale;
                _spriteText.Position = positionText;
                _spriteText.Data = "for this feature to work";
                frame.Add(_spriteText);
            }
        }
    }
}
