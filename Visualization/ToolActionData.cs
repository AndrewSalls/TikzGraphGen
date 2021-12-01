using System;
using System.Collections.Generic;
using System.Drawing;
using TikzGraphGen.GraphData;

namespace TikzGraphGen.Visualization
{
    public class ToolActionData
    {
        public Coord Corner { get; set; }
        public Graph Selected { get; set; }
        public Point? MouseDown { get; set; }
        public Point MouseDrag { get; set; }
        public bool Dragging { get; set; }
        public Vertex FirstVertex { get; set; }
        public List<PointF> LassoPoints { get; private set; }

        public bool DrawBorder { get; set; }
        public bool DrawUnitGrid { get; set; }
        public bool SnapToAngle { get; set; }
        public float AngleSnapAmt { get; set; } // Must be between 0 and 2pi
        public bool SnapToUnit { get; set; }
        public bool SnapToUnitGrid { get; set; }
        public bool LabelSnapToEdge { get; set; }

        public ToolActionData(Coord cornerPos, Point? mDown, Point mDrag, bool isDragging, Vertex first, List<PointF> lasso, RoutedShortcutCommand rsc)
        {
            Corner = cornerPos;
            MouseDown = mDown;
            MouseDrag = mDrag;
            Dragging = isDragging;
            FirstVertex = first;
            LassoPoints = lasso;

            DrawBorder = false;
            DrawUnitGrid = false;
            SnapToAngle = true;
            AngleSnapAmt = MathF.PI / 12f;
            SnapToUnit = false;
            SnapToUnitGrid = false;
            Selected = null;

            rsc.ToggleBorder = () => { DrawBorder = !DrawBorder; rsc.RefreshWindow(); };
            rsc.ToggleAngleSnap = () => { SnapToAngle = !SnapToAngle; rsc.RefreshWindow(); };
            rsc.ToggleUnitSnap = () => { SnapToUnit = !SnapToUnit; rsc.RefreshWindow(); };
            rsc.ToggleGridUnitSnap = () => { SnapToUnitGrid = !SnapToUnitGrid; rsc.RefreshWindow(); };
            rsc.ToggleUnitGrid = () => { DrawUnitGrid = !DrawUnitGrid; rsc.RefreshWindow(); };
            rsc.ToggleLabelEdgeSnap = () => { LabelSnapToEdge = !LabelSnapToEdge; rsc.RefreshWindow(); };
        }
        public ToolActionData(int screenWidth, RoutedShortcutCommand rsc) : this(new Coord((screenWidth - DrawingWindowRenderer.PAGE_SIZE.Width) / 2, 0), null, new Point(0, 0), false, null, new List<PointF>(), rsc) {}

    public void Reset()
        {
            FirstVertex = null;
            Dragging = false;
            MouseDown = null;
            LassoPoints.Clear();
        }
    }
}
