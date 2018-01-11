//=============================================================================
//
// Copyright (C) 2007 Michael Coyle, Blue Toque
// http://www.BlueToque.ca/Products/CodeGeneration.html
// michael.coyle@BlueToque.ca
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// http://www.gnu.org/licenses/gpl.txt
//
//=============================================================================
using System;
using System.Text;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Web.Services.Description;
using System.Web.Services.Discovery;
using System.IO;
using System.CodeDom;
using System.Diagnostics;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Specialized;

using CodeGeneration.CodeModifiers;

namespace CodeGeneration.Generators
{
    /// <summary>
    /// Generate a Proxy from a WSDL document
    /// </summary>
    public class WsdlProxyGenerator : BaseCodeGenerator, ICodeGenerator
    {
        public WsdlProxyGenerator(string url)
        {
            m_url = url;   
        }

        #region fields

        private string m_wsdl;

        private string m_url;

        private ServiceDescriptionImportStyle m_style = ServiceDescriptionImportStyle.Client;

        private XmlSchemas m_schemas = new XmlSchemas();

        private ServiceDescriptionCollection m_descriptions = new ServiceDescriptionCollection();

        //private string m_proxyServer;

        #endregion

        #region properties

        public string Wsdl
        {
            get { return m_wsdl; }
            set { m_wsdl = value; }
        }

        public string Url
        {
            get { return m_url; }
            set { m_url = value; }
        }

        /// <summary>
        /// The Style controls weather we generate a server or a client proxy
        /// </summary>
        public ServiceDescriptionImportStyle Style
        {
            get { return m_style; }
            set { m_style = value; }
        }

        //public string ProxyServer
        //{
        //    get { return m_proxyServer; }
        //    set { m_proxyServer = value; }
        //}

        #endregion

        #region utility
        protected override void AddReferencedAssemblies()
        {
            if (!ReferencedAssemblies.Contains("System.dll"))
                ReferencedAssemblies.Add("System.dll");
            if (!ReferencedAssemblies.Contains("mscorlib.dll"))
                ReferencedAssemblies.Add("mscorlib.dll");
            if (!ReferencedAssemblies.Contains("System.Xml.dll"))
                ReferencedAssemblies.Add("System.Xml.dll");
            if (!ReferencedAssemblies.Contains("System.Web.Services.dll"))
                ReferencedAssemblies.Add("System.Web.Services.dll");
            if (!ReferencedAssemblies.Contains("System.Data.dll"))
                ReferencedAssemblies.Add("System.Data.dll");
        }
        #endregion

        #region old download wsdl
        //private bool DownloadWsdl()
        //{
        //    if (m_wsdl != null)
        //    {
        //        Trace.TraceInformation("WSDL is already downloaded");
        //        return true;
        //    }

        //    try
        //    {
        //        Trace.TraceInformation("Beginning download of WSDL from {0}", m_url);
        //        WebRequest request = WebRequest.Create(m_url);

        //        //#region ATTACH A CLIENT CERTIFICATE IF ONE IS PROVIDED (ATTACH IT TO THE REQUEST)
        //        //ServicePointManager.CertificatePolicy = AssemblyCall.myCertificateValidation;
        //        //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CertificateValidation.myRemoteCertificateValidationCallback);
        //        //X509Certificate certificate = AppCache.UDDIServerDictionary.ClientCertificate;
        //        //if (certificate != null)
        //        //  request.ClientCertificates.Add(certificate);
        //        //#endregion

        //        using (WebResponse response = request.GetResponse())
        //        {
        //            using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.ASCII))
        //            {
        //                m_wsdl = sr.ReadToEnd();
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Errors.Add(new CompilerError(string.Empty, 0, 0, string.Empty, string.Format("Error downloading WSDL from {0}: {1}", m_url, ex.Message)));
        //        Trace.TraceError(ex.ToString());
        //        if (this.ThrowExceptions) throw;
        //        return false;
        //    }

        //    return true;
        //}
        #endregion

        #region downloading and reading WSDL, XSD

        /// <summary>
        /// Download WSDL
        /// </summary>
        /// <returns></returns>
        private bool DownloadWsdl()
        {
            DiscoveryClientProtocol discoveryClientProtocol = this.CreateDiscoveryClient();

            // is the url local or remote?
            if (File.Exists(m_url))
            {
                ProcessLocal(m_url, discoveryClientProtocol);
            }
            else
            {
                ProcessUrl(m_url, discoveryClientProtocol);
            }

            return true;
        }

        /// <summary>
        /// Add documents to either service description collection or
        /// schema collection
        /// </summary>
        /// <param name="path"></param>
        /// <param name="document"></param>
        private void AddDocument( string path,  object document)
        {
            ServiceDescription serviceDescription = document as ServiceDescription;
            if (serviceDescription != null)
            {
                // if the service description has not already been imported,
                // add it to the "descriptions" collection
                if (!m_descriptions.Contains(serviceDescription) )
                {
                    m_descriptions.Add(serviceDescription);

                    #region write out the WSDL string to the m_wsdl
                    StringWriter stringWriter = new StringWriter();
                    using (XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter))
                    {
                        xmlWriter.Formatting = Formatting.Indented;
                        serviceDescription.Write(xmlWriter);
                    }

                    m_wsdl = stringWriter.ToString();
                    #endregion
                }
                else
                {
                    Trace.TraceWarning(
                        string.Format("Warning: Ignoring duplicate service description with TargetNamespace='{0}' from '{1}'.", 
                        serviceDescription.TargetNamespace, 
                        path));
                }
            }
            else
            {
                XmlSchema schema = document as XmlSchema;
                if (schema != null)
                {
                    // if this schema is no already in the "schemas" collection,
                    // then add it
                    if (!m_schemas.Contains(schema.TargetNamespace))
                    {
                        m_schemas.Add(schema);

                        #region write the schema text out to the m_xsds StringCollection
                        //StringWriter stringWriter = new StringWriter();
                        //using (XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter))
                        //{
                        //    xmlWriter.Formatting = Formatting.Indented;
                        //    schema.Write(xmlWriter);
                        //}
                        //this.m_xsds.Add(stringWriter.ToString());
                        #endregion
                    }
                    else
                    {
                        Trace.TraceWarning( string.Format("Warning: Ignoring duplicate schema description with TargetNamespace='{0}' from '{1}'.",
                            schema.TargetNamespace, 
                            path));
                    }
                }
            }
        }

        /// <summary>
        /// Process local paths
        /// </summary>
        /// <param name="path"></param>
        /// <param name="discoveryClientProtocol"></param>
        private void ProcessLocal( string path, DiscoveryClientProtocol discoveryClientProtocol)
        {
            string extension = Path.GetExtension(path);

            if (string.Compare(extension, ".discomap", true) == 0)
            {
                discoveryClientProtocol.ReadAll(path);
            }
            else
            {
                object obj = null;
                if (string.Compare(extension, ".wsdl", true) == 0)
                {
                    obj = this.ReadLocalDocument(false, path);
                }
                else
                {
                    if (string.Compare(extension, ".xsd", true) != 0)
                        throw new InvalidOperationException("Unknown file type " + path);

                    obj = this.ReadLocalDocument(true, path);
                }

                if (obj != null)
                    this.AddDocument(path, obj);

            }
        }

        /// <summary>
        /// Process remote urls
        /// </summary>
        /// <param name="url"></param>
        /// <param name="discoveryClientProtocol"></param>
        private void ProcessUrl(string url, DiscoveryClientProtocol discoveryClientProtocol)
        {
            try
            {
                DiscoveryDocument discoveryDocument = discoveryClientProtocol.DiscoverAny(url);
                discoveryClientProtocol.ResolveAll();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("General Error " + url, ex);
            }

            foreach (DictionaryEntry entry in discoveryClientProtocol.Documents)
                this.AddDocument((string)entry.Key, entry.Value);
        }

        /// <summary>
        /// Read documents on local hard drive
        /// </summary>
        /// <param name="isSchema"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private object ReadLocalDocument(bool isSchema, string path)
        {
            object obj = null;

            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    if (isSchema)
                        return XmlSchema.Read(new XmlTextReader(reader), null);

                    obj = ServiceDescription.Read(reader.BaseStream);
                }
            }
            catch
            {
                throw;
            }

            return obj;
        }
        #endregion

        /// <summary>
        /// Create the discovery protocol client, configuring it with credentials for proxy servers and
        /// web servers, and ppossibly also with X509 certificates
        /// </summary>
        /// <returns></returns>
        private DiscoveryClientProtocol CreateDiscoveryClient()
        {
            DiscoveryClientProtocol discoveryClientProtocol = new DiscoveryClientProtocol();
            discoveryClientProtocol.AllowAutoRedirect = true;

            // TODO: figure out how to prompt for valid credentials otherwise we are stuck with public web servers
            //if (!string.IsNullOrEmpty(this.WsdlProperties.UserName))
            //    discoveryClientProtocol.Credentials = new NetworkCredential(this.WsdlProperties.UserName, this.WsdlProperties.Password, this.WsdlProperties.Domain);
            //else
            discoveryClientProtocol.Credentials = CredentialCache.DefaultCredentials;

            #region set up the proxy server
            // TODO: figure out how to store username and passwords (or prompt for them)
            //if (!string.IsNullOrEmpty(this.ProxyServer))
            //{
            //    IWebProxy proxy = new WebProxy(this.WsdlProperties.ProxyServer);
            //    proxy.Credentials = new NetworkCredential(
            //        this.WsdlProperties.ProxyUserName,
            //        this.WsdlProperties.ProxyPassword,
            //        this.WsdlProperties.ProxyDomain);
            //    discoveryClientProtocol.Proxy = proxy;
            //}
            #endregion

            return discoveryClientProtocol;
        }

        public bool Compile()
        {
            Errors.Clear();
            
            if (!DownloadWsdl())
                return false;
            
            try
            {
                #region import the service description

                ServiceDescriptionImporter importer = new ServiceDescriptionImporter();
                // add downloaded schemas here

                // add the service description (s)
                importer.AddServiceDescription(m_descriptions[0], null, null);

                // add the imported schemas
                importer.Schemas.Add(m_schemas);
                
                // Generate a proxy client.
                importer.Style = m_style;

                // protocol defaults to SOAP

                // Generate properties to represent primitive values.
                importer.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties;

                CreateCodeNamespace();

                CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
                codeCompileUnit.Namespaces.Add(CodeNamespace);

                // Import the service into the Code-DOM tree. This creates proxy code that uses the service.
                ServiceDescriptionImportWarnings warnings = importer.Import(CodeNamespace, codeCompileUnit);

                // Handle all import warnings
                HandleImportWarnings(warnings);

                #endregion

                if (!GenerateCode())
                    return false;

            }
            catch (Exception ex)
            {
                Errors.Add(new CompilerError(string.Empty, 0,0,string.Empty,ex.Message));
                Trace.TraceError(ex.ToString());
                if (this.ThrowExceptions) throw;
                return false;
            }

            return true;
        }

        private void HandleImportWarnings(ServiceDescriptionImportWarnings warnings)
        {
            // TODO: explicitly handle all of the warnings generated by the ServiceImporter
            if (warnings != 0)
            {
                Trace.TraceError("Warnings: {0}", warnings);

                StringBuilder exceptionMessage = new StringBuilder();

                if ((warnings | ServiceDescriptionImportWarnings.NoCodeGenerated) == warnings)
                {
                    CompilerError error = new CompilerError();
                    error.ErrorText = ErrorMessages.NoCodeGenerated;
                    error.IsWarning = false;
                    error.FileName = m_url;
                    error.ErrorNumber = "CG110";
                    Errors.Add(error);

                    exceptionMessage.AppendLine(error.ToString());
                }
                if ((warnings | ServiceDescriptionImportWarnings.NoMethodsGenerated) == warnings)
                {
                    CompilerError error = new CompilerError();
                    error.ErrorText = ErrorMessages.NoMethodsGenerated;
                    error.IsWarning = false;
                    error.FileName = m_url;
                    error.ErrorNumber = "CG111";
                    Errors.Add(error);

                    exceptionMessage.AppendLine(error.ToString());
                }
                if ((warnings | ServiceDescriptionImportWarnings.OptionalExtensionsIgnored) == warnings)
                {
                    CompilerError error = new CompilerError();
                    error.ErrorText = ErrorMessages.OptionalExtensionsIgnored;
                    error.IsWarning = true;
                    error.FileName = m_url;
                    error.ErrorNumber = "CG112";
                    Errors.Add(error);

                }
                if ((warnings | ServiceDescriptionImportWarnings.RequiredExtensionsIgnored) == warnings)
                {
                    CompilerError error = new CompilerError();
                    error.ErrorText = ErrorMessages.RequiredExtensionsIgnored;
                    error.IsWarning = false;
                    error.FileName = m_url;
                    error.ErrorNumber = "CG113";
                    Errors.Add(error);

                    exceptionMessage.AppendLine(error.ToString());
                }

                if ((warnings | ServiceDescriptionImportWarnings.SchemaValidation) == warnings)
                {
                    CompilerError error = new CompilerError();
                    error.ErrorText = ErrorMessages.SchemaValidation;
                    error.IsWarning = false;
                    error.FileName = m_url;
                    error.ErrorNumber = "CG114";
                    Errors.Add(error);

                    exceptionMessage.AppendLine(error.ToString());

                }
                if ((warnings | ServiceDescriptionImportWarnings.UnsupportedBindingsIgnored) == warnings)
                {
                    CompilerError error = new CompilerError();
                    error.ErrorText = ErrorMessages.UnsupportedBindingsIgnored;
                    error.IsWarning = true;
                    error.FileName = m_url;
                    error.ErrorNumber = "CG115";
                    Errors.Add(error);

                }
                if ((warnings | ServiceDescriptionImportWarnings.UnsupportedOperationsIgnored) == warnings)
                {
                    CompilerError error = new CompilerError();
                    error.ErrorText = ErrorMessages.UnsupportedOperationsIgnored;
                    error.IsWarning = true;
                    error.FileName = m_url;
                    error.ErrorNumber = "CG116";
                    Errors.Add(error);
                }
                if ((warnings | ServiceDescriptionImportWarnings.WsiConformance) == warnings)
                {
                    CompilerError error = new CompilerError();
                    error.ErrorText = ErrorMessages.WsiConformance;
                    error.IsWarning = true;
                    error.FileName = m_url;
                    error.ErrorNumber = "CG117";
                    Errors.Add(error);
                }

                if (exceptionMessage.Length!=0)
                {
                    throw new ApplicationException(string.Format("{0}; Warnings : {1}", exceptionMessage.ToString(), warnings));
                }
                else
                {
                    Trace.TraceWarning("Warnings occurred when generating code for web service {0}", warnings);
                }

            }
        }

    }
}
