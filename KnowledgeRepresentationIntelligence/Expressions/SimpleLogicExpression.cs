﻿using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("KnowledgeRepresentationReasoning.Test")]
namespace KnowledgeRepresentationReasoning.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using ExpressionEvaluator;

    using KnowledgeRepresentationReasoning.Helpers;
    using KnowledgeRepresentationReasoning.World;

    internal class SimpleLogicExpression : ILogicExpression
    {
        private readonly char[] specialCharacters = new[] { '|', '&', '(', ')', '!' };
        private readonly string _condition;

        public SimpleLogicExpression(SimpleLogicExpression logicExpression)
        {
            this._condition = logicExpression._condition;
        }

        public SimpleLogicExpression()
        {
            // TODO: Complete member initialization
        }

        public SimpleLogicExpression(string _condition)
        {
            // TODO: Complete member initialization
            this._condition = _condition;
        }

        private string _expression { get; set; }

        public bool Evaluate()
        {
            if (this._expression.Equals(string.Empty)) return false;            
            var expression = new CompiledExpression(this._expression);
            return (bool)expression.Eval();
        }

        public bool Evaluate(IEnumerable<Tuple<string, bool>> values)
        {
            if (this._expression.Equals(string.Empty)) return false;
            var expression = new CompiledExpression(this._expression);
            expression.RegisterType("h", typeof(ExpressionHelper));
            if (values != null)
            {
                foreach (var value in values)
                {
                    expression.RegisterType(value.Item1, value.Item2);
                }
            }
            return (bool)expression.Eval();
        }

        public void SetExpression(string expression)
        {
            this._expression = expression??string.Empty;
        }

        public string[] GetFluentNames()
        {
            string filteredString = this._expression;
            filteredString = this.specialCharacters.Aggregate(filteredString, (current, specialCharacter) => current.Replace(specialCharacter, ' '));
            filteredString = Regex.Replace(filteredString, " {2,}", " ");
            return filteredString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
        }

        public List<Fluent[]> CalculatePossibleFluents()
        {
            var result = new List<Fluent[]>();
            string[] fluentNames = this.GetFluentNames();
            int numberOfFluents = fluentNames.Length;
            foreach (var code in Gray.GetGreyCodesWithLengthN(numberOfFluents))
            {
                var possibleFluents = new Fluent[numberOfFluents];
                for (int i = 0; i < numberOfFluents; i++)
                    possibleFluents[i] = new Fluent { Id = i.ToString(), Name = fluentNames[i], Value = code[i] };
                if (this.Evaluate(possibleFluents.Select(t => new Tuple<string, bool>(t.Name, t.Value))))
                    result.Add(possibleFluents);
            }
            return result;
        }

        class ExpressionHelper
        {
            public static bool impl(bool a, bool b)
            {
                if (a == false) return true;
                return b;
            }

            public static bool rown(bool a, bool b)
            {
                if (a == b) return true;
                else return false;
            }
        }


        public void AddExpression(ILogicExpression logicExpression)
        {
            if (logicExpression == null) return;

            _expression = "(" + _expression + ") && (" + logicExpression + ")";
        }
    }
}
