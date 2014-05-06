﻿using log4net.Config;
using System.Linq;

[assembly: XmlConfigurator(Watch = true)]
namespace KnowledgeRepresentationReasoning
{
    using System.Threading.Tasks;

    using Autofac;
    using Autofac.Extras.CommonServiceLocator;

    using KnowledgeRepresentationReasoning.Expressions;
    using KnowledgeRepresentationReasoning.Helpers;
    using KnowledgeRepresentationReasoning.Logic;
    using KnowledgeRepresentationReasoning.Queries;
    using KnowledgeRepresentationReasoning.Scenario;
    using KnowledgeRepresentationReasoning.World;
    using KnowledgeRepresentationReasoning.World.Records;

    using log4net;

    using Microsoft.Practices.ServiceLocation;
    using System.Collections.Generic;
    using System;

   
    public class Reasoning : IReasoning
    {
        private static IContainer Container { get; set; }
        private ILog _logger { get; set; }
        private WorldDescription worldDescription { get; set; }
        private ScenarioDescription scenarioDescription { get; set; }

        public int TInf { get; set; }

        public Reasoning()
        {
            worldDescription = new WorldDescription();
            scenarioDescription = new ScenarioDescription();
            _logger = ServiceLocator.Current.GetInstance<ILog>();
            TInf = 100;
        }

        public void AddWorldDescriptionRecord(WorldDescriptionRecord record)
        {
            worldDescription.Descriptions.Add(new Tuple<WorldDescriptionRecordType, WorldDescriptionRecord>(record.Type, record));
        }

        public void RemoveWorldDescriptionRecord(WorldDescriptionRecord record)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateWorldDescriptionRecord(WorldDescriptionRecord record)
        {
            throw new System.NotImplementedException();
        }

        public WorldDescription GetWorldDescription()
        {
            return worldDescription;
        }

        public void AddScenarioDescriptionRecord(ScenarioDescriptionRecord record)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveScenarioDescriptionRecord(ScenarioDescriptionRecord record)
        {
            throw new System.NotImplementedException();
        }

        public void UpdateScenarioDescriptionRecord(ScenarioDescriptionRecord record)
        {
            throw new System.NotImplementedException();
        }

        public ScenarioDescription GetScenarioDescription()
        {
            return scenarioDescription;
        }

        public QueryResult ExecuteQuery(Query query)
        {
            QueryResultsContainer queryResultsContainer = new QueryResultsContainer(query.questionType);

            //tree initialization
            Tree tree = new Tree(TInf);
            //add first level
            int numberOfImpossibleLeaf = 0;
            tree.AddFirstLevel(worldDescription, scenarioDescription, out numberOfImpossibleLeaf);
            
            queryResultsContainer.Add(QueryResult.False, numberOfImpossibleLeaf);
            if (queryResultsContainer.CanAnswer())
                return queryResultsContainer.CollectResults();

            //generate next level if query can't answer yet
            while (!queryResultsContainer.CanAnswer())
            {
                //for each leafs:
                    //genereate childs for leaf
                    //create next level in tree
                    //for each child:
                        //if leaf isPossible=false:
                            //add FALSE to resultsContainer (can check answer)
                        //if leaf isEnded=true: add query answer (only TRUE/FALSE are valid) to resultsContainer (can check answer)
                        //else
                            //query validation:
                                //if result == TRUE/FALSE add to resultContainer and delete child
                                //else add child to queue in tree
                //for ends
                int childsCount = tree.LastLevel.Count;
                for (int i = 0; i < childsCount; ++i)
                {
                    Vertex leaf = tree.LastLevel[i];
                    if (!CheckIfLeafIsPossible(leaf))
                    {
                        queryResultsContainer.Add(QueryResult.False);
                        if (queryResultsContainer.CanAnswer())
                            break;
                    }
                    else
                    {
                        //if (CheckIfLeafIsEnded(leaf))
                        //{
                        //    QueryResult result = query.CheckCondition(leaf.State, leaf.Action, leaf.Time);
                        //    if (result != QueryResult.True && result != QueryResult.False)
                        //    {
                        //        _logger.Warn("Unexpected query result!");
                        //        return QueryResult.Error; 
                        //        //return QueryResult.False;
                        //    }
                        //    queryResultsContainer.Add(result);
                        //    if (queryResultsContainer.CanAnswer())
                        //        break;
                        //}
                        tree.SaveLastLevel();
                        List<Vertex> nextLevel = GenerateChildsForLeaf(leaf);
                        
                        foreach (var child in nextLevel)
                        {
                            if (!CheckIfLeafIsPossible(child))
                            {
                                queryResultsContainer.Add(QueryResult.False);
                                if (queryResultsContainer.CanAnswer())
                                    break;
                            }
                            QueryResult result = query.CheckCondition(child.State, child.WorldAction, child.Time);
                            if (result == QueryResult.True || result == QueryResult.False)
                            {
                                queryResultsContainer.Add(result);
                            }
                            else tree.Add(child);
                        }
                    }
                }
            }

            return queryResultsContainer.CollectResults();
        }

        private List<Vertex> GenerateChildsForLeaf(Vertex leaf)
        {
            List<Vertex> vertices = new List<Vertex>();

            Vertex child = new Vertex();
            child.IsPossible = false;
            List<Vertex> impossibleChild = new List<Vertex>() { child };

            int actualTime = leaf.Time;
            int nextObservationTime = scenarioDescription.GetNextObservationTime(actualTime);
            int nextTime = GetNextTimestamp(leaf, scenarioDescription);

            if (!CheckNearestObservation(leaf, actualTime, nextObservationTime, nextTime))
                return impossibleChild;

            leaf.Update(nextTime);

            if (!CheckIfLeafIsPossible(leaf))
                return impossibleChild;

            var implications = worldDescription.GetImplications(leaf);
            vertices = leaf.CreateChildsBasedOnImplications(implications);

            return vertices;
        }

        private bool CheckNearestObservation(Vertex leaf, int actualTime, int nextObservationTime, int nextTime)
        {
            if (nextTime > nextObservationTime)
            {
                ScenarioObservationRecord nextObservation = scenarioDescription.GetObservationFromTime(nextObservationTime);
                if (!nextObservation.CheckState(leaf.State, actualTime))
                {
                    _logger.Warn("Leaf is incopatibile with observation!\n" +
                                    "State: " + leaf.State +
                                    "Observation: " + nextObservation);
                    return false;
                }
            }
            return true;
        }

        private int GetNextTimestamp(Vertex leaf, ScenarioDescription scenarioDescription)
        {
            int nextActionTime = scenarioDescription.GetNextActionTime(leaf.Time);
            int actualActionEndTime = leaf.WorldAction.GetEndTime()??Int32.MaxValue;
            int nextActionStartTime = leaf.GetNextActionTime()??Int32.MaxValue;

            return Math.Min(nextActionTime, Math.Min(actualActionEndTime, nextActionStartTime));
        }

        private bool CheckIfLeafIsEnded(Vertex leaf)
        {
            bool isEnded = false;

            bool noAction = leaf.WorldAction.Equals(null);

            return isEnded;
        }

        private bool CheckIfLeafIsPossible(Vertex leaf)
        {
            return worldDescription.Validate(leaf) && this.scenarioDescription.CheckIfLeafIsPossible(leaf);
        }

        public Task<QueryResult> ExecuteQueryAsync(Query query)
        {
            throw new System.NotImplementedException();
        }

        public static void Initialize()
        {
            // Autofac
            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoggingModule());
            builder.RegisterInstance(LogManager.GetLogger(typeof(Reasoning))).As<ILog>();
            builder.RegisterType<Tree>().As<ITree>();
            builder.RegisterType<SimpleLogicExpression>().As<ILogicExpression>();
            Container = builder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(Container));

            // WorldDescription
            
            // ScenarioDescription
        }
    }
}