/// The SignalsType struct stores volatile flags updated during the search
/// typically in an async fashion e.g. to stop the search by the GUI.
public struct SignalsType
{
    public volatile bool stop, stopOnPonderhit, firstRootMove, failedLowAtRoot;
};