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
    <xsl:variable name="XSL_VERSION" select="'30.0.0.17'"/>
    <xsl:variable name="CR_INDENT"><xsl:text>&#10;&#13;        </xsl:text></xsl:variable>

                                             <xsl:template match="/Model">                                  <!-- start matching at the root  -->


/*!
** \file <xsl:value-of select="@modelFile"/> (source model file)
**
** \warning THIS IS AN AUTOMATICALLY GENERATED FILE. DO NOT EDIT IT DIRECTLY 
** 
** \brief Code generated source implements domain: <xsl:value-of select="//ModeledDomain/@name"/> 
** 
**   Source directory: <xsl:value-of select="@sourceFileDirectory"/>
** 
**     Model filename: <xsl:value-of select="@modelFile"/>
** 
**  Intermediate file: <xsl:value-of select="@intermediateFile"/>
** 
**         Created by: Enterprise Architect 9 (EA Model Compiler v<xsl:value-of select="@EACompilerVersion"/>)
** 
**      XSL Transform: C.xsl (v<xsl:value-of select="$XSL_VERSION"/>)
**
**          Generated: <xsl:value-of select="@generated"/>**  
**
** \copyright (c) 2012, Array Power, Inc.  All Rights Reserved.
*/

#include &lt;stddef.h&gt;
#include &lt;stdbool.h&gt;
#include "PLATFORM.h"
/**/







static void* _SELF;	// the universal self reference, set by each state action at invocation
/**/
/**/
#define IsInstInUse(i)                      ((i)->common_.alloc != 0)
#define NEW(ClassName)	                    ((ClassName)mechInstCreate(&amp;ClassName ## _class, ClassName ## _INIITIAL_STATE))		// usage example:   Dog oDog = new(Dog);
#define FOREACH(ClassName, InstanceName)    ClassName InstanceName; for(InstanceName = ClassName ## _ ## BeginStorage; InstanceName != ClassName ## _ ## EndStorage; InstanceName++)     if( IsInstInUse(InstanceName) )
// usage example: FOREACH( Transmit, oTransmit ) { oTransmit->count++; }
/**/
#define IsInstanceInUse( pInstance )    ((pInstance)->common_.alloc != 0)

/**/                                                <xsl:apply-templates select="//Model/ModeledDomains/ModeledDomain"/>    
                                             </xsl:template>   
   
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// ModeledDomain mode=DEFINITION-->
                                             <xsl:template match="ModeledDomain">                            
/**/


/*!
** \ingroup sysBackground
** \defgroup LEVEL2_<xsl:value-of select="@name"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>
**
** The <xsl:value-of select="@name"/> class diagram:
** \image html EAID_<xsl:value-of select="Diagram/@diagramGUID"/>.gif
* @{
*/


#undef DEBUGTRACE    // redefining this useful macro to use the domain/module specific version with this domain specified "under the hood"
#define DEBUGTRACE(lev, txt, a1, a2, a3) TraceM(Domain_<xsl:value-of select="@name"/>, lev, txt, (unsigned long)(a1), (unsigned long)(a2), (unsigned long)(a3))
/**/

                                                <xsl:apply-templates select="//State[(@isIgnoreState = 'false') and (@isPigtailState = 'false') and (@isMeatballState = 'false')]" mode="PROTOTYPE"/>

                                                <xsl:call-template name="ALLOCATION_SIZES"/>

                                                <xsl:call-template name="PROTOTYPES"/>
                                                
                                                <xsl:apply-templates select="Classes/Class"/>
                                                                                                
                                                <xsl:call-template name="LINK_UNLINK"/>
 
                                                <xsl:apply-templates select="//State[(@isIgnoreState = 'false') and (@isPigtailState = 'false') and (@isMeatballState = 'false')]" mode="DEFINITION"/>
 
                                                <xsl:call-template name="INITIALIZE_MECHANISMS"/>

                                                <xsl:call-template name="TAG_LINKED_VALUES"/>

                                                <xsl:call-template name="INITIAL_INSTANCE_POPULATION"/>

                                                <xsl:call-template name="ACCEPTED_MESSAGES"/>

                                                <xsl:call-template name="CLASS_OPERATIONS"/>
/*! @} end of LEVEL2_<xsl:value-of select="@name"/> */
                                              </xsl:template>     

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// Class -->
                                             <xsl:template match="Class">                                   
                                                 <xsl:variable name="CONTEXT_CLASS" select="."/>
                                                 <xsl:variable name="NORMAL_STATES" select="States/State[ @isMeatballState ='false' and @isPigtailState ='false' and @isIgnoreState='false']"/>
                                                 <xsl:choose>
                                                     <xsl:when test="./Stereotypes/Stereotype [@name = 'OMIT']">
/**/
//
// Omit class (by stereotype): <xsl:value-of select="@name"/>
//
                                                     </xsl:when>
                                                 <xsl:otherwise>   
/*!
** \defgroup LEVEL3_<xsl:value-of select="../../@name"/>_<xsl:value-of select="@name"/><xsl:text> Class: </xsl:text><xsl:value-of select="@name"/>
**
** <xsl:value-of select="@description"/>
**
<xsl:if test="string-length(@stateDiagramGUID)">
** The <xsl:value-of select="@name"/> state model diagram:
** \image html EAID_<xsl:value-of select="@stateDiagramGUID"/>.gif
</xsl:if>
** @{
*/
/**/ 
/**/  
    /*!
    ***********************************************************************************************
    **
    ** Class: <xsl:value-of select="@name"/>   (instance allocation: <xsl:value-of select="@minimumAllocation"/>)
    ** 
    ** \brief <xsl:value-of select="@description"/>
    **
    */

    #define <xsl:value-of select="@name"/>_CLASSID   <xsl:value-of select="@classID"/>L                                 
/**/    
    /*! /brief the structure representing a single instance of class '<xsl:value-of select="@name"/>' */
    typedef struct _<xsl:value-of select="@name"/>
    {
        struct mechinstance common_ ; 
    <xsl:for-each select="Attributes/Attribute">
        <xsl:text>    </xsl:text><xsl:value-of select="@dataType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>;<xsl:text>                              </xsl:text>/*!&gt; <xsl:value-of select="@description"/> */
    </xsl:for-each><xsl:call-template name="REFERENTIAL_ATTRIBUTES"/> 
    } *<xsl:value-of select="@name"/>;  /*!&lt; the class name is a 'type' represented as a pointer to the instance storage/state structure */
/**/
    <xsl:variable name="ACTION_FUNCTION_COUNT" select="count(States/State[(@isIgnoreState = 'false') and (@isPigtailState = 'false') and (@isMeatballState = 'false') ])"/> 


    static struct objectdispatchblock <xsl:value-of select="@name"/>_odb;                        // RAM allocations (initialized below)                       
    static struct mechclass <xsl:value-of select="@name"/>_class;                                                          
    static struct installocblock <xsl:value-of select="@name"/>_iab;                                                        
    <xsl:if test="$ACTION_FUNCTION_COUNT > 0">
    static PtrActionFunction <xsl:value-of select="@name"/>_acttbl[ <xsl:value-of select="$ACTION_FUNCTION_COUNT"/> ];      
    </xsl:if>

                                                    <xsl:call-template name="DESTROY_SELF"/> 
/**/
                                                         <xsl:if test="count(States/State[ @isMeatballState ='false' and @isPigtailState ='false' and @isIgnoreState='false']) > 0">                                                    



/**/
     /*! /brief enumerator for all states of class '<xsl:value-of select="@name"/>'   */
     enum <xsl:value-of select="@name"/>_STATE                  
     {
     <xsl:for-each select="$NORMAL_STATES">
<xsl:text>    </xsl:text><xsl:value-of select="../../@name"/>_<xsl:value-of select="@name"/>_STATE = <xsl:value-of select="position()-1"/>,
     </xsl:for-each><xsl:text>    </xsl:text><xsl:value-of select="@name"/>_STATE_Count = <xsl:value-of select="count($NORMAL_STATES)"/>
     };
/**/
      /*! /brief enumerator for all events handled by class '<xsl:value-of select="@name"/>'   */   
     enum <xsl:value-of select="@name"/>_EVENT         
     {
     <xsl:for-each select="Events/Event"><xsl:text>    </xsl:text><xsl:value-of select="../../@name"/>_<xsl:value-of select="@name"/>_EVENT = <xsl:value-of select="position()-1"/>,
     </xsl:for-each><xsl:text>    </xsl:text><xsl:value-of select="@name"/>_EVENT_Count = <xsl:value-of select="count(Events/Event)"/> 
     };
/**/
                                                        </xsl:if>    
    <xsl:variable name="INITIAL_STATE_NAME"  select="States/State[@isInitialState = 'true']/@name"/> 
    <xsl:choose>
        <xsl:when test="string-length($INITIAL_STATE_NAME) > 0">
    #define <xsl:value-of  select="@name"/>_INIITIAL_STATE  <xsl:value-of select="@name"/>_<xsl:value-of select="$INITIAL_STATE_NAME"/>_STATE
        </xsl:when>
        <xsl:otherwise>
    #define <xsl:value-of  select="@name"/>_INIITIAL_STATE  0
        </xsl:otherwise>
    </xsl:choose>
                                                     <xsl:if test="count(Events/Event) > 0">     <!-- if this class has a state model -->
                                                            <xsl:apply-templates select="Events/Event"/>
/**/    
    /*! /brief the state transition table for class '<xsl:value-of select="@name"/>'   */                                                        
    static StateCode <xsl:value-of select="@name"/>_TransitionTable[] =            
    {  
<xsl:for-each select="$CONTEXT_CLASS/Events/Event">
    <xsl:variable name="CONTEXT_EVENT" select="."/>
    <xsl:variable name="CONTEXT_EVENT_ELEMENT_ID" select="@eventElementID"/>

    <xsl:value-of select="$CR_INDENT"/>// <xsl:value-of select="$CONTEXT_EVENT_ELEMENT_ID"/>
    <xsl:for-each select="$CONTEXT_CLASS/States/State [(@isIgnoreState = 'false') and (@isPigtailState = 'false') and (@isMeatballState = 'false')]">
        <xsl:variable name="SOURCE_STATE" select="."/>        
        <xsl:variable name="SOURCE_STATE_NAME" select="$SOURCE_STATE/@name"/>
        
        <xsl:variable name="TRANSITION" select="$CONTEXT_CLASS/Transitions/Transition [(SourceSideState/@name = $SOURCE_STATE_NAME) and (@eventElementID = $CONTEXT_EVENT_ELEMENT_ID) ]"/>
        <xsl:variable name="PIGTAIL_TRANSITION" select="$CONTEXT_CLASS/Transitions/Transition [ (@isPigtailTransition = 'true') and (@eventElementID = $CONTEXT_EVENT_ELEMENT_ID) ]"/>     

        
        <xsl:choose>
            <xsl:when test="$TRANSITION/TargetSideState/@isIgnoreState = 'true'">
                        <xsl:value-of select="$CR_INDENT"/>MECH_STATECODE_IG,
            </xsl:when>

            <xsl:when test="$TRANSITION">
                        <xsl:value-of select="$CR_INDENT"/><xsl:value-of select="$CONTEXT_CLASS/@name"/>_<xsl:value-of select="$TRANSITION/TargetSideState/@name"/>_STATE,                                                 // normal:  <xsl:value-of select="$SOURCE_STATE_NAME"/>  -->  <xsl:value-of select="$TRANSITION/@eventElementID"/>  -->  <xsl:value-of select="$TRANSITION/TargetSideState/@name"/>
            </xsl:when>
        
            <xsl:when test="$PIGTAIL_TRANSITION">
                       <xsl:value-of select="$CR_INDENT"/><xsl:value-of select="$CONTEXT_CLASS/@name"/>_<xsl:value-of select="$PIGTAIL_TRANSITION/TargetSideState/@name"/>_STATE,                                                 // pigtail:  <xsl:value-of select="$SOURCE_STATE_NAME"/>  
            </xsl:when>

            <xsl:otherwise>
                        <xsl:value-of select="$CR_INDENT"/>MECH_STATECODE_CH,    
            </xsl:otherwise>
        </xsl:choose>




    </xsl:for-each>
</xsl:for-each>
    } ;
    
                                                            <xsl:call-template name="ACCESSORS"/> 
/**/ 
                                                             </xsl:if>
/**/
                                                            
                                                
                                                     </xsl:otherwise>
                                                 </xsl:choose>

                                                 <xsl:call-template name="INIT_MECHANISMS"/>
/*! @} end of group: LEVEL3_<xsl:value-of select="../../@name"/>_<xsl:value-of select="@name"/> */
                                             </xsl:template>
    
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// LINK_UNLINK-->
                                             <xsl:template name="LINK_UNLINK">      
/*!
** \addtogroup LEVEL3_<xsl:value-of select="@name"/>_LINK_UNLINK Linking and Unlinking
** @{
*/                                              <xsl:call-template name="RELATE"/>
                                                <xsl:call-template name="UNRELATE_ALL"/>
/*! @} end of LEVEL3_<xsl:value-of select="@name"/>_LINK_UNLINK */
/**/
/**/   
                                             </xsl:template>   

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// State mode=DEFINITION-->
                                             <xsl:template match="State" mode="DEFINITION">                  
/*!
** \addtogroup LEVEL3_<xsl:value-of select="../../../../@name"/>_<xsl:value-of select="../../@name"/>
** @{
*/
/**/
    //_____________________________________________________________
    /*! /brief state action '`<xsl:value-of  select="@name" />' of class '<xsl:value-of  select="../../@name" />' */
    static void <xsl:value-of  select="../../@name" />_<xsl:value-of  select="@name" />(void *const s_, void *const p_)   
<xsl:choose>
    <xsl:when test="@isQuiet = 'true'">   {  static <xsl:value-of  select="../../@name" /> self; self = (<xsl:value-of  select="../../@name" />) s_; _SELF = self;  <xsl:call-template name="EVENT_PARAMETERS"/></xsl:when>
    <xsl:otherwise>   
    {  static <xsl:value-of  select="../../@name" /> self; self = (<xsl:value-of  select="../../@name" />) s_;  _SELF = self;  <xsl:call-template name="EVENT_PARAMETERS"/></xsl:otherwise>
</xsl:choose>  
       //printf("entered state: <xsl:value-of  select="@name" />\n\r");   // uncomment this line to announce this state  
       //DEBUGTRACE(DEFAULT_TRACE_VOLUME, "<xsl:value-of select="@stateNumber"/>", 0, 0, 0);
/**/
<xsl:value-of select="text()"/>       
    }
/*! @} */
/**/

                                             </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// State mode=PROTOTYPE-->
                                             <xsl:template match="State" mode="PROTOTYPE">            
/*!
** \addtogroup LEVEL3_<xsl:value-of select="../../../../@name"/>_<xsl:value-of select="../../@name"/>
** @{
*/
    static void <xsl:value-of  select="../../@name" />_<xsl:value-of  select="@name" />(void *const s_, void *const p_);
/*! @} */
                                             </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// Event-->
                                             <xsl:template match="Event">                                    
                                                <xsl:if test="count(Parameter) > 0">
    typedef struct _<xsl:value-of select="../../@name"/>_<xsl:value-of select="@name"/>_eventData 
    {
        <xsl:for-each select="Parameter"><xsl:value-of select="@dataType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>;
        </xsl:for-each> 
    } <xsl:value-of select="../../@name"/>_<xsl:value-of select="@name"/>_eventData;
/**/
                                                                                                       
                                                </xsl:if> 
                                             </xsl:template>  
    
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// ALLOCATION_SIZES-->
                                             <xsl:template name="ALLOCATION_SIZES">                         
                                                 <xsl:for-each select="//Relationship">
                                                     <xsl:variable name="OTHER_CLASS_NAME" select="ThatSide/@className"/>
                                                     <xsl:variable name="THIS_CLASS_NAME" select="ThisSide/@className"/>
                                                     <xsl:variable name="OTHER_CLASS_MINIMUM_ALLOCATION" select="//Class [@name=$OTHER_CLASS_NAME]/@minimumAllocation"/>
    #define <xsl:value-of select="@name"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>s_COUNT <xsl:value-of select="$OTHER_CLASS_MINIMUM_ALLOCATION"/>      // allocated number of <xsl:value-of select="$OTHER_CLASS_NAME"/> instances  
                                                     </xsl:for-each>
/**/
                                             </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// REFERENTIAL_ATTRIBUTES (context: Class) -->
                                             <xsl:template name="REFERENTIAL_ATTRIBUTES">                    
                                                <xsl:variable name="CLASS_NAME" select="@name"/>
                                                <xsl:for-each select="Supertype">
                                                    <xsl:variable name="SUPERTYPE_NAME" select="@name"/>
        struct _<xsl:value-of select="$SUPERTYPE_NAME"/>* SUPERTYPE_<xsl:value-of select="$SUPERTYPE_NAME"/>;                                 // generalization (isA) -- points to this class's subtype
                                                </xsl:for-each>

                                                <xsl:for-each select="//Supertype[@name = $CLASS_NAME]">    <!-- am i anyone's supertype? -->       
                                                    <xsl:variable name="SUBTYPE_NAME" select="../@name"/>                                 
        struct _<xsl:value-of select="$SUBTYPE_NAME"/>* SUBTYPE_<xsl:value-of select="$SUBTYPE_NAME"/>;                                    // specialization (isA) -- exactly ONE of these pointers should be non-null
                                                </xsl:for-each>

                                                <xsl:for-each select="Relationships/Relationship">
                                                     <xsl:variable name="RELATIONSHIP_NAME" select="@name"/>
                                                     <xsl:variable name="OTHER_CLASS_NAME" select="ThatSide/@className"/>
                                                     <xsl:variable name="OTHER_CLASS_MINIMUM_ALLOCATION" select="//Class [@name=$OTHER_CLASS_NAME]/@minimumAllocation"/>
                                                     <xsl:variable name="REF_ATTRIBUTE_NAME" select="concat(concat($RELATIONSHIP_NAME, '_'),$OTHER_CLASS_NAME)"/>
                                                    <xsl:choose>
                                                        <xsl:when test="ThatSide/@isMany='false'">
        struct _<xsl:value-of select="$OTHER_CLASS_NAME"/>* <xsl:value-of select="$REF_ATTRIBUTE_NAME"/>;                                         // referential attribute </xsl:when>
                                                        <xsl:when test="ThatSide/@isMany='true'">
        struct _<xsl:value-of select="$OTHER_CLASS_NAME"/>* <xsl:value-of select="$REF_ATTRIBUTE_NAME"/>s[<xsl:value-of select="$OTHER_CLASS_MINIMUM_ALLOCATION"/>];    // referential attribute list </xsl:when>
                                                    </xsl:choose>
                                                 </xsl:for-each>
                                             </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// ACCESSORS-->
                                             <xsl:template name="ACCESSORS">                                 
    bool IsInstanceOf_<xsl:value-of select="@name"/>(MechInstance oInstance)                                           { return ((void*)<xsl:value-of select="@name"/>_BeginStorage &lt;= (void*)oInstance) &amp;&amp; ((void*)oInstance &lt; (void*)<xsl:value-of select="@name"/>_EndStorage); }
/**/    

                                                 <xsl:for-each select="Events/Event">   
/**/                                   
    // <xsl:value-of select="@name"/>  .................................................................................  event transmission variants
/**/
    /*! /brief send event '<xsl:value-of select="@name"/>' to the specified target instance  */   
    void <xsl:value-of select="@name"/>_Send(<xsl:value-of select="../../@name"/> targetInstance<xsl:call-template name="DATATYPE_PARAMETERS_LEADING_COMMA"/>)                         
    { 
        MechEcb newEvent = mechEventNew(<xsl:value-of select="../../@name"/>_<xsl:value-of select="@name"/>_EVENT, (MechInstance)targetInstance, 0);
        <xsl:for-each select="Parameter">((<xsl:value-of select="../../../@name"/>_<xsl:value-of select="../@name"/>_eventData*)(&amp;(newEvent)->eventParameters))-><xsl:value-of select="@name"/> = <xsl:value-of select="@name"/>;
        </xsl:for-each>       
        mechEventPost( newEvent ); 
    }
/**/
    /*! /brief send a delayed event '<xsl:value-of select="@name"/>' to the specified target instance  */
    void <xsl:value-of select="@name"/>_SendDelayed(<xsl:value-of select="../../@name"/> targetInstance, CLICKS_U32 delayClicks<xsl:call-template name="DATATYPE_PARAMETERS_LEADING_COMMA"/>)                             
    { 
        MechEcb newEvent = mechEventNew(<xsl:value-of select="../../@name"/>_<xsl:value-of select="@name"/>_EVENT, (MechInstance)targetInstance, 0);
        <xsl:for-each select="Parameter">((<xsl:value-of select="../../../@name"/>_<xsl:value-of select="../@name"/>_eventData*)(&amp;(newEvent)->eventParameters))-><xsl:value-of select="@name"/> = <xsl:value-of select="@name"/>;
        </xsl:for-each>       
        mechEventPostDelay( newEvent, delayClicks ); 
    }
/**/
    /*! /brief Send event '<xsl:value-of select="@name"/>' to self */
    void <xsl:value-of select="@name"/>_SendToSelf(<xsl:call-template name="DATATYPE_PARAMETERS"/>) { <xsl:value-of select="@name"/>_Send( _SELF<xsl:call-template name="PARAMETERS_LEADING_COMMA"/>); }   
   
    /*! /brief Send a delayed event '<xsl:value-of select="@name"/>' to the specified target instance (with the delay expressed in native clock ticks)   */  
    void <xsl:value-of select="@name"/>_SendDelayedToSelf(CLICKS_U32 delayClicks<xsl:call-template name="DATATYPE_PARAMETERS_LEADING_COMMA"/>) { <xsl:value-of select="@name"/>_SendDelayed( _SELF, delayClicks<xsl:call-template name="PARAMETERS_LEADING_COMMA"/>); }    

    /*! /brief Send a delayed event '<xsl:value-of select="@name"/>' to the specified target instance (with the delay expressed in milliseconds)   */ 
    void <xsl:value-of select="@name"/>_SendDelayedToSelf_ms(long milliseconds<xsl:call-template name="DATATYPE_PARAMETERS_LEADING_COMMA"/>) { <xsl:value-of select="@name"/>_SendDelayed( _SELF, MsecToClicks( milliseconds )<xsl:call-template name="PARAMETERS_LEADING_COMMA"/>); }  

    /*! /brief Kill zero or more delayed events '<xsl:value-of select="@name"/>' to self waiting in the delayed event queue */
    void <xsl:value-of select="@name"/>_CancelDelayedToSelf()  { assert(_SELF != NULL); mechEventDelayCancel( <xsl:value-of select="../../@name"/>_<xsl:value-of select="@name"/>_EVENT, _SELF, 0 ); }   

                                               </xsl:for-each>                                                                        
                                             </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// DATATYPE_PARAMETERS_LEADING_COMMA-->
                                             <xsl:template name="DATATYPE_PARAMETERS_LEADING_COMMA">         
                                                 <xsl:for-each select="Parameter">, <xsl:value-of select="@dataType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>  
                                                 </xsl:for-each>
                                             </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// DATATYPE_PARAMETERS-->
                                             <xsl:template name="DATATYPE_PARAMETERS">                       
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
                                            
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// DATATYPE_PARAMETERS_VOID-->
                                             <xsl:template name="DATATYPE_PARAMETERS_VOID">                  
                                               <xsl:choose>
                                                   <xsl:when test="count(Parameter) = 0">void</xsl:when>
                                                   <xsl:otherwise>
                                                       <xsl:for-each select="Parameter"> 
                                                         <xsl:variable name="CURRENT_INDEX" select="position()"/>
                                                         <xsl:choose>
                                                             <xsl:when test="$CURRENT_INDEX = 1">  <xsl:value-of select="@dataType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>  
                                                             </xsl:when>
                                                             <xsl:when test="$CURRENT_INDEX > 1">, <xsl:value-of select="@dataType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>  
                                                             </xsl:when>
                                                         </xsl:choose>
                                                       </xsl:for-each>
                                                   </xsl:otherwise>
                                                </xsl:choose>
                                             </xsl:template> 

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// METHOD_PARAMETER_STRING_LEADING_COMMA-->
                                             <xsl:template name="PARAMETERS_LEADING_COMMA">                  
                                                 <xsl:for-each select="Parameter">, <xsl:value-of select="@name"/>  
                                                 </xsl:for-each>
                                             </xsl:template>
                                            
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// PARAMETERS-->
                                             <xsl:template name="PARAMETERS">                                
                                                 <xsl:for-each select="Parameter"> 
                                                     <xsl:variable name="CURRENT_INDEX" select="position()"/>
                                                     <xsl:choose>
                                                         <xsl:when test="$CURRENT_INDEX = 1">  <xsl:value-of select="@name"/>  
                                                         </xsl:when>
                                                         <xsl:when test="$CURRENT_INDEX > 1">, <xsl:value-of select="@name"/>  
                                                         </xsl:when>
                                                     </xsl:choose>
                                                 </xsl:for-each>
                                             </xsl:template> 
   
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// DESTROY_SELF-->
                                             <xsl:template name="DESTROY_SELF">                              
                                                <xsl:variable name="CLASS_NAME" select="@name"/>
                                                <xsl:variable name="DOMAIN_NAME" select="../../@name"/>
/**/
    static void <xsl:value-of select="$CLASS_NAME"/>_Destroy_Instance( <xsl:value-of select="$CLASS_NAME"/> o<xsl:value-of select="$CLASS_NAME"/> )    // unlink from any/all other instances and remove self from the class collection
    {
                                                 <xsl:for-each select="//Relationship">
                                                     <xsl:variable name="RELATIONSHIP_NAME" select="@name"/>
                                                     <xsl:variable name="OTHER_CLASS_NAME" select="ThatSide/@className"/>
                                                     <xsl:variable name="THIS_CLASS_NAME" select="ThisSide/@className"/>
        <xsl:if test="$THIS_CLASS_NAME = $CLASS_NAME">
        unrelateAll_<xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>(o<xsl:value-of select="$THIS_CLASS_NAME"/>);
        </xsl:if>       
                                                 </xsl:for-each>
        mechInstDestroy( (MechInstance) o<xsl:value-of select="$CLASS_NAME"/> );
    }                                           </xsl:template>                                
                                            
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// INITIALIZE_MECHANISMS-->
                                             <xsl:template name="INITIALIZE_MECHANISMS">                  
/**/
    /*! /brief invoke at runtime the initializers for each class in domain '<xsl:value-of select="//ModeledDomain/@name"/>' */
    static void initializeMechanism()              
    {                                             
                                                <xsl:for-each select="Classes/Class">
                                                  <!--  <xsl:if test="count(States/State[ @isMeatballState ='false' and @isPigtailState ='false' and @isIgnoreState='false']) > 0">-->
        initializeMechanisms_<xsl:value-of select="@name"/>();
                                                  <!--  </xsl:if>-->
                                                </xsl:for-each>
    }
/**/
                                             </xsl:template>                                
                                            
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// PROTOTYPES-->
                                             <xsl:template name="PROTOTYPES">                                
/**/
                                                 <xsl:for-each select="//Class">
    typedef struct _<xsl:value-of select="@name"/> *<xsl:value-of select="@name"/>;
                                                  </xsl:for-each>    
/**/ 
/*!
** \addtogroup LEVEL3_HELPER_FUNCTIONS
** @{
*/
                                                 <xsl:for-each select="//Operation">

    static <xsl:value-of select="@returnType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>(<xsl:call-template name="DATATYPE_PARAMETERS"/>);

                                                 </xsl:for-each>
                                                 <xsl:for-each select="//Relationship">
                                                     <xsl:variable name="RELATIONSHIP_NAME" select="@name"/>
                                                     <xsl:variable name="OTHER_CLASS_NAME" select="ThatSide/@className"/>
                                                     <xsl:variable name="THIS_CLASS_NAME" select="ThisSide/@className"/>
                                                     <xsl:variable name="OTHER_CLASS_MINIMUM_ALLOCATION" select="//Class [@name=$OTHER_CLASS_NAME]/@minimumAllocation"/>
    static void unrelateAll_<xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>(<xsl:value-of select="$THIS_CLASS_NAME"/> o<xsl:value-of select="$THIS_CLASS_NAME"/>);
                                                 </xsl:for-each>
/*! @}  end of LEVEL3_HELPER_FUNCTIONS */                                                  
/**//**/
                                                 <xsl:for-each select="//Relationship">
                                                     <xsl:variable name="RELATIONSHIP_NAME" select="@name"/>
                                                     <xsl:variable name="OTHER_CLASS_NAME" select="ThatSide/@className"/>
                                                     <xsl:variable name="THIS_CLASS_NAME" select="ThisSide/@className"/>
                                                     <xsl:variable name="OTHER_CLASS_MINIMUM_ALLOCATION" select="//Class [@name=$OTHER_CLASS_NAME]/@minimumAllocation"/>
                                                     <xsl:variable name="REF_ATTRIBUTE_NAME" select="concat(concat($RELATIONSHIP_NAME, '_'),$OTHER_CLASS_NAME)"/>
/*!
** \addtogroup LEVEL3_<xsl:value-of select="../../../../@name"/>_LINK_UNLINK
** @{
*/
    static void relate_<xsl:value-of select="$REF_ATTRIBUTE_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>(<xsl:value-of select="$THIS_CLASS_NAME"/> self, <xsl:value-of select="$OTHER_CLASS_NAME"/> o<xsl:value-of select="$OTHER_CLASS_NAME"/>);
    static void unrelate_<xsl:value-of select="$REF_ATTRIBUTE_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>(<xsl:value-of select="$THIS_CLASS_NAME"/> self, <xsl:value-of select="$OTHER_CLASS_NAME"/> o<xsl:value-of select="$OTHER_CLASS_NAME"/>);
/*! @} */                                 
                                                 </xsl:for-each>

                                                 <xsl:for-each select="//Class">
/*!
** \addtogroup LEVEL3_<xsl:value-of select="../../@name"/>_<xsl:value-of select="@name"/>
** @{
*/
    /*! destroy (return to the free pool) a particular instance of class '<xsl:value-of select="@name"/>' */
    static void <xsl:value-of select="@name"/>_Destroy_Instance( <xsl:value-of select="@name"/> targetInstance );    
/*! @} */
                                                 </xsl:for-each>

                                                 <xsl:for-each select="Classes/Class"> 
/**/
/*!
** \addtogroup LEVEL3_<xsl:value-of select="../../@name"/>_<xsl:value-of select="@name"/>
** @{
*/
    void <xsl:value-of select="@name"/>_Destroy_Instance( <xsl:value-of select="@name"/> targetInstance );  
/*! @} */
                                                 </xsl:for-each>
                                                 <xsl:for-each select="Classes/Class/Events/Event"> 
/**/
/*!
** \addtogroup LEVEL3_<xsl:value-of select="../../../../@name"/>_<xsl:value-of select="../../@name"/>
** @{
*/
    void <xsl:value-of select="@name"/>_Send(<xsl:value-of select="../../@name"/> targetInstance<xsl:call-template name="DATATYPE_PARAMETERS_LEADING_COMMA"/>);
    void <xsl:value-of select="@name"/>_SendDelayed(<xsl:value-of select="../../@name"/> targetInstance, CLICKS_U32 delayClicks<xsl:call-template name="DATATYPE_PARAMETERS_LEADING_COMMA"/>);                            
    void <xsl:value-of select="@name"/>_SendToSelf(<xsl:call-template name="DATATYPE_PARAMETERS_VOID"/>);                          
    void <xsl:value-of select="@name"/>_SendDelayedToSelf(CLICKS_U32 delayClicks<xsl:call-template name="DATATYPE_PARAMETERS_LEADING_COMMA"/>); 
    void <xsl:value-of select="@name"/>_CancelDelayedToSelf(void);
/*! @} */
                                                 </xsl:for-each>
                                                 <xsl:for-each select="Classes/Class"> 
/**/
/*!
** \addtogroup LEVEL3_<xsl:value-of select="../../@name"/>_<xsl:value-of select="@name"/>
** @{
*/
    #define <xsl:value-of select="@name"/>s_ALLOCATION_COUNT <xsl:value-of select="@minimumAllocation"/>                                                     
    static  struct _<xsl:value-of select="@name"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>_storage[<xsl:value-of select="@name"/>s_ALLOCATION_COUNT];
    #define <xsl:value-of select="@name"/>_BeginStorage      (<xsl:value-of select="@name"/>_storage)
    #define <xsl:value-of select="@name"/>_EndStorage        (<xsl:value-of select="@name"/>_storage + (sizeof(<xsl:value-of select="@name"/>_storage) / sizeof(<xsl:value-of select="@name"/>_storage[0])))                                                         
<xsl:if test="@minimumAllocation='1'">
    #define <xsl:value-of select="@name"/>_SINGLETON         ( (<xsl:value-of select="@name"/>)&amp;(<xsl:value-of select="@name"/>_storage[0]) )                 
</xsl:if>
/*! @} */
                                                 </xsl:for-each>                                             
                                            </xsl:template> 
                                         
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// RELATE (context: Class) -->
                                             <xsl:template name="RELATE">  
<!--/*!
** \defgroup LEVEL3_LINK_UNLINK Linking and Unlinking Operations
** @{
*/-->
/**/ 
/**/ 
                                                <xsl:for-each select="//Supertype">    
                                                    <xsl:call-template name="RELATE_SUBTYPE_SUPERTYPE"/>       
                                                 </xsl:for-each>
                              
                                                <xsl:for-each select="//Relationship">
                                                    <xsl:variable name="RELATIONSHIP_NAME" select="@name"/>
                                                    <xsl:variable name="OTHER_CLASS_NAME" select="ThatSide/@className"/>
                                                    <xsl:variable name="THIS_CLASS_NAME" select="ThisSide/@className"/>
                                                    <xsl:choose>
                                                          <xsl:when test="@isReflexive='true'">
/**/
    // Note: Class '<xsl:value-of select="$THIS_CLASS_NAME"/>' has a reflexive relationship '<xsl:value-of select="$RELATIONSHIP_NAME"/>' -- relate and unrelate operations have been omitted to prevent confusion -- just handle the relationship "by hand"
    static void relate_<xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>(<xsl:value-of select="$OTHER_CLASS_NAME"/> o<xsl:value-of select="$OTHER_CLASS_NAME"/>, <xsl:value-of select="$THIS_CLASS_NAME"/> o<xsl:value-of select="$THIS_CLASS_NAME"/>1)
    {
    }
    static void unrelate_<xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>(<xsl:value-of select="$OTHER_CLASS_NAME"/> o<xsl:value-of select="$OTHER_CLASS_NAME"/>, <xsl:value-of select="$THIS_CLASS_NAME"/> o<xsl:value-of select="$THIS_CLASS_NAME"/>1)
    {
    }
/**/

                                                      </xsl:when>
                                                          <xsl:otherwise>
                                                               <xsl:call-template name="RELATE_NONREFLEXIVE"/>
                                                               <xsl:call-template name="UNRELATE_NONREFLEXIVE"/>
                                                          </xsl:otherwise>
                                                      </xsl:choose>
                                                </xsl:for-each>
<!--/*! @} end of group: LEVEL3_LINK_UNLINK Linking and Unlinking Operations */-->
                                            </xsl:template>                
                                             
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// RELATE_SUBTYPE_SUPERTYPE (context: Supertype) -->
                                             <xsl:template name="RELATE_SUBTYPE_SUPERTYPE">     
                                                <xsl:variable name="CLASS_NAME" select="../@name"/>
    /**/                                               
    static void relate_SUBTYPE_<xsl:value-of select="$CLASS_NAME"/>( <xsl:value-of select="$CLASS_NAME"/> o<xsl:value-of select="$CLASS_NAME"/>, <xsl:value-of select="@name"/> o<xsl:value-of select="@name"/>)
    {
        o<xsl:value-of select="$CLASS_NAME"/>->SUPERTYPE_<xsl:value-of select="@name"/> = o<xsl:value-of select="@name"/>;         
        o<xsl:value-of select="@name"/>->SUBTYPE_<xsl:value-of select="$CLASS_NAME"/> = o<xsl:value-of select="$CLASS_NAME"/>;
   }
                                             </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// RELATE_NONREFLEXIVE (context: Relationship) -->
                                             <xsl:template name="RELATE_NONREFLEXIVE">     
                                                     <xsl:variable name="RELATIONSHIP_NAME" select="@name"/>
                                                     <xsl:variable name="OTHER_CLASS_NAME" select="ThatSide/@className"/>
                                                     <xsl:variable name="THIS_CLASS_NAME" select="ThisSide/@className"/>
/**/
    static void relate_<xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>(<xsl:value-of select="$THIS_CLASS_NAME"/> o<xsl:value-of select="$THIS_CLASS_NAME"/>, <xsl:value-of select="$OTHER_CLASS_NAME"/> o<xsl:value-of select="$OTHER_CLASS_NAME"/>)
    {
<xsl:choose>
    <xsl:when test="ThatSide/@isMany = 'false'">    
        o<xsl:value-of select="$THIS_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/> = o<xsl:value-of select="$OTHER_CLASS_NAME"/>;
    </xsl:when>
    <xsl:otherwise>
        for(int i=0; i &lt; <xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>s_COUNT; i++ )
        {
            if( o<xsl:value-of select="$THIS_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>s[i] == NULL)
            {
                o<xsl:value-of select="$THIS_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>s[i] = o<xsl:value-of select="$OTHER_CLASS_NAME"/>;
                break;
            }
        }    
    </xsl:otherwise>
</xsl:choose>
<xsl:choose>
    <xsl:when test="ThisSide/@isMany = 'false'">    
        o<xsl:value-of select="$OTHER_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/> = o<xsl:value-of select="$THIS_CLASS_NAME"/>;
    </xsl:when>
    <xsl:otherwise>
        for(int i=0; i &lt; <xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>s_COUNT; i++ )
        {
            if( o<xsl:value-of select="$OTHER_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>s[i] == NULL)
            {
                o<xsl:value-of select="$OTHER_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>s[i] = o<xsl:value-of select="$THIS_CLASS_NAME"/>;
                break;
            }
        }    
    </xsl:otherwise>
</xsl:choose>
    }
                                             </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// UNRELATE_NONREFLEXIVE (context: Relationship) -->
                                             <xsl:template name="UNRELATE_NONREFLEXIVE">   
                                                     <xsl:variable name="RELATIONSHIP_NAME" select="@name"/>
                                                     <xsl:variable name="OTHER_CLASS_NAME" select="ThatSide/@className"/>
                                                     <xsl:variable name="THIS_CLASS_NAME" select="ThisSide/@className"/>
/**/
    static void unrelate_<xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>(<xsl:value-of select="$THIS_CLASS_NAME"/> o<xsl:value-of select="$THIS_CLASS_NAME"/>, <xsl:value-of select="$OTHER_CLASS_NAME"/> o<xsl:value-of select="$OTHER_CLASS_NAME"/>)
    {
<xsl:choose>
    <xsl:when test="ThatSide/@isMany = 'false'">    
        o<xsl:value-of select="$THIS_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/> = NULL;
    </xsl:when>
    <xsl:otherwise>
        for(int i=0; i &lt; <xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>s_COUNT; i++ )
        {
            if( o<xsl:value-of select="$THIS_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>s[i] == o<xsl:value-of select="$OTHER_CLASS_NAME"/>)
            {
                o<xsl:value-of select="$THIS_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>s[i] = NULL;
                break;
            }
        }    
    </xsl:otherwise>
</xsl:choose>
<xsl:choose>
    <xsl:when test="ThisSide/@isMany = 'false'">    
         o<xsl:value-of select="$OTHER_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/> = NULL;
    </xsl:when>
    <xsl:otherwise>
        for(int i=0; i &lt; <xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>s_COUNT; i++ )
        {
            if( o<xsl:value-of select="$OTHER_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>s[i] == o<xsl:value-of select="$THIS_CLASS_NAME"/>)
            {
                o<xsl:value-of select="$OTHER_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>s[i] = NULL;
                break;
            }
        }    
    </xsl:otherwise>
</xsl:choose>
    }
                                             </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// UNRELATE_ALL-->
                                             <xsl:template name="UNRELATE_ALL">                              
<!--/*!
** \addtogroup LEVEL3_LINK_UNLINK
** @{
*/-->
                                                     <xsl:for-each select="//Relationship">
                                                     <xsl:variable name="RELATIONSHIP_NAME" select="@name"/>
                                                     <xsl:variable name="OTHER_CLASS_NAME" select="ThatSide/@className"/>
                                                     <xsl:variable name="THIS_CLASS_NAME" select="ThisSide/@className"/>
/**/
/*!
** \brief An instance of '<xsl:value-of select="$THIS_CLASS_NAME"/>' disconnects from any/all instances of '<xsl:value-of select="$OTHER_CLASS_NAME"/>' related across relationship <xsl:value-of select="$RELATIONSHIP_NAME"/>
** \param o<xsl:value-of select="$THIS_CLASS_NAME"/> -- the instance of '<xsl:value-of select="$THIS_CLASS_NAME"/>' to be released from all <xsl:value-of select="$RELATIONSHIP_NAME"/> relationships  
** \return true if at least one instance of relationship '<xsl:value-of select="$RELATIONSHIP_NAME"/>' was found to break
*/
static void unrelateAll_<xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>(<xsl:value-of select="$THIS_CLASS_NAME"/> o<xsl:value-of select="$THIS_CLASS_NAME"/>)
    {
                                                    
                                                    <xsl:choose>
                                                        <xsl:when test="ThisSide/@isMany = 'true'">
        // the '<xsl:value-of select="$THIS_CLASS_NAME"/>' side of the '<xsl:value-of select="$RELATIONSHIP_NAME"/>' relationship is 'many' AND                                               
                                                           <xsl:choose>
                                                                <xsl:when test="ThatSide/@isMany = 'true'">
        // the '<xsl:value-of select="$OTHER_CLASS_NAME"/>' side of the '<xsl:value-of select="$RELATIONSHIP_NAME"/>' relationship is 'many'    [Case A]
        if( o<xsl:value-of select="$THIS_CLASS_NAME"/> != NULL )                                              // if the '<xsl:value-of select="$OTHER_CLASS_NAME"/>' side of the '<xsl:value-of select="$RELATIONSHIP_NAME"/>
        {
            FOREACH( <xsl:value-of select="$OTHER_CLASS_NAME"/>, o<xsl:value-of select="$OTHER_CLASS_NAME"/> )
            {
                for( int i = 0; i &lt; <xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>s_COUNT; i++)
                {
                    if( o<xsl:value-of select="$OTHER_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>s[i] == o<xsl:value-of select="$THIS_CLASS_NAME"/> )                           // if this pointer (one of many) points back to '<xsl:value-of select="$THIS_CLASS_NAME"/>'
                    {
                        unrelate_<xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>( o<xsl:value-of select="$OTHER_CLASS_NAME"/>, o<xsl:value-of select="$THIS_CLASS_NAME"/> );             // uncouple those two instances
                    }
                }
            }
        }
                                                                </xsl:when>

                                                                <xsl:otherwise>
        // the '<xsl:value-of select="$OTHER_CLASS_NAME"/>' side of the '<xsl:value-of select="$RELATIONSHIP_NAME"/>' relationship is 'one'     [Case B]
        if( o<xsl:value-of select="$THIS_CLASS_NAME"/> != NULL )                                              // if the '<xsl:value-of select="$THIS_CLASS_NAME"/>' side of the '<xsl:value-of select="$RELATIONSHIP_NAME"/>' relationship is non-NULL
        {
            unrelate_<xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>( o<xsl:value-of select="$THIS_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>, o<xsl:value-of select="$THIS_CLASS_NAME"/> );             // uncouple those two instances
        }   
                                                                </xsl:otherwise>
                                                           </xsl:choose>
                                                        </xsl:when>

                                                        <xsl:otherwise>
        // the '<xsl:value-of select="$THIS_CLASS_NAME"/>' side of the '<xsl:value-of select="$RELATIONSHIP_NAME"/>' relationship is 'one' AND
                                                           <xsl:choose>
                                                                <xsl:when test="ThatSide/@isMany = 'true'">
        // the '<xsl:value-of select="$OTHER_CLASS_NAME"/>' side of the '<xsl:value-of select="$RELATIONSHIP_NAME"/>' relationship is 'many'    [Case C]
        if( o<xsl:value-of select="$THIS_CLASS_NAME"/> != NULL )                                              // if the '<xsl:value-of select="$OTHER_CLASS_NAME"/>' side of the '<xsl:value-of select="$RELATIONSHIP_NAME"/>
        {
            FOREACH( <xsl:value-of select="$OTHER_CLASS_NAME"/>, o<xsl:value-of select="$OTHER_CLASS_NAME"/> )
            {
                if( o<xsl:value-of select="$OTHER_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/> == o<xsl:value-of select="$THIS_CLASS_NAME"/> )                           // if this pointer (one of many) points back to '<xsl:value-of select="$THIS_CLASS_NAME"/>'
                {
                    unrelate_<xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>( o<xsl:value-of select="$OTHER_CLASS_NAME"/>, o<xsl:value-of select="$THIS_CLASS_NAME"/> );             // uncouple those two instances
                }
            }
        }                                                        
                                                                  </xsl:when>
                                                              <xsl:otherwise>
        // the '<xsl:value-of select="$OTHER_CLASS_NAME"/>' side of the '<xsl:value-of select="$RELATIONSHIP_NAME"/>' relationship is 'one'   [Case D]
        if( o<xsl:value-of select="$THIS_CLASS_NAME"/> != NULL )            // if the '<xsl:value-of select="$THIS_CLASS_NAME"/>' side of the '<xsl:value-of select="$RELATIONSHIP_NAME"/>' relationship is non-NULL
        {
            unrelate_<xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/>( o<xsl:value-of select="$THIS_CLASS_NAME"/>, o<xsl:value-of select="$THIS_CLASS_NAME"/>-><xsl:value-of select="$RELATIONSHIP_NAME"/>_<xsl:value-of select="$OTHER_CLASS_NAME"/> );       // unlink both ends of the '<xsl:value-of select="$RELATIONSHIP_NAME"/>' relationship
        }
                                                             </xsl:otherwise>
                                                           </xsl:choose>
                                                        </xsl:otherwise>
                                                    </xsl:choose>
    }
                                                 </xsl:for-each>

<!--/*! @} end of LEVEL3_LINK_UNLINK */-->
                                             </xsl:template>                                                 

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// ASSERT( TEST_VALUE, ERROR_MESSAGE ) -->
                                             <xsl:template name="ASSERT">                                    
<xsl:param name="TEST_VALUE" />
<xsl:param name="ERROR_MESSAGE" />
<xsl:if test="$TEST_VALUE">
    <xsl:value-of select="$ERROR_MESSAGE"/>       <!-- put the error message into the output file -->
    <xsl:message terminate="yes">                 
        <xsl:value-of select="$ERROR_MESSAGE"/>   <!-- also emit the error message as an error and terminate processing -->
    </xsl:message>
</xsl:if>
                                             </xsl:template>                                                    

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// INITIAL_INSTANCE_POPULATION -->
                                             <xsl:template name="INITIAL_INSTANCE_POPULATION">      
/**/
    //_____________________________________________________________
    /*! /brief create the initial instance population for domain '<xsl:value-of select="@name"/>' */
    void <xsl:value-of select="@name"/>_CreateInstancePopulation()   
    {
        initializeMechanism();                              // initialize all the mechanism tables for classes in this domain
        initializeTLVs();                                   // initialize all the TLV-related mechanisms for attributes in this domain
/**/
                                                <xsl:for-each select="//Instances/Instance">        <!-- create new instances -->
        /**/<xsl:value-of  select="@className"/><xsl:text> </xsl:text><xsl:value-of  select="@id"/> = NEW(<xsl:value-of  select="@className"/>);
            /**/DEBUGTRACE(TRACE_VOLUME_MEDIUM, "<xsl:value-of  select="@className"/>", 0, <xsl:value-of  select="@id"/>, 0 );
                                                    <xsl:for-each select="AttributeValue">
            /**/<xsl:value-of  select="../@id"/>-><xsl:value-of  select="@name"/> = <xsl:value-of  select="@initialValue"/>;
                                                    </xsl:for-each>
                                                    <xsl:for-each select="InitialEvent">
                                                       <xsl:variable name="DELAY" select="@delay"/>
                                                       <xsl:choose>
                                                           <xsl:when test="string-length($DELAY) > 0">/**/
            /**/<xsl:value-of select="@name"/>_SendDelayed( <xsl:value-of select="../@id"/>, <xsl:value-of select="$DELAY"/> );
                                                           </xsl:when>
                                                           <xsl:otherwise>
            /**/<xsl:value-of select="@name"/>_Send( <xsl:value-of select="../@id"/> );
                                                           </xsl:otherwise>
                                                       </xsl:choose>
                                                   </xsl:for-each>/**/
                                                </xsl:for-each>
/**/
                                                <xsl:for-each select="//Relates/Relate">            <!-- go through the normal formalizing relates -->
                                                    <xsl:choose>
                                                        <xsl:when test="@name">
                                                          <xsl:variable name="RELATE_NAME" select="@name"/>
                                                          <xsl:variable name="RELATIONSHIP" select="//Relationship[@name = $RELATE_NAME]"/>
                                                          <xsl:variable name="THIS_CLASS_NAME" select="$RELATIONSHIP[1]/ThisSide/@className"/>        
                                                          <xsl:variable name="THAT_CLASS_NAME" select="$RELATIONSHIP[1]/ThatSide/@className"/>        
        relate_<xsl:value-of select="$RELATIONSHIP[1]/@name"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>_<xsl:value-of select="$THAT_CLASS_NAME"/>( <xsl:value-of select="@id2"/>, <xsl:value-of select="@id1"/> );     <xsl:call-template name="ASSERT"><xsl:with-param name="TEST_VALUE" select="string-length($THIS_CLASS_NAME) = 0"/> <xsl:with-param name="ERROR_MESSAGE" select="concat(concat(concat('PYCAA.XSL: Illegal relationship in Relate element named: ', $RELATE_NAME), '  position: '), position())"/>           </xsl:call-template>
                                                        </xsl:when>
                                                    </xsl:choose>      
                                                </xsl:for-each>
/**/
                                                <xsl:for-each select="//Relates/Relate">            <!-- go through the subclass/superclass relates -->
                                                    <xsl:choose>
                                                        <xsl:when test="@subtype">
                                                          <xsl:variable name="SUBTYPE_CLASS_NAME" select="@subtype"/>
                                                          <xsl:variable name="CLASS_ELEMENT" select="//Class[@name = $SUBTYPE_CLASS_NAME]"/>
                                                          <xsl:variable name="CLASS_NAME" select="$CLASS_ELEMENT/@name"/>
                                                          <xsl:variable name="SUPERCLASS_NAME" select="$CLASS_ELEMENT/Supertype/@name"/>          
        relate_SUBTYPE_<xsl:value-of select="$SUBTYPE_CLASS_NAME"/>( <xsl:value-of select="@id1"/>, <xsl:value-of select="@id2"/> );     <xsl:call-template name="ASSERT"><xsl:with-param name="TEST_VALUE" select="string-length($CLASS_NAME) = 0"/> <xsl:with-param name="ERROR_MESSAGE" select="concat('Illegal Relate element (unknown subtype name) at position ', position())"/>           </xsl:call-template>
                                                        </xsl:when>
                                                    </xsl:choose>      
                                                </xsl:for-each>
    }
                                             </xsl:template>      

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// EVENT_PARAMETERS (context: State)-->
                                            <xsl:template name="EVENT_PARAMETERS">          
<xsl:variable name="STATE_NAME" select="@name"/>
<xsl:variable name="CLASS_NAME" select="../../@name"/>
<xsl:for-each select="//TargetSideState[@name = $STATE_NAME]">
    <xsl:if test="position() = 1">  <!-- in case there are many hits, just use the first one -->
        <xsl:variable name="EVENT_NAME" select="//TargetSideState[@name = $STATE_NAME]/../@eventElementID"/>
        <xsl:variable name="EVENT" select="//Event[@eventElementID = $EVENT_NAME]"/>
        <xsl:if test="count($EVENT/Parameter) > 0">
            <xsl:value-of select="$CLASS_NAME"/>_<xsl:value-of select="$EVENT_NAME"/>_eventData* eventData = (<xsl:value-of select="$CLASS_NAME"/>_<xsl:value-of select="$EVENT_NAME"/>_eventData*)p_;
        </xsl:if>
    </xsl:if>
</xsl:for-each>
                                             </xsl:template>       
    
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// ACCEPTED_MESSAGES (context: Domain)-->
                                             <xsl:template name="ACCEPTED_MESSAGES">                                                                                           
/*!
** \defgroup LEVEL3_<xsl:value-of select="@name"/>_BRIDGING Bridge Functions
** @{
*/     
                                                <xsl:for-each select="Bridging/AcceptedMessages">
/**/ 
/**/ 
    // Domain '<xsl:value-of  select="@name"/>' Accepted Bridging Messages
                                                    <xsl:for-each select="Message">
                                                        <xsl:variable name="RETURN_TYPE" select="ReturnValue/@dataType"/>
 /**/
    //_____________________________________________________________
    /*! <xsl:if test="string-length(@meaning) > 0"> /brief <xsl:value-of select="@meaning"/> </xsl:if> */
    <xsl:value-of select="$RETURN_TYPE"/><xsl:text> </xsl:text> <xsl:value-of select="../../../@name"/>_<xsl:value-of select="@name"/>(<xsl:call-template name="DATATYPE_PARAMETERS"/>)   
    {<xsl:value-of select="ImplementationCode/text()"/>
    }
      /**/
                                                    </xsl:for-each>
    /**/
                                                </xsl:for-each>
/*! @} end of group: LEVEL3_<xsl:value-of select="@name"/>_BRIDGING  */
                                             </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// FUNCTION_DESCRIPTION-->
                                             <xsl:template name="FUNCTION_DESCRIPTION">                      
        <xsl:if test="string-length(@description) > 0">     // <xsl:value-of select="@description"/></xsl:if>
                                             </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// FUNCTION_DESCRIPTION (context: ModeledDomain )-->
                                             <xsl:template name="TAG_LINKED_VALUES">         
/**/
                                                <xsl:variable name="DOMAIN_NAME" select="@name"/>
                                                <xsl:variable name="TLV_COUNT" select="count(//Classes/Class/Attributes/Attribute)"/>
/**/
static TLV_TUPLE TLV_TABLE[ <xsl:value-of select="$TLV_COUNT"/> + 1 ];
static void initializeTLVs()            // here we are jumping through hoops to make the code relocatable -- all the pointers need to be resolved at runtime
{
    int i = 0;   // incrementing array index
                                                <xsl:for-each select="//Class">
                                                    <xsl:variable name="CLASS_NAME" select="@name"/>
                                                   <!-- <xsl:variable name="CLASS_POSITION" select="position()"/>-->
                                                        <xsl:if test="@minimumAllocation='1'">                                                   
                                                            <xsl:for-each select="Attributes/Attribute  [@TLVhashcode != '0']">
                                                                <xsl:variable name="PERSISTENT" select="@persistent"/>
                                                                <xsl:variable name="HAS_UPDATE_OPERATION" select="@hasUpdateOperation"/>
                                                           <!--     <xsl:variable name="ATTRIBUTE_POSITION" select="position()"/>-->
                                                                <xsl:variable name="TEMPORARY_NAME" select="generate-id()"/>
                                                                <xsl:choose>
                                                                    <xsl:when test="$HAS_UPDATE_OPERATION">
    SetTLVtuple( &amp;TLV_TABLE[i++], TLV_<xsl:value-of select="$DOMAIN_NAME"/>_<xsl:value-of select="$CLASS_NAME"/>_<xsl:value-of select="@name"/>, tlvType_<xsl:value-of select="@dataType"/>, &amp;(<xsl:value-of select="$CLASS_NAME"/>_SINGLETON-><xsl:value-of select="@name"/>), <xsl:value-of select="$PERSISTENT"/>, (MechInstance)<xsl:value-of select="$CLASS_NAME"/>_SINGLETON, &amp;<xsl:value-of select="@name"/>); 
                                                                    </xsl:when>
                                                                    <xsl:otherwise>
    SetTLVtuple( &amp;TLV_TABLE[i++], TLV_<xsl:value-of select="$DOMAIN_NAME"/>_<xsl:value-of select="$CLASS_NAME"/>_<xsl:value-of select="@name"/>, tlvType_<xsl:value-of select="@dataType"/>, &amp;(<xsl:value-of select="$CLASS_NAME"/>_SINGLETON-><xsl:value-of select="@name"/>), <xsl:value-of select="$PERSISTENT"/>,  (MechInstance)<xsl:value-of select="$CLASS_NAME"/>_SINGLETON, null);                                                             
                                                                    </xsl:otherwise>
                                                                </xsl:choose>
                                                            </xsl:for-each>
                                                        </xsl:if>
                                                </xsl:for-each>
    TLV_TUPLE _terminator = { 0, tlvType_terminator_zero, 0 };  TLV_TABLE[i++] = _terminator;   // last entry of zeros terminates the table
/**/
    RegisterDomainTLVlist( TLV_TABLE );                         // add this TLV table to the list of TLV tables
};
                                            </xsl:template>
   
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// CLASS_OPERATIONS (context: Domain)-->
                                             <xsl:template name="CLASS_OPERATIONS">          
/*!
** \defgroup LEVEL3_HELPER_FUNCTIONS Helper Functions
** @{
*/
/**/ 
/**/ 
                                                <xsl:for-each select="//Operation">
/**/                                                
    //____________________________________________________________
                                                    <xsl:variable name="CLASS_NAME" select="../../@name"/>
                                                    <xsl:choose>
                                                        <xsl:when test="@updatesAttributeName">
    // this operation is automatically executed whenever class '<xsl:value-of select="$CLASS_NAME"/>' has attribute '<xsl:value-of select="@updatesAttributeName"/>' updated via the TLV mechanism
    static <xsl:value-of select="@returnType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>( <xsl:value-of select="$CLASS_NAME"/> self )   
                                                        </xsl:when>
                                                        <xsl:otherwise>
    /*! /brief Domain '<xsl:value-of  select="../../../../@name"/>' static helper function (found in class '<xsl:value-of select="$CLASS_NAME"/>') */   
    static <xsl:value-of select="@returnType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>(<xsl:call-template name="DATATYPE_PARAMETERS"/>)   
                                                        </xsl:otherwise>
                                                    </xsl:choose>
    {
    <xsl:value-of select="text()"/>  
    }
                                                </xsl:for-each>
/*! @} end of group: LEVEL3_HELPER_FUNCTIONS */
                                            </xsl:template>   
                 
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// INIT_MECHANISMS-->
                                             <xsl:template name="INIT_MECHANISMS">                         
                                                 <xsl:variable name="ACTION_FUNCTION_COUNT" select="count(States/State[(@isIgnoreState = 'false') and (@isPigtailState = 'false') and (@isMeatballState = 'false') ])"/> 

/**/
    /*! /brief initialize all the mechanism data structures for class '<xsl:value-of select="@name"/>' */
    static void initializeMechanisms_<xsl:value-of select="@name"/>()
    {
        <xsl:value-of select="@name"/>_iab.availableInPool = <xsl:value-of select="@minimumAllocation"/>;
        <xsl:value-of select="@name"/>_iab.storageStart    = <xsl:value-of select="@name"/>_storage;
        <xsl:value-of select="@name"/>_iab.storageFinish   = <xsl:value-of select="@name"/>_storage + <xsl:value-of select="@name"/>_iab.availableInPool;
        <xsl:value-of select="@name"/>_iab.storageLast     = <xsl:value-of select="@name"/>_storage + 0;
        <xsl:value-of select="@name"/>_iab.allocCounter    = 0;
        <xsl:value-of select="@name"/>_iab.instanceSize    = sizeof(struct _<xsl:value-of select="@name"/>);
/**/
                                                <xsl:if test="$ACTION_FUNCTION_COUNT > 0">
        // state action table
        PtrActionFunction _<xsl:value-of select="@name"/>_acttbl[ <xsl:value-of select="$ACTION_FUNCTION_COUNT"/> ] = 
        {
            <xsl:for-each select="States/State[(@isIgnoreState = 'false') and (@isPigtailState = 'false') and (@isMeatballState = 'false') ]"><xsl:value-of  select="../../@name"/>_<xsl:value-of  select="@name"/>,
            </xsl:for-each>
        };
        memcpy( &amp;<xsl:value-of select="@name"/>_acttbl, &amp;_<xsl:value-of select="@name"/>_acttbl, sizeof(&amp;<xsl:value-of select="@name"/>_acttbl) * <xsl:value-of select="$ACTION_FUNCTION_COUNT"/> ); 
/**/
        // struct objectdispatchblock
        <xsl:value-of select="@name"/>_odb.stateCount = <xsl:value-of select="@name"/>_STATE_Count;
        <xsl:value-of select="@name"/>_odb.eventCount = <xsl:value-of select="@name"/>_EVENT_Count;
        <xsl:value-of select="@name"/>_odb.transitionTable = <xsl:value-of select="@name"/>_TransitionTable;
        <xsl:value-of select="@name"/>_odb.actionTable = <xsl:value-of select="@name"/>_acttbl;
        //<xsl:value-of select="@name"/>_odb.finalStates = NULL;
/**/
                                                </xsl:if>
        // struct mechclass 
        <xsl:value-of select="@name"/>_class.iab = &amp;<xsl:value-of select="@name"/>_iab;
    <xsl:choose >     <!-- if this class has a state model -->
    <xsl:when test="count(Events/Event) > 0"><xsl:text>    </xsl:text><xsl:value-of select="@name"/>_class.odb = &amp;<xsl:value-of select="@name"/>_odb;
    </xsl:when>
    <xsl:otherwise><xsl:text>    </xsl:text><xsl:value-of select="@name"/>_class.odb = NULL;
    </xsl:otherwise>
    </xsl:choose>    //<xsl:value-of select="@name"/>_class.pdb = NULL;
    };
/**/
                                                            </xsl:template>
                 
   
</xsl:stylesheet>

