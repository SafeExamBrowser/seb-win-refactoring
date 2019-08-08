<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">
    <xsl:output method="xml" indent="yes"/>
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()" />
        </xsl:copy>
    </xsl:template>
    <xsl:template match="wix:File[substring(@Source, string-length(@Source) - string-length('SafeExamBrowser.exe') + 1) = 'SafeExamBrowser.exe']">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()" />
            <xsl:attribute name="Id">
                <xsl:text>MainExecutable</xsl:text>
            </xsl:attribute>
        </xsl:copy>
        <wix:File Id="ApplicationIconFile" Source="Resources\Application.ico" />
        <wix:File Id="ConfigurationIconFile" Source="Resources\ConfigurationFile.ico" />
        <wix:ProgId Id="ConfigurationFileExtension" Description="SEB Configuration File" Icon="ConfigurationIconFile" Advertise="no">
            <wix:Extension Id="seb" ContentType="application/seb" Advertise="no">
                <wix:Verb Id="open" Command="Open" Argument="&quot;%1&quot;" TargetFile="MainExecutable" />
            </wix:Extension>
        </wix:ProgId>
        <wix:RegistryKey Root="HKCR" Key="seb">
            <wix:RegistryValue Value="URL:Safe Exam Browser Protocol" Type="string" />
            <wix:RegistryValue Name="URL Protocol" Value="" Type="string" />
            <wix:RegistryValue Key="DefaultIcon" Value="[#ApplicationIconFile]" Type="string" />
            <wix:RegistryValue Key="shell\open\command" Value="&quot;[ApplicationDirectory]$(var.SafeExamBrowser.Runtime.TargetFileName)&quot; &quot;%1&quot;" Type="string" />
        </wix:RegistryKey>
        <wix:RegistryKey Root="HKCR" Key="sebs">
            <wix:RegistryValue Value="URL:Safe Exam Browser Secure Protocol" Type="string" />
            <wix:RegistryValue Name="URL Protocol" Value="" Type="string" />
            <wix:RegistryValue Key="DefaultIcon" Value="[#ApplicationIconFile]" Type="string" />
            <wix:RegistryValue Key="shell\open\command" Value="&quot;[ApplicationDirectory]$(var.SafeExamBrowser.Runtime.TargetFileName)&quot; &quot;%1&quot;" Type="string" />
        </wix:RegistryKey>
    </xsl:template>
</xsl:stylesheet>