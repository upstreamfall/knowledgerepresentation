﻿using System;
using Xceed.Wpf.Toolkit.Core;

namespace KnowledgeRepresentationInterface.Views.QueriesControls
{
    using System.Collections.Generic;
    using System.Linq;

    using KnowledgeRepresentationReasoning.Queries;
    using KnowledgeRepresentationReasoning.Scenario;
    using KnowledgeRepresentationReasoning.World;

    /// <summary>
    /// Interaction logic for QueAccesibleCondition.xaml
    /// </summary>
    public partial class QueAccesibleCondition : QueControl
    {
        public QueAccesibleCondition()
        {
            InitializeComponent();
            RegisterName("queContr_cond", this);
        }

        public QueAccesibleCondition(List<ScenarioDescription> scenarios, List<WorldAction> actions, List<Fluent> fluents)
            : base(scenarios, actions, fluents)
        {
            InitializeComponent();
            RegisterName("queContr_cond", this);
        }

        public override Query GetQuery(QuestionType questionType)
        {
            if (String.IsNullOrEmpty(SelectedScenario))
                throw new InvalidContentException("Scenario name is required.");
            if (String.IsNullOrEmpty(TextBoxCondition.Text))
                throw new InvalidContentException("Condition is required.");

            var selectedScenario = Scenarios.First(t => t.Name.Equals(SelectedScenario));
            return new AccesibleConditionQuery(questionType, TextBoxCondition.Text, selectedScenario);
        }

        public override void Clear()
        {
            SelectedScenario = "";
            TextBoxCondition.Text = null;
        }
    }
}