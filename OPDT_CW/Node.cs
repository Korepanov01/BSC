using System.Collections.Generic;

namespace OPDT_CW;

internal struct Node
{
    public string Description { get; }
    public Dictionary<string, float> Parameters { get; }
    public float RiskLevel { get; }

    public Node(string description, Dictionary<string, float> parameters, float riskLevel)
    {
        Description = description;
        Parameters = parameters;
        RiskLevel = riskLevel;
    }
}