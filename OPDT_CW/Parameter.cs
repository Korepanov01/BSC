using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSC
{
    public enum ParameterType
    {
        It,
        Finance,
        Clients,
        Internal,
        Education
    }

    public struct Parameter
    {
        public string Name { get; set; }
        public float Value { get; set; }

        public ParameterType ParameterType { get; set; }

        public Parameter(string name, float value, ParameterType parameterType)
        {
            Name = name;
            Value = value;
            ParameterType = parameterType;
        }
    }
}
