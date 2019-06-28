namespace EarTrumpet.UI.Helpers
{
    public enum FlyoutViewState
    {
        NotLoaded,      // Initialization phase (startup)
        Hidden,         // Flyout is fully ready to begin a show cycle
        Opening,        // Open animation
        Open,
        Closing_Stage1, // Closing (animation optimal)
        Closing_Stage2, // Delay de-bounce state to avoid unstable open/hide.
    }

}
