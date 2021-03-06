﻿using KnowledgeRepresentationReasoning.World;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;

namespace KnowledgeRepresentationInterface.Views.ScenarioControls
{
    /// <summary>
    /// Interaction logic for AddedAction.xaml
    /// </summary>
    public partial class AddedAction : UserControl, ISceControl, INotifyPropertyChanged
    {
        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private int _time;

        public int Time
        {
            get { return _time; }
            set
            {
                _time = value;
                OnPropertyChanged("Time");
            }
        }

        private string _actionName;

        public string ActionName
        {
            get { return _actionName; }
            set { _actionName = value;
                OnPropertyChanged("ActionName"); }
        }


        private List<Fluent> _fluents;

        public List<Fluent> Fluents
        {
            get { return _fluents; }
            set { _fluents = value;
            ShowFluents();
            }
        }
        



        public AddedAction()
        {
            InitializeComponent();
           
        }

        private void ShowFluents()
        {
            if (_fluents != null)
            {
                foreach (var item in _fluents)
                {
                    LabelFluents.Content += item.Name + ": " + (item.Value ? "T" : "F") + ", ";
                }
            }
        }

        public void CleanValues()
        {
            throw new NotImplementedException();
        }
    }

}
