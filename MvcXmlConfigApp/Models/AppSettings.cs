using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace MvcXmlConfigApp.Models;

[XmlRoot(ElementName = "Settings", Namespace = SettingsSchema.Namespace)]
public class AppSettings
{
    [Required]
    [Display(Name = "Application name")]
    [XmlElement("ApplicationName")]
    public string? ApplicationName { get; set; }

    [Display(Name = "Enable feature X")]
    [XmlElement("EnableFeatureX")]
    public bool EnableFeatureX { get; set; }

    [Range(1, 1000)]
    [Display(Name = "Maximum items")]
    [XmlElement("MaxItems")]
    public int MaxItems { get; set; } = 10;

    [Required]
    [Display(Name = "Theme")]
    [XmlElement("Theme")]
    public string? Theme { get; set; }

    [Range(5, 3600)]
    [Display(Name = "Refresh interval (seconds)")]
    [XmlElement("RefreshIntervalSeconds")]
    public int RefreshIntervalSeconds { get; set; } = 60;
}

public static class SettingsSchema
{
    public const string Namespace = "http://schemas.example.com/settings";
    public const string XmlFileName = "settings.xml";
    public const string SchemaFileName = "settings.xsd";
}
