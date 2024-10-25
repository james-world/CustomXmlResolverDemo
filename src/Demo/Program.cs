using System.Reflection;
using System.Xml;
using System.Xml.Xsl;

using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();
    
// Default to using the the local storage emulator if a connection string isn't provided
string connectionString = configuration["AzureBlobXmlResolver:ConnectionString"] ?? "UseDevelopmentStorage=true"; 

// swap comments to use the caching resolver
// XmlResolver resolver = new AzureBlobXmlResolver(connectionString);
XmlResolver resolver = new CachingXmlResolver(new AzureBlobXmlResolver(connectionString), TimeSpan.FromMinutes(5), 10);
XsltSettings xlstSettings = new XsltSettings(enableDocumentFunction: true, enableScript: true);
XslCompiledTransform xslt = new XslCompiledTransform();

Environment.CurrentDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName;

// This is how you would use the local file system, you can do this to see the output you expect when using blobs
Console.WriteLine("Local file system:");
TransformFile("Example/books.xml", "Example/main.xslt", XmlResolver.FileSystemResolver, xlstSettings);

// Note, the azureblob syntax tells the XSLT processor to use the AzureBlobXmlResolver to resolve URIs
// it's only needed here - the XSLT file itself doesn't need to know about the resolver and can just use relative URIs like normal.
// See the content of Example/main.xslt.xml to see how it just has a relative URI to the included file.
Console.WriteLine("Azure Blob Storage:");
TransformFile("Example/books.xml", "azureblob://xmltest/main.xslt", resolver, xlstSettings);
Console.WriteLine("Cache:");
TransformFile("Example/books.xml", "azureblob://xmltest/main.xslt", resolver, xlstSettings);

// function to transform a file given a resolve and settings
static void TransformFile(string inputFileName, string baseXsltFile, XmlResolver resolver, XsltSettings settings)
{
    XslCompiledTransform xslt = new XslCompiledTransform();
    xslt.Load(baseXsltFile, settings, resolver);

    using XmlReader input = XmlReader.Create(inputFileName);
    using StringWriter sw = new StringWriter();
    using XmlWriter output = XmlWriter.Create(sw);
    
    xslt.Transform(input, output);
    Console.WriteLine(sw.ToString());
}