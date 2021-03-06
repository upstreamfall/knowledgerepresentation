﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

using KnowledgeRepresentationInterface.Views.EnvironmentControls;
using KnowledgeRepresentationReasoning.World;
using KnowledgeRepresentationReasoning.World.Records;

namespace KnowledgeRepresentationInterface.Views
{
    /// <summary>
    /// Interaction logic for Environment.xaml
    /// </summary>
    public partial class Environment : UserControl, INotifyPropertyChanged
    {
        #region Properties

        private int _timeInf;
        private ObservableCollection<Fluent> _fluents;
        private ObservableCollection<WorldAction> _actions; 
        private ObservableCollection<WorldDescriptionRecord> _statements;
        private WorldDescriptionRecordType _selectedWDRecordType;
        private Dictionary<WorldDescriptionRecordType, EnvControl> _statementsControls;
        public event PropertyChangedEventHandler PropertyChanged;

        public WorldDescriptionRecordType SelectedWDRecordType
        {
            get { return _selectedWDRecordType; }
            set
            {
                _selectedWDRecordType = value;
                this.GruopBoxStatements.Content = _statementsControls[_selectedWDRecordType];
                NotifyPropertyChanged("SelectedWDRecordType");
            }
        }

        public ObservableCollection<Fluent> Fluents
        {
            get { return _fluents; }
            set { _fluents = value; }
        }

        public ObservableCollection<WorldAction> Actions
        {
            get { return _actions; }
            set { _actions = value; }
        }

        public ObservableCollection<WorldDescriptionRecord> Statements
        {
            get { return _statements; }
            set { _statements = value; }
        }
       
        public IEnumerable<WorldDescriptionRecordType> WDRecordType
        {
            get
            {
                return Enum.GetValues(typeof(WorldDescriptionRecordType)).Cast<WorldDescriptionRecordType>().Where(t => t != WorldDescriptionRecordType.Initially);
            }
        }
        
        #endregion

        #region Constructor and initialization
        public Environment()
        {
            _fluents = new ObservableCollection<Fluent>();
            _actions = new ObservableCollection<WorldAction>();
            _statements = new ObservableCollection<WorldDescriptionRecord>();
            InitControls();
            InitializeComponent();
        }

        private void InitControls()
        {
            _statementsControls = new Dictionary<WorldDescriptionRecordType, EnvControl>
                                      {
                                          {WorldDescriptionRecordType.ActionCausesIf, new EnvCausesIf(_actions, _fluents)},
                                          {WorldDescriptionRecordType.ActionInvokesAfterIf, new EnvInvokesAfterIf(_actions, _fluents)},
                                          {WorldDescriptionRecordType.ActionReleasesIf, new EnvReleasesIf(_actions, _fluents)},
                                          {WorldDescriptionRecordType.ExpressionTriggersAction, new EnvTriggers(_actions, _fluents)},
                                          {WorldDescriptionRecordType.ImpossibleActionAt, new EnvImpossibleAt(_actions, _fluents)},
                                          {WorldDescriptionRecordType.ImpossibleActionIf, new EnvImpossibleIf(_actions, _fluents)}
                                      };
        }
        #endregion

        #region Property Changed
        protected virtual void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
        
        #region Buttons events

        private void ButtonNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateTimeInf())
                return;
            ParseFluentsToInitialRecords();

            Switcher.NextPage(_timeInf, _fluents.ToList(), _actions.ToList(), _statements.ToList());
        }

        private void ButtonAddFluent_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxFluents.Text == "")
            {
                LabelFluentsActionsValidation.Content = "Fluent name is required.";
                return;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(TextBoxFluents.Text, @"[a-zA-Z]+[a-zA-Z0-9\-]*$"))
            {
                LabelFluentsActionsValidation.Content = "Fluent name should be alphanumeric.";
                return;
            }
                
            if (!Fluents.Contains(Fluents.FirstOrDefault(f => (f.Name == TextBoxFluents.Text))))
            {
                var f = new Fluent { Name = this.TextBoxFluents.Text };
                Fluents.Add(f);
                TextBoxFluents.Text = "";
                LabelFluentsActionsValidation.Content = "";
            }
            else
            {
                LabelFluentsActionsValidation.Content = "Fluent with this name already exists.";
            }
        }

        private void ButtonRemoveFluent_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxFluents.SelectedIndex == -1)
                return;
            var fluent = (Fluent)ListBoxFluents.SelectedValue;
            Fluents.Remove(fluent);
        }

        private void ButtonAddAction_Click(object sender, RoutedEventArgs e)
        {
            if (TextBoxActionName.Text == "")
            {
                LabelFluentsActionsValidation.Content = "Action name is required.";
                return;
            }
            if (!System.Text.RegularExpressions.Regex.IsMatch(TextBoxActionName.Text, @"[a-zA-Z]+[a-zA-Z0-9\-]*$"))
            {
                LabelFluentsActionsValidation.Content = "Action name should be alphanumeric.";
                return;
            }
            if (!UpDownTime.Value.HasValue)
            {
                LabelFluentsActionsValidation.Content = "Please specify the duration.";
                return;
            }
            int duration = UpDownTime.Value.Value;
            if (!Actions.Contains(Actions.FirstOrDefault(f => (f.Id == TextBoxActionName.Text && f.Duration == duration))))
            {
                var action = new WorldAction { Id = TextBoxActionName.Text, Duration = duration };
                Actions.Add(action);
                TextBoxActionName.Text = "";
                UpDownTime.Value = null;
                LabelFluentsActionsValidation.Content = "";

            }
            else
            {
                LabelFluentsActionsValidation.Content = "Such action already exists.";
            }
        }

        private void ButtonRemoveAction_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxActions.SelectedIndex == -1)
                return;
            var action = (WorldAction)ListBoxActions.SelectedValue;
            //usuń niepoprawne statementy, które zawierają usuwaną akcję
            for (int i=0; i<Statements.Count; i++)
            {
                var worldDescriptionRecord = Statements[i];
                if (worldDescriptionRecord.ToString().Contains(action.ToString()))
                {
                    Statements.Remove(worldDescriptionRecord);
                    i--;
                }
                    
            }

            Actions.Remove(action);
        }

        private void ButtonAddStatement_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedWDRecordType == WorldDescriptionRecordType.Initially)
                return;
            try
            {
                WorldDescriptionRecord wdr = _statementsControls[SelectedWDRecordType].GetWorldDescriptionRecord();
                Statements.Add(wdr);
            }
            catch (TypeLoadException exception)
            {
            }
        }

        private void ButtonRemoveStatement_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxStatements.SelectedIndex == -1)
                return;

            var wdr = (WorldDescriptionRecord)ListBoxStatements.SelectedValue;
            Statements.Remove(wdr);
        }

        #endregion

        #region Adding data into reasoning module

        private void ParseFluentsToInitialRecords()
        {
            foreach (var f in _fluents)
            {
                var ir = new InitialRecord(f.Name);
                _statements.Add(ir);
            }
        }

        private bool ValidateTimeInf()
        {
            if (!Int32.TryParse(TextBoxTimeInf.Text, out _timeInf))
            {
                LabelTimeInfValidation.Content = "It is necessary to fill default end time value.";
                return false;
            }
            LabelTimeInfValidation.Content = "";
            return true;
        }

        #endregion

    }
}
