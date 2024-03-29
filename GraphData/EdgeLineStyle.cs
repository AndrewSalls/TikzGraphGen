﻿using System.Drawing;
using static TikzGraphGen.ToolSettingDictionary;

namespace TikzGraphGen
{
    public class EdgeLineStyle
    {
        public enum Concentration
        {
            Loose, Regular, Dense
        }
        public enum LineStyle
        {
            Solid, Dash, Dot, DotDash, DashDot, DashDotDot, None
        }
        public enum EdgeCapShape
        {
            Latex, Stealth, Triangle, OpenTriangle, Angle, Hook, Backet, Parenthesis, Circle, Star, Diamond, OpenDiamond, Serif,
            LeftTo, LeftHook, RightTo, RightHook, RoundCap, ButtCap, TriangleCap, FastCap, None
        }

        private EdgeToolInfo _lineInfo;
        public EdgeToolInfo LineInfo { get { return _lineInfo; } set { _lineInfo = value; } }
        public EdgeCapToolInfo SDirectionCap { get; set; }
        public EdgeCapToolInfo DDirectionCap { get; set; }

        public EdgeLineStyle()
        {
            _lineInfo = new EdgeToolInfo
            {
                Style = LineStyle.None,
                Color = Color.Transparent,
                Density = Concentration.Regular,
                DashWidth = 0,
                DashSpacing = 0,
                PatternOffset = 0,
                Thickness = 0
            };
            SDirectionCap = new EdgeCapToolInfo
            {
                IsReversed = false,
                IsThick = false,
                TriangleDegree = 90,
                Style = EdgeCapShape.None
            };
            DDirectionCap = new EdgeCapToolInfo
            {
                IsReversed = false,
                IsThick = false,
                TriangleDegree = 90,
                Style = EdgeCapShape.None
            };
        }
        public EdgeLineStyle(EdgeToolInfo line, EdgeCapToolInfo sCap, EdgeCapToolInfo tCap)
        {
            LineInfo = line;
            SDirectionCap = sCap;
            DDirectionCap = tCap;
        }
    }
}
