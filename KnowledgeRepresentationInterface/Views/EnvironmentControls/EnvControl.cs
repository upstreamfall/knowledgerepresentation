﻿using KnowledgeRepresentationReasoning.World.Records;
using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace KnowledgeRepresentationInterface.Views.EnvironmentControls
{
    using KnowledgeRepresentationReasoning.World;

    public abstract class EnvControl : UserControl
    {
        public abstract WorldDescriptionRecord GetWorldDescriptionRecord();

        public abstract void CleanValues();

        protected virtual bool ParseAction(int selectedIndex, out string errorInfo)
        {
            if (selectedIndex == -1)
            {
                errorInfo = "Please select an action.";
                return false;
            }
            errorInfo = "";
            return true;
        }

        protected virtual bool ParseAction(string actionText, out WorldAction worldAction, out string errorInfo)
        {
            string actionString = actionText;
            worldAction = new WorldAction();
            errorInfo = "";
            try
            {
                var act = actionString.Replace("(", "").Replace(")", "").Split(',');

                worldAction.Id = act[0];
                worldAction.Duration = Int32.Parse(act[1]);
                return true;
            }
            catch (Exception e)
            {
                errorInfo = "Action in a wrong format!";
                return false;
            }
        }

        protected virtual bool ParseExpression(string expressionText, out string expression, out string errorInfo)
        {
            expression = expressionText;
            errorInfo = "";
            if (expression == "")
            {
                errorInfo = "All fields are reqiured";
                return false;
            }
            return true;
        }

        protected virtual bool ParseTimeLenght(int? value, out string errorInfo)
        {
            if (!value.HasValue)
            {
                errorInfo = "You must specify the time.";
                return false;
            }
            errorInfo = "";
            return true;
        }

        protected virtual bool ParseTimeLenght(string timeText, out int timeLenght, out string errorInfo)
        {
            timeLenght = 0;
            errorInfo = "";
            try
            {
                timeLenght = Int32.Parse(timeText);
                return true;
            }
            catch (Exception)
            {
                errorInfo = "Wrong number format";
                return false;
            }
        }
    }
}
