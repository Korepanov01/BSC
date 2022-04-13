using System.Collections.Generic;

namespace BSC;

internal struct Node
{
    public string Description { get; }
    public List<Parameter>[] Parameters { get; }

    public Node(string description, List<Parameter>[] parameters)
    {
        Description = description;
        Parameters = parameters;
    }
}