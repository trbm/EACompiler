<?xml version="1.0" encoding="UTF-8"?>


<!--
   This file is a stylesheet (XSL) transform designed to produce an XML file specifying
   all the Tag Linked Value (TLV) items from a compiled model.
-->


<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
      xmlns:xs="http://www.w3.org/2001/XMLSchema"
      exclude-result-prefixes="xs"
      version="2.1">

    <xsl:variable name="XSL_VERSION" select="'27.10.5'"/>
    <xsl:output method="text" omit-xml-declaration="no" indent="yes" xml:space="preserve"/>
    <xsl:template match="/Model">&lt;?xml version="1.0" encoding="UTF-8"?&gt;
<![CDATA[<!--]]>
 ** ____________________________________________________________________________
 ** 
 **          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY 
 ** ____________________________________________________________________________
 ** 
 **   Source directory: <xsl:value-of select="@sourceFileDirectory"/>
 ** 
 **     Model filename: <xsl:value-of select="@modelFile"/>
 ** 
 **  Intermediate file: <xsl:value-of select="@intermediateFile"/>
 ** 
 **         Created by: Enterprise Architect 9 (EA Model Compiler v<xsl:value-of select="@EACompilerVersion"/>)
 ** 
 **      XSL Transform: TagLinkedValues.xsl (v<xsl:value-of select="$XSL_VERSION"/>)
 **
 **          Generated: <xsl:value-of select="@generated"/>
 ** ____________________________________________________________________________
 ** 
 **            © Copyright 2012,  The Random Organization     All rights reserved.
 ** ____________________________________________________________________________
<![CDATA[-->



<root xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="../../EACompiler/bin/Debug/XSL/TagLinkedValues.xsd">
    <TagLlinkValues> ]]>
        <xsl:for-each select=" //Class [@minimumAllocation='1']">
            <xsl:apply-templates  select="Attributes/Attribute [@TLVhashcode != '0']"/>
        </xsl:for-each>
<![CDATA[    </TagLlinkValues>
</root>]]>
    </xsl:template>  







    <xsl:template match="Attribute">        <!-- ____________________________________________ template: Attribute  -->
        <xsl:variable name="DOMAIN_NAME" select="../../../../@name"/>
        <xsl:variable name="CLASS_NAME" select="../../@name"/>
        <xsl:variable name="ATTRIBUTE_NAME" select="@name"/>
       <![CDATA[ <TagLinkValue tagId="TLV_]]><xsl:value-of select="$DOMAIN_NAME"/>_<xsl:value-of select="$CLASS_NAME"/>_<xsl:value-of select="$ATTRIBUTE_NAME"/>" domain="<xsl:value-of select="$DOMAIN_NAME"/>"    class="<xsl:value-of select="$CLASS_NAME"/>"  attribute="<xsl:value-of select="$ATTRIBUTE_NAME"/>"     attributeType="<xsl:value-of select="@dataType"/>"    hashcode="<xsl:value-of select="@TLVhashcode"/>"   persistent="<xsl:value-of select="@persistent"/>"  description="<xsl:value-of select="@description" disable-output-escaping="yes"/>"   <![CDATA[ />]]> 
    </xsl:template>
</xsl:stylesheet>
