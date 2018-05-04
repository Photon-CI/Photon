using System.Collections.Generic;

namespace Photon.Library.GitHub
{
    public static class CommitState
    {
        private static readonly Dictionary<CommitStates, string> stateMap;

        static CommitState()
        {
            stateMap = new Dictionary<CommitStates, string> {
                [CommitStates.Pending] = "pending",
                [CommitStates.Success] = "success",
                [CommitStates.Failure] = "failure",
                [CommitStates.Error] = "error",
            };
        }

        public static string GetString(CommitStates state)
        {
            return stateMap[state];
        }
    }

    public enum CommitStates
    {
        Pending,
        Success,
        Failure,
        Error,
    }
}
