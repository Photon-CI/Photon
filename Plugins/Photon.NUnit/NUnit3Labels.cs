namespace Photon.NUnitPlugin
{
    public enum NUnit3Labels
    {
        Default = 0,

        /// <summary>
        /// No labeling is used. Both normal and immediate output appear
        /// in the order produced - i.e. immediate first.
        /// </summary>
        Off,

        /// <summary>
        /// A label appears before each sequence of output lines from the same test.
        /// Since tests may be run in parallel, output from different tests may be intermixed.
        /// </summary>
        On,

        /// <summary>
        /// A label appears at the start of every test, whether it produces output or not.
        /// Additional labels are produced as needed if interspersed output takes place,
        /// just as for --labels=On. Synonym for --labels=All.
        /// </summary>
        Before,

        /// <summary>
        /// A label appears at the end of every test, whether it produced output or not.
        /// This label includes the pass/fail status of the test in addition to its name.
        /// Additional labels are produced as needed if there is any output, just as for --labels=On.
        /// </summary>
        After,

        /// <summary>
        /// A label appears at the start of every test, whether it produces output or not.
        /// Additional labels are produced as needed if interspersed output takes place,
        /// just as for --labels=On. Synonym for --labels=Before. 
        /// </summary>
        All,
    }
}
