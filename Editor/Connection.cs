﻿using System;
using UnityEditor;
using UnityEngine;

namespace SAS.StateMachineGraph.Editor
{
    public class Connection
    {
        public BaseNode StartNode { get; }
        public BaseNode EndNode { get; }

        public StateModel SourceStateModel { get; }
        public StateModel TargetStateModel { get; }

        private Vector2 _startPos;
        private Vector2 _endPos;

        private Action<Connection> _removeConnection;
        RuntimeStateMachineController _runtimeStateMachineController;

        public Connection(RuntimeStateMachineController runtimeStateMachineController, BaseNode start, BaseNode end, StateModel sourceStateModel, StateModel targetStateModel, Action<Connection> removeConnection)
        {
            StartNode = start;
            EndNode = end;
            SourceStateModel = sourceStateModel;
            TargetStateModel = targetStateModel;
            _runtimeStateMachineController = runtimeStateMachineController;
            _removeConnection = removeConnection;
        }

       

        private void DrwaConnection()
        {
           
            bool inverted = false;
            if (StartNode.Position.y < EndNode.Position.y)
            {
                _startPos = StartNode.startPort.rect.center;
                _endPos = EndNode.startPort.rect.center;
            }
            else
            {
                _startPos = EndNode.endPort.rect.center;
                _endPos = StartNode.endPort.rect.center;
                inverted = true;
            }

            EditorUtilities.DrawLine(_startPos, _endPos);
            if (SourceStateModel.GetTransitionCount(TargetStateModel) <= 1)
                EditorUtilities.DrawArrow(_startPos, _endPos, inverted);
            else
                EditorUtilities.DrawTrippleArrow(_startPos, _endPos, inverted);
        }

        private static float DistanceToPolyLine(params Vector3[] points)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));
            float dist = HandleUtility.DistanceToLine(points[0], points[1]);
            for (int i = 2; i < points.Length; i++)
            {
                float d = HandleUtility.DistanceToLine(points[i - 1], points[i]);
                if (d < dist)
                    dist = d;
            }
            return dist;
        }

        public void Draw()
        {
            DrwaConnection();
        }

        public void ProcessMouseEvent(Event e)
        {
            ProcessMouseEvent(e, new Vector3[] { _startPos, _endPos });
        }

        private void ProcessMouseEvent(Event e, Vector3[] points)
        {
            if (DistanceToPolyLine(points) < 10)

                switch (e.type)
                {
                    case EventType.MouseDown:

                        if (e.button == 0)
                        {

                            Selection.activeObject = SourceStateModel;
                            StartNode.IsFocused = false;
                            int index = SourceStateModel.GetTransitionStateIndex(TargetStateModel);
                            StateTransitionInspector.Show(index, _runtimeStateMachineController, SourceStateModel.ToSerializedObject());

                        }
                        break;
                    case EventType.MouseUp:
                        if (e.button == 1)
                        {
                            e.Use();
                            ProcessContextMenu(e.mousePosition);
                        }
                        break;

                }
        }

        private void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Clear Connection"), false, () => _removeConnection?.Invoke(this));
            genericMenu.ShowAsContext();
        }
    }
}