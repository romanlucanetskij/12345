using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Hosting;
using MvcXmlConfigApp.Models;

namespace MvcXmlConfigApp.Services;

public class XmlSettingsService : ISettingsService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _xmlPath;
    private readonly string _schemaPath;
    private readonly XmlSerializer _serializer;

    public XmlSettingsService(IWebHostEnvironment environment)
    {
        _environment = environment;
        var dataDirectory = Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(dataDirectory);

        _xmlPath = Path.Combine(dataDirectory, SettingsSchema.XmlFileName);
        _schemaPath = Path.Combine(dataDirectory, SettingsSchema.SchemaFileName);
        _serializer = new XmlSerializer(typeof(AppSettings));

        EnsureSeedData();
    }

    public Task<AppSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_xmlPath))
        {
            throw new FileNotFoundException($"Settings file not found at '{_xmlPath}'.");
        }

        using var stream = File.OpenRead(_xmlPath);
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { Async = true });
        var settings = (AppSettings?)_serializer.Deserialize(reader);
        return Task.FromResult(settings ?? new AppSettings());
    }

    public async Task UpdateSettingsAsync(AppSettings settings, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var xml = Serialize(settings);
        ValidateAgainstSchema(xml);
        await File.WriteAllTextAsync(_xmlPath, xml, Encoding.UTF8, cancellationToken);
    }

    private void EnsureSeedData()
    {
        if (File.Exists(_xmlPath))
        {
            return;
        }

        var defaultSettings = new AppSettings
        {
            ApplicationName = "Sample MVC XML App",
            EnableFeatureX = true,
            MaxItems = 25,
            Theme = "Light",
            RefreshIntervalSeconds = 120
        };

        var xml = Serialize(defaultSettings);
        ValidateAgainstSchema(xml);
        File.WriteAllText(_xmlPath, xml, Encoding.UTF8);
    }

    private string Serialize(AppSettings settings)
    {
        using var stringWriter = new Utf8StringWriter();
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add(string.Empty, SettingsSchema.Namespace);
        _serializer.Serialize(stringWriter, settings, namespaces);
        return stringWriter.ToString();
    }

    private void ValidateAgainstSchema(string xml)
    {
        if (!File.Exists(_schemaPath))
        {
            throw new FileNotFoundException($"Schema file not found at '{_schemaPath}'.");
        }

        var schemas = new XmlSchemaSet();
        using var schemaReader = XmlReader.Create(_schemaPath);
        schemas.Add(SettingsSchema.Namespace, schemaReader);

        var document = XDocument.Parse(xml);
        string? validationError = null;
        document.Validate(schemas, (o, e) =>
        {
            validationError ??= e.Message;
        }, true);

        if (validationError is not null)
        {
            throw new XmlSchemaValidationException(validationError);
        }
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
