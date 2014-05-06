﻿using KnowledgeRepresentationReasoning.World;
using System.Collections.Generic;
using System.Linq;

namespace KnowledgeRepresentationReasoning.Logic
{
    public class Vertex
    {
        public State State { get; private set; }
        public Action Action { get; set; }
        public int Time { get; private set; }
        private Vertex Root { get; set; }
        public bool IsPossible { get; set; }
        public bool IsEnded { get; set; }

        public List<Action> NextActions { get; set; }

        public Vertex(State state, Action action, int time, Vertex root)
        {
            State = state;
            Action = action;
            Time = time;
            Root = root;
        }

        public Vertex()
        {
            // TODO: Complete member initialization
        }

        internal int? GetNextActionTime()
        {
            return NextActions.Min(action => action.StartAt);
        }

        internal void Update(int nextTime)
        {
            UpdateStateOnFluentChange();
            //int nextTime = UpdateAction();
            //UpdateTime(nextTime);
        }

        private int UpdateAction()
        {
            int endTime = Action.StartAt + Action.Duration??-1;

            return 0;
        }

        private void UpdateTime(int newTime)
        {
            Time = newTime;
        }

        private void UpdateStateOnFluentChange()
        {
            throw new System.NotImplementedException();
        }

        internal List<Vertex> CreateChildsBasedOnImplications(List<Implication> implications)
        {
            throw new System.NotImplementedException();
        }
    }
}