using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace InspectCodeSnapshot
{
    [XmlRoot("Report")]
    public class Report
    {
        [XmlElement("Issues")]
        public Issues Issues { get; set; }
    }

    public class Issues
    {
        [XmlElement("Project")]
        public List<Project> Project { get; set; }
    }

    public class Project
    {
        [XmlAttribute("Name")]
        public string Name { get; set; }
        
        [XmlElement("Issue")]
        public List<Issue> Issues { get; set; }
    }
    
    public class Issue
    {
        [XmlAttribute("TypeId")]
        public string TypeId { get; set; }
        
        [XmlAttribute("File")]
        public string File { get; set; }
        
        [XmlAttribute("Offset")]
        public string Offset { get; set; }
        
        [XmlAttribute("Line")]
        public string Line { get; set; }
        
        [XmlAttribute("Message")]
        public string Message { get; set; }

        public override string ToString()
        {
            return $"TypeId: {TypeId} File: {File} Offset: {Offset} Line: {Line} Message: {Message}";
        }
    }
}