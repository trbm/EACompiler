<?xml version="1.0" encoding="UTF-8"?>

<!--
   Note: the string /**/ appearing below are special markers to indicate blank lines
   which should not be suppressed at the end of the transformation. These are removed
   from the final .c file at the end of the transformation processing.
-->


<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    exclude-result-prefixes="xs"
    version="2.1">

    <xsl:output method="text" omit-xml-declaration="yes" indent="yes" xml:space="preserve"/>
    <xsl:variable name="XSL_VERSION" select="'30.0.0'"/>
    <xsl:variable name="CR_INDENT"><xsl:text>&#10;&#13;        </xsl:text></xsl:variable>

                                             <xsl:template match="/Model">                                  <!-- start matching at the root  -->

// ____________________________________________________________________________
// 
//          THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY 
// ____________________________________________________________________________
// 
//   Source directory: <xsl:value-of select="@sourceFileDirectory"/>
// 
//     Model filename: <xsl:value-of select="@modelFile"/>
// 
//  Intermediate file: <xsl:value-of select="@intermediateFile"/>
// 
//         Created by: Enterprise Architect 9 (EA Model Compiler v<xsl:value-of select="@EACompilerVersion"/>)
// 
//      XSL Transform: H_PYCCA.xsl (v<xsl:value-of select="$XSL_VERSION"/>)
//
//          Generated: <xsl:value-of select="@generated"/>
// ____________________________________________________________________________
// 
//            © Copyright 2012,  The Random Organization     All rights reserved.
// ____________________________________________________________________________
/**/
                                                    <xsl:apply-templates select="//Model/ModeledDomains/ModeledDomain"/>    
                                                        <xsl:call-template name="ACCEPTED_MESSAGES"/>                                      
                                             </xsl:template>   
        
                                             <xsl:template match="ModeledDomain">                           <!-- ______________________________________ ModeledDomain mode=DEFINITION -->
/**/  
#ifndef <xsl:value-of select="@name"/>_H
#define <xsl:value-of select="@name"/>_H
/**/ 
    #include "Types.h" 
/**/                                          
    void <xsl:value-of select="@name"/>_CreateInstancePopulation(void);  
                                                <xsl:call-template name="ACCEPTED_MESSAGES"/>
                                                <xsl:call-template name="ENUMERATIONS"/>
                                                <xsl:call-template name="TLV_NAMES"/>
                                                <xsl:call-template name="TLV_INDEXES"/>
/**/ 
/**/ 
#endif   // <xsl:value-of select="@name"/>_H                                               
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
                                            
                                             <xsl:template name="ACCEPTED_MESSAGES">        <!-- context: Domain ______________________________________ ACCEPTED_MESSAGES -->
                                                <xsl:for-each select="Bridging/AcceptedMessages">
      /**/
                                                    <xsl:for-each select="Message">
                                                        <xsl:variable name="RETURN_TYPE" select="ReturnValue/@dataType"/>
 /**/
    <xsl:value-of select="$RETURN_TYPE"/><xsl:text> </xsl:text> <xsl:value-of select="../../../@name"/>_<xsl:value-of select="@name"/>(<xsl:call-template name="DATATYPE_PARAMETERS"/>); <xsl:call-template name="FUNCTION_DESCRIPTION"/>
                                                    </xsl:for-each>
    /**/
                                                </xsl:for-each>
                                             </xsl:template>

                                             <xsl:template name="ENUMERATIONS">             <!-- context: Domain ______________________________________ ENUMERATIONS -->
                                                <xsl:for-each select="//Enumerations/Enumeration">
/**/
    typedef enum _<xsl:value-of select="@name"/>
    {  
<xsl:for-each select="Enumerator"><xsl:choose><xsl:when test="@initialValue"><xsl:text>        </xsl:text><xsl:value-of select="@name"/> = <xsl:value-of select="@initialValue"/>,     // initial value provided in the class diagram of the model
</xsl:when>
<xsl:otherwise><xsl:text>        </xsl:text><xsl:value-of select="@name"/>,    
</xsl:otherwise></xsl:choose>
        </xsl:for-each>
    } <xsl:value-of select="@name"/>;

                                                </xsl:for-each>
                                            </xsl:template> 

                                             <xsl:template name="TLV_NAMES">                <!-- context: Domain ______________________________________ TLV_NAMES -->
/**/
                                                    <xsl:variable name="DOMAIN_NAME" select="@name"/>
                                                    <xsl:for-each select="Classes/Class">
                                                    <xsl:variable name="CLASS_NAME" select="@name"/>
                                                        <xsl:if test="@minimumAllocation='1'">                                                   
                                                            <xsl:for-each select="Attributes/Attribute [@TLVhashcode != '0']">
    #define TLV_<xsl:value-of select="$DOMAIN_NAME"/>_<xsl:value-of select="$CLASS_NAME"/>_<xsl:value-of select="@name"/><xsl:text>      </xsl:text><xsl:value-of select="@TLVhashcode"/>
                                                            </xsl:for-each>
                                                        </xsl:if>
                                                </xsl:for-each>
                                            </xsl:template> 

                                             <xsl:template name="TLV_INDEXES">              <!-- context: Domain ______________________________________ TLV_INDEXES -->
/**/
                                                    <xsl:variable name="DOMAIN_NAME" select="@name"/>
                                                    <xsl:for-each select="Classes/Class/Attributes/Attribute [@TLVhashcode != '0']">
                                                        <xsl:variable name="CLASS_NAME" select="../../@name"/>
   #define TLV_<xsl:value-of select="$DOMAIN_NAME"/>_<xsl:value-of select="$CLASS_NAME"/>_<xsl:value-of select="@name"/>_INDEX  <xsl:value-of select="position() - 1"/> 
                                                    </xsl:for-each>
                                            </xsl:template> 


</xsl:stylesheet>

