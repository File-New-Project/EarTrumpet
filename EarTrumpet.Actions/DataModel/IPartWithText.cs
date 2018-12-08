namespace EarTrumpet_Actions.DataModel
{
    interface IPartWithText
    {
        string Text { get; set; }
        string PromptText { get; }
        string EmptyText { get; }
    }
}
