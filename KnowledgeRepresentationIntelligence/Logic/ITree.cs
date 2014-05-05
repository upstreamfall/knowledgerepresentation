﻿namespace KnowledgeRepresentationReasoning.Logic
{
    internal interface ITree
    {
        int AddFirstLevel(World.WorldDescription WorldDescription, Scenario.ScenarioDescription ScenarioDescription, out int numberOfImpossibleLeaf);

        int LastLevelCount();
    }
}
