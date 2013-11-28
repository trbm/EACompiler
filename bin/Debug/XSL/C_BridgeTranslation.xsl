<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xs="http://www.w3.org/2001/XMLSchema" exclude-result-prefixes="xs" version="2.0">
    <xsl:output method="text" omit-xml-declaration="yes" indent="yes" xml:space="preserve"/>
    <xsl:variable name="XSL_VERSION" select="0.1"/>

    <xsl:template match="/Bridge">                                      <!-- start matching at the root  -->
// __________________________________________________
// 
//          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY 
// __________________________________________________
// 
//      XSL Transform: PYCCA_BridgeTranslation.xsl (v<xsl:value-of select="$XSL_VERSION"/>)
// __________________________________________________
// 
//            © Copyright 2012,  The Random Organization  All rights reserved.
// __________________________________________________
/**/
        <xsl:for-each select="IncludeFile">
#include "<xsl:value-of select="@filename"/>"            
        </xsl:for-each>
/**/
        <xsl:apply-templates  select="Domain"/>
    </xsl:template>

    <xsl:template match="Domain" mode="ACCEPTED_MESSAGES">             <!-- ______________________________________ Domain mode="ACCEPTED_MESSAGES" -->
// Domain '<xsl:value-of  select="@name"/>' Accepted Messages
        <xsl:for-each select="AcceptedMessages/Message">
 
   void <xsl:value-of  select="@name"/>_<xsl:value-of select="@name"/>(<xsl:call-template name="DATATYPE_PARAMETERS"/>) <xsl:call-template name="FUNCTION_DESCRIPTION"/>
   {<xsl:value-of select="ImplementationCode/text()"/>
   }
/**/
        </xsl:for-each>

/**/
      

    </xsl:template>

    <xsl:template name="FUNCTION_DESCRIPTION">                     <!-- ______________________________________ FUNCTION_DESCRIPTION -->
        <xsl:if test="string-length(@description) > 0">     // <xsl:value-of select="@description"/></xsl:if>
    </xsl:template>

    <xsl:template name="DATATYPE_PARAMETERS">                      <!-- ______________________________________ DATATYPE_PARAMETERS -->
        <xsl:for-each select="Parameter"> 
            <xsl:variable name="CURRENT_INDEX" select="position()"/>
            <xsl:choose>
                <xsl:when test="$CURRENT_INDEX = 1">  <xsl:value-of select="@dataType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>  
                </xsl:when>
                <xsl:when test="$CURRENT_INDEX > 1">, <xsl:value-of select="@dataType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>  
                </xsl:when>
            </xsl:choose>
        </xsl:for-each>
    </xsl:template> 
                                            
    

</xsl:stylesheet>
