namespace Stockroom.Web.Components.Shared;

public enum NoticeKind
{
    Success,
    Error,
}

// A message for the user always travels with its kind. Nothing
// guesses the kind from the words of the text.
public sealed record Notice(NoticeKind Kind, string Text);
