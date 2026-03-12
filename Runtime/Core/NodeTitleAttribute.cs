using System;

public class NodeTitleAttribute : Attribute
{
    public readonly string Title;
    public readonly string CreateMenuName;
    

    public NodeTitleAttribute(string title)
    {
        this.Title = title;
        this.CreateMenuName = title;
    }
    
    public NodeTitleAttribute(string title, string createMenuName)
    {
        this.Title = title;
        this.CreateMenuName = createMenuName;
    }
}