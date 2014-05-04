﻿using log4net.Config;

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

   
    public class ReasoningFacade : IReasoning
    {
        private IContainer Container { get; set; }
        private ILog logger { get; set; }
        private WorldDescription worldDescription { get; set; }
        private ScenarioDescription scenarioDescription { get; set; }

        public int TInf { get; set; }

        public ReasoningFacade()
        {
            this.Initialize();
            logger = ServiceLocator.Current.GetInstance<ILog>();

            TInf = 100;
        }

        public void AddWorldDescriptionRecord(WorldDescriptionRecord record)
        {
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
            throw new System.NotImplementedException();
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
            throw new System.NotImplementedException();
        }

        public QueryResult ExecuteQuery(Query query)
        {
            QueryResultsContainer queryResultsContainer = new QueryResultsContainer(query.questionType);

            //tree initialization
            Tree tree = new Tree(TInf);
            //add first level
            tree.AddFirstLevel(worldDescription, scenarioDescription);

            //generate next level if query can't answer yet
            while (!queryResultsContainer.CanAnswer())
            {
                //for each leafs:
                    //if leaf isPossible=true:
                        //false - add FALSE to resultsContainer (can check answer)
                        //true:
                            //if leaf isEnded=true: add query answer (only TRUE/FALSE are valid) to resultsContainer (can check answer)
                            //genereate childs for leaf
                            //create next level in tree
                            //for each child:
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
                        if (CheckIfLeafIsEnded(leaf))
                        {
                            QueryResult result = query.CheckCondition(leaf.State, leaf.Action, leaf.Time);
                            if (result != QueryResult.True && result != QueryResult.False)
                            {
                                logger.Warn("Unexpected query result!");
                                return QueryResult.Error;
                            }
                            queryResultsContainer.Add(result);
                            if (queryResultsContainer.CanAnswer())
                                break;
                        }
                        tree.SaveLastLevel();
                        List<Vertex> nextLevel = GenerateChildsForLeaf(leaf);
                        foreach (var child in nextLevel)
                        {
                            QueryResult result = query.CheckCondition(child.State, child.Action, child.Time);
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
            throw new System.NotImplementedException();
        }

        private bool CheckIfLeafIsEnded(Vertex leaf)
        {
            throw new System.NotImplementedException();
        }

        private bool CheckIfLeafIsPossible(Vertex leaf)
        {
            return worldDescription.CheckIfLeafIsPossible(leaf) && this.scenarioDescription.CheckIfLeafIsPossible(leaf);
        }

        public Task<QueryResult> ExecuteQueryAsync(Query query)
        {
            throw new System.NotImplementedException();
        }

        public void Initialize()
        {
            // Autofac
            var builder = new ContainerBuilder();
            builder.RegisterModule(new LoggingModule());
            builder.RegisterInstance(LogManager.GetLogger(typeof(ReasoningFacade))).As<ILog>();
            builder.RegisterType<Tree>().As<ITree>();
            builder.RegisterType<SimpleLogicExpression>().As<ILogicExpression>();
            Container = builder.Build();
            ServiceLocator.SetLocatorProvider(() => new AutofacServiceLocator(Container));

            // WorldDescription
            
            // ScenarioDescription
        }
    }
}
