﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using TikzGraphGen.Visualization;
using static TikzGraphGen.EdgeLineStyle;
using static TikzGraphGen.Visualization.TikzDrawingWindow;

namespace TikzGraphGen
{
    public class RoutedShortcutCommand
    {
        public static readonly string UNSAVED_CLOSE_WARNING = "Yout are about to close the current file with unsaved changes.";
        public static readonly string CONFIRMATION = "Close without saving?";

        private SelectedTool _currentTool;
        public SelectedTool CurrentTool {
            get { return _currentTool; }
            set { _currentTool = value; CurrentToolChanged(_currentTool); }
        }
        private Concentration _lineConcentration;
        public Concentration LineConcentration
        {
            get { return _lineConcentration; }
            set { _lineConcentration = value; LineConcentrationChanged(_lineConcentration); }
        }
        private LineStyle _lineStyle;
        public LineStyle LineStyle
        {
            get { return _lineStyle; }
            set { _lineStyle = value; LineStyleChanged(_lineStyle); }
        }
        private VertexBorderStyle.BorderStyle _borderStyle;
        public VertexBorderStyle.BorderStyle BorderStyle
        {
            get { return _borderStyle; }
            set { _borderStyle = value; BorderStyleChanged(_borderStyle); }
        }

        private readonly ToolSettingDictionary _toolInfo;
        public ToolSettingDictionary ToolInfo//TODO: Update drawing window and menustrip to use this event
        {
            get { return _toolInfo; }
        }

        private bool _canUndo;
        public bool CanUndo
        {
            get { return _canUndo; }
            set { _canUndo = value; CanUndoStatusChanged(_canUndo); }
        }
        private bool _canRedo;
        public bool CanRedo
        {
            get { return _canRedo; }
            set { _canRedo = value; CanRedoStatusChanged(_canRedo); }
        }

        public event CurrentTool_Change CurrentToolChanged;
        public event Concentration_Change LineConcentrationChanged;
        public event LineStyle_Change LineStyleChanged;
        public event BorderStyle_Change BorderStyleChanged;
        public event ToolInfo_Change ToolInfoChanged;
        public event CanUndo_Change CanUndoStatusChanged;
        public event CanRedo_Change CanRedoStatusChanged;

        public RoutedShortcutCommand()
        {
            _currentTool = SelectedTool.Vertex;
            _lineConcentration = Concentration.Regular;
            _lineStyle = LineStyle.Solid;
            _borderStyle = VertexBorderStyle.BorderStyle.Circle;

            _toolInfo = new();
        }

        public GeneralCommand NewProject; //
        public GeneralCommand Open; //
        public GeneralCommand Save; //
        public GeneralCommand SaveAs; //
        public GeneralCommand Export; //
        public GeneralCommand DisplayInfo; //
        public GeneralCommand Quit; //
        public GeneralCommand Undo; //
        public GeneralCommand Redo; //
        public GeneralCommand DisplayHistory; //
        public GeneralCommand DeleteSelected; //
        public GeneralCommand Cut; //
        public GeneralCommand Copy; //
        public GeneralCommand Paste; //
        public GeneralCommand ResizeAll; //
        public GeneralCommand SetUnits; //
        public GeneralCommand FontOptions; //
        public GeneralCommand Preferences; //
        public GeneralCommand ZoomInc; //
        public GeneralCommand ZoomDec; //
        public GeneralCommand ZoomFit; //
        public GeneralCommand ToggleBorder; //
        public GeneralCommand ToggleFullscreen; //
        public GeneralCommand ToggleAngleSnap; //
        public GeneralCommand ToggleUnitSnap; //
        public GeneralCommand ToggleGridUnitSnap; //
        public GeneralCommand ToggleUnitGrid; //
        public GeneralCommand ToggleLabelEdgeSnap; //
        public GeneralCommand ToggleMenu; //
        public GeneralCommand ToggleTools; //
        public GeneralCommand Guide; //
        public GeneralCommand ContextHelp; // <- Have TikzWindow overlay a large transparent window,
        //which gets the position of the mouse when clicking and opens the guide to the relevent information of the object at those coordinates in the rest of TikzWindow
        public GeneralCommand About; //
        public GeneralCommand SelectAll; //

        public static string SavePrompt() //Retry = save, Abort = continue, Ignore = cancel
        {
            TaskDialogPage display = new()
            {
                AllowCancel = true,
                AllowMinimize = false,
                Buttons = new TaskDialogButtonCollection() { new(TikzWindow.NO_SAVE_QUIT_DIALOG), new(TikzWindow.SAVE_QUIT_DIALOG), new(TikzWindow.CANCEL_QUIT_DIALOG) },
                Caption = CONFIRMATION,
                Heading = UNSAVED_CLOSE_WARNING
            };
            display.DefaultButton = display.Buttons[2];
            return TaskDialog.ShowDialog(display).Text;
        }

        public void UpdateToolInfo(object newData)
        {
            switch(newData)
            {
                case ToolSettingDictionary.VertexToolInfo vertex:
                    _toolInfo.VertexInfo = vertex;
                    break;
                case ToolSettingDictionary.EdgeToolInfo edge:
                    _toolInfo.EdgeInfo = edge;
                    break;
                case ToolSettingDictionary.EdgeCapToolInfo edgeCap:
                    _toolInfo.EdgeCapInfo = edgeCap;
                    break;
                case ToolSettingDictionary.LabelToolInfo label:
                    _toolInfo.LabelInfo = label;
                    break;
                case ToolSettingDictionary.EraserToolInfo eraser:
                    _toolInfo.EraserInfo = eraser;
                    break;
                case ToolSettingDictionary.TransformToolInfo transform:
                    _toolInfo.TransformInfo = transform;
                    break;
                /*case ToolSettingDictionary.SelectToolInfo select:
                    _toolInfo.SelectInfo = select;
                    break;
                case ToolSettingDictionary.AreaSelectToolInfo areaSelect:
                    _toolInfo.AreaSelectInfo = areaSelect;
                    break;
                case ToolSettingDictionary.LassoToolInfo lasso:
                    _toolInfo.LassoInfo = lasso;
                    break;*/
                case ToolSettingDictionary.WeightToolInfo weight:
                    _toolInfo.WeightInfo = weight;
                    break;
                case ToolSettingDictionary.TrackerToolInfo tracker:
                    _toolInfo.TrackerInfo = tracker;
                    break;
                case ToolSettingDictionary.MergeToolInfo merge:
                    _toolInfo.MergeInfo = merge;
                    break;
                case ToolSettingDictionary.SplitToolInfo split:
                    _toolInfo.SplitInfo = split;
                    break;
                default:
                    throw new NotImplementedException();
            }

            ToolInfoChanged(_toolInfo);
        }

        public delegate void CurrentTool_Change(SelectedTool tool);
        public delegate void Concentration_Change(Concentration conc);
        public delegate void LineStyle_Change(LineStyle style);
        public delegate void BorderStyle_Change(VertexBorderStyle.BorderStyle style);
        public delegate void ToolInfo_Change(ToolSettingDictionary toolInfo);
        public delegate void CanUndo_Change(bool canUndo);
        public delegate void CanRedo_Change(bool canRedo);

        public delegate void GeneralCommand();
    }
}
