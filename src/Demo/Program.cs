using System.Reflection;
using System.Xml;
using System.Xml.Xsl;

using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

string connectionString = configuration["AzureBlobXmlResolver:ConnectionString"] ?? throw new Exception("See README.md");

XmlResolver resolver = new AzureBlobXmlResolver(connectionString);
XsltSettings xlstSettings = new XsltSettings(enableDocumentFunction: true, enableScript: true);
XslCompiledTransform xslt = new XslCompiledTransform();

Environment.CurrentDirectory = new FileInfo(Assembly.GetExecutingAssembly().Location).Directory!.FullName;

// This is how you would use the local file system, you can do this to see the output you expect when using blobs
// xslt.Load("Example/main.xslt.xml", xlstSettings, XmlResolver.FileSystemResolver);

xslt.Load("azureblob://xmltest/main.xslt.xml", xlstSettings, resolver);

using XmlReader input = XmlReader.Create("Example/books.xml");
using StringWriter sw = new StringWriter();
using XmlWriter output = XmlWriter.Create(sw);

// run transform and get output in a string
xslt.Transform(input, output);
Console.WriteLine(sw.ToString());