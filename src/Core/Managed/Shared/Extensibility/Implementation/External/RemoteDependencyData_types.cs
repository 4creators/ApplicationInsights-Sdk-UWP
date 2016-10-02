
//------------------------------------------------------------------------------
// This code was generated by a tool.
//
//   Tool : Bond Compiler 0.4.1.0
//   File : RemoteDependencyData_types.cs
//
// Changes to this file may cause incorrect behavior and will be lost when
// the code is regenerated.
// <auto-generated />
//------------------------------------------------------------------------------


// suppress "Missing XML comment for publicly visible type or member"
#pragma warning disable 1591


#region ReSharper warnings
// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantNameQualifier
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
// ReSharper disable UnusedParameter.Local
// ReSharper disable RedundantUsingDirective
#endregion

namespace Microsoft.ApplicationInsights.Extensibility.Implementation.External
{
    using System.Collections.Generic;

    
    
    [System.CodeDom.Compiler.GeneratedCode("gbc", "0.4.1.0")]
    internal partial class RemoteDependencyData
        : Domain
    {
        
        
        public int ver { get; set; }

        
        
        
        public string name { get; set; }

        
        
        
        public string id { get; set; }

        
        
        
        public string resultCode { get; set; }

        
        public DataPointType kind { get; set; }

        
        public double value { get; set; }

        
        
        
        
        public string duration { get; set; }

        
        public DependencyKind dependencyKind { get; set; }

        
        
        public bool success { get; set; }

        
        public bool async { get; set; }

        
        public DependencySourceType dependencySource { get; set; }

        
        
        public string commandName { get; set; }

        
        
        
        public string data { get; set; }

        
        
        
        public string dependencyTypeName { get; set; }

        
        
        
        public string target { get; set; }

        
        
        
        
        public IDictionary<string, string> properties { get; set; }

        
        
        
        public IDictionary<string, double> measurements { get; set; }

        public RemoteDependencyData()
            : this("AI.RemoteDependencyData", "RemoteDependencyData")
        {}

        protected RemoteDependencyData(string fullName, string name)
        {
            ver = 2;
            this.name = "";
            id = "";
            resultCode = "";
            kind = DataPointType.Measurement;
            duration = "";
            dependencyKind = DependencyKind.Other;
            success = true;
            dependencySource = DependencySourceType.Undefined;
            commandName = "";
            
            dependencyTypeName = "";
            target = "";
            properties = new Dictionary<string, string>();
            measurements = new Dictionary<string, double>();
        }
    }
} // AI










