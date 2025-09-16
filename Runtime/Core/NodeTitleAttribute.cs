using System;

public class NodeTitleAttribute : Attribute
{
    public readonly string Title;

    public NodeTitleAttribute(string title)
    {
        this.Title = title;
    }
}