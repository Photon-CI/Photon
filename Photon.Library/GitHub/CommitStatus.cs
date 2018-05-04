using Newtonsoft.Json;

namespace Photon.Library.GitHub
{
    public class CommitStatus
    {
        /// <summary>
        /// The state of the status.
        /// </summary>
        public CommitStates State {get; set;}

        /// <summary>
        /// The target URL to associate with this status. This URL will
        /// be linked from the GitHub UI to allow users to easily see
        /// the source of the status.
        /// </summary>
        public string TargetUrl {get; set;}

        /// <summary>
        /// A short description of the status.
        /// </summary>
        public string Description {get; set;}

        /// <summary>
        /// A string label to differentiate this status from the status of other systems.
        /// </summary>
        public string Context {get; set;}


        public CommitStatus()
        {
            Context = "default";
        }

        public string ToJson(Formatting formatting = Formatting.None)
        {
            return JsonConvert.SerializeObject(new {
                state = CommitState.GetString(State),
                target_url = TargetUrl,
                description = Description,
                context = Context,
            }, formatting);
        }
    }
}
