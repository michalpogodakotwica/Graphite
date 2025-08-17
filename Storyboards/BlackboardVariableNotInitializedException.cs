using System;

namespace Storyboards
{
    public class BlackboardVariableNotInitializedException : Exception
    {
        public BlackboardVariableNotInitializedException(IBlackboardVariable blackboardVariable) : 
            base($"Blackboard variable '{blackboardVariable.Name}' of type '{blackboardVariable.Type}' " +
                 "has not been initialized. Did you forget to select value in the graph editor?")
        {
        }
    }
}