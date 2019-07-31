<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:wix="http://schemas.microsoft.com/wix/2006/wi">
    <xsl:output method="xml" indent="yes"/>
    <xsl:template match="@*|node()">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()" />
        </xsl:copy>
    </xsl:template>
    <xsl:template match="wix:File[substring(@Source, string-length(@Source) - string-length('Service.exe') + 1) = 'Service.exe']">
        <xsl:copy>
            <xsl:apply-templates select="@*|node()" />
        </xsl:copy>
        <wix:ServiceInstall DisplayName="Safe Exam Browser Service" Name="SafeExamBrowser" Account="LocalSystem" ErrorControl="normal" Start="auto"
                            Type="ownProcess" Vital="yes" Interactive="no" Description="Performs operations which require elevated privileges." />
        <wix:ServiceControl Id="ServiceControl" Name="SafeExamBrowser" Start="install" Stop="uninstall" Remove="uninstall" Wait="yes" />
    </xsl:template>
</xsl:stylesheet>