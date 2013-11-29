<?xml version="1.0" encoding="UTF-8"?>

<!--
   Note: the string /**/ appearing below is a special markers to indicate blank lines
   which should not be suppressed at the end of the transformation. These are removed
   from the final output file at the end of the transformation processing.
-->


<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
    xmlns:xs="http://www.w3.org/2001/XMLSchema"
    exclude-result-prefixes="xs"
    version="2.1">

    <xsl:output method="text" omit-xml-declaration="yes" indent="yes" xml:space="preserve"/>
    <xsl:variable name="XSL_VERSION" select="'33.0.0.00'"/>
    <xsl:variable name="CR_INDENT"><xsl:text>&#10;&#13;        </xsl:text></xsl:variable>


<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// Model -->
                                                <xsl:template match="/Model">                     <!-- start matching at the root  -->
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
**      XSL Transform: CSharp.xsl (v<xsl:value-of select="$XSL_VERSION"/>)
**
**          Generated: <xsl:value-of select="@generated"/>**  
**
** \copyright © 2013, The Random Organization  All Rights Reserved.
*/
/**/
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Diagnostics;
using Mechanisms;
                                                    <xsl:for-each select="//Namespace">
using <xsl:value-of select="@name"/>;    <xsl:if test="@description">            // <xsl:value-of select="@description"/></xsl:if>
                                                    </xsl:for-each>
/**/


                                                    <xsl:apply-templates select="ModeledDomains/ModeledDomain"/>          
                                                </xsl:template>                                
                               
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// ModeledDomain -->
                                                <xsl:template  match="ModeledDomain">
namespace <xsl:value-of select="@name"/>
{
                                                    <xsl:call-template name="INITIAL_INSTANCE_POPULATION"/>
                                                    <xsl:call-template name="ENUMERATIONS"/>
                                                    <xsl:call-template name="INTERFACES"/>           
                                                    <xsl:call-template name="CLASSES"/>
                                                    <xsl:call-template name="EVENT_GENERATION"/>   
} // end of namespace <xsl:value-of select="@name"/>
/**/
/**/
                                                </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// STATE_TRANSITION_TABLES (context: Class) -->
                                                <xsl:template name="STATE_TRANSITION_TABLES">
                                                    <xsl:for-each select="States/State  [@isMeatballState='false' and @isIgnoreState='false' and @isPigtailState='false']">
                                                        <xsl:if test="position() = 1">
        /**/
        // State Transition Tables 
                                                        </xsl:if>
        static private Dictionary&lt;Guid, ModeledState&gt; o<xsl:value-of select="@name"/>_TransitionTable;    //    <xsl:value-of select="@isMeatballState"/>, <xsl:value-of select="@isIgnoreState"/>, <xsl:value-of select="@isPigtailState"/>
                                                    </xsl:for-each>
                                                </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// CONSTRUCTOR (context: Class) -->
                                               <xsl:template name="CONSTRUCTOR">       
                                                   <xsl:variable name="CLASS_NAME" select="@name"/>
        /**/                                         
       public <xsl:value-of select="@name"/>()  // constructor
       {
            sjmErrorHandler oErrorHandler = new sjmErrorHandler();
                                                   
            EnableDebugMessages = true;
            AllInstances.Add(this);              // add this newly minted instance to the list of all instances of this type       
            //           
                                                   <xsl:for-each select="Attributes/Attribute">
                                                       <xsl:if test="@initialValue">
            /**/<xsl:value-of select="@name"/> = <xsl:value-of select="@initialValue"/>;     // initial value
                                                       </xsl:if>
                                                   </xsl:for-each> 
            //
                                                   <xsl:if test="States/State  [@isMeatballState='false' and @isIgnoreState='false' and @isPigtailState='false']"> 
            if( o<xsl:value-of select="States/State/@name"/>_TransitionTable == null )    // create state transition tables, once only
            { 
                try
                {
                                                       <xsl:for-each select="States/State  [@isMeatballState='false' and @isIgnoreState='false' and @isPigtailState='false']">
                    o<xsl:value-of select="@name"/>_TransitionTable = new Dictionary&lt;Guid, ModeledState&gt;();                                         
                                                        </xsl:for-each>
                    /**/
                    // normal transitions
                                                        <xsl:for-each select="Transitions/Transition  [@isPigtailTransition='false' and @eventElementID and TargetSideState/@isIgnoreState='false']">
                                                         <!--   <xsl:if test="@isInitialStateIndicatorTransition != 'false'">-->
                    /**/
                    oErrorHandler.SupplementalInformation = "Illegal use of '<xsl:value-of select="@eventElementID"/>'\r\n\r\nState machine '<xsl:value-of select='$CLASS_NAME'/>'\r\n\r\n(you probably have an illegal pigtail or ignore structure somewhere)";
                    o<xsl:value-of select="SourceSideState/@name"/>_TransitionTable.Add(Events.EventID_<xsl:value-of select="@eventElementID"/>, new <xsl:value-of select="TargetSideState/@name"/>());
                                                         <!--   </xsl:if>-->
                                                        </xsl:for-each>
                /**/
                    // pigtail transitions (a single target state, irrespective of the current state)
                                                         <xsl:for-each select="Transitions/Transition  [@isPigtailTransition='true' and @eventElementID and TargetSideState/@isIgnoreState='false']">
                                                             <xsl:variable name="PIGTAIL_TRANSITION" select="@eventElementID"/>
                                                             <xsl:variable name="PIGTAIL_TARGET_STATE" select="TargetSideState/@name"/>
                                                             <xsl:for-each select="../../States/State [@isMeatballState='false' and @isIgnoreState='false' and @isPigtailState='false']">         
                                                                 <xsl:variable name="STATE_NAME" select="@name"/>
                    /**/
                    oErrorHandler.SupplementalInformation = "Illegal (pigtail) use of '<xsl:value-of select="@eventElementID"/>'\r\n\r\nState machine '<xsl:value-of select='$CLASS_NAME'/>'\r\n\r\n(you probably have an illegal pigtail or ignore structure somewhere)";
                    o<xsl:value-of select="$STATE_NAME"/>_TransitionTable.Add(Events.EventID_<xsl:value-of select="$PIGTAIL_TRANSITION"/>, new <xsl:value-of select="$PIGTAIL_TARGET_STATE"/>());
                                                            </xsl:for-each>
                /**/
                                                        </xsl:for-each>
                    // ignored transitions
                                                        <xsl:for-each select="Transitions/Transition  [@isPigtailTransition='false' and @eventElementID and TargetSideState/@isIgnoreState='true']">
                    /**/
                    oErrorHandler.SupplementalInformation = "Illegal (ignored) use of '<xsl:value-of select="@eventElementID"/>'\r\n\r\nState machine '<xsl:value-of select='$CLASS_NAME'/>'\r\n\r\n(you probably have an illegal pigtail or ignore structure somewhere)";
                    o<xsl:value-of select="SourceSideState/@name"/>_TransitionTable.Add(Events.EventID_<xsl:value-of select="@eventElementID"/>, null);
                                                        </xsl:for-each>
               }    
               catch (Exception ex)
               {
                   oErrorHandler.Announce(ex);
               }                                                       
           }
                                                        <xsl:if test="States/State [@isInitialState='true']">
           this.oCurrentState = new <xsl:value-of select="States/State [@isInitialState='true']/@name"/>();  // initial state (as indicated by the black meatball)
                                                        </xsl:if>                                       
                                                    </xsl:if>
       }  // end of <xsl:value-of select="@name"/> constructor
       /**/
                                               </xsl:template> 

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// TRANSITIONS (context: ModeledDomain) -->
                                                <xsl:template  name="TRANSITIONS">
                                                    <xsl:for-each select="//Event">
                                                        <xsl:if test="position() = 1">
        /**/
        // Event Identifiers 
                                                        </xsl:if>
        public static Guid EventID_<xsl:value-of select="@name"/> = Guid.NewGuid();
                                                    </xsl:for-each>
                                                </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// STATE_IDS (context: Class) -->
                                                <xsl:template  name="STATE_IDS">
                                                    <xsl:for-each select="States/State [@isMeatballState='false' and @isIgnoreState='false' and @isPigtailState='false']">
                                                        <xsl:if test="position() = 1">
        /**/
        // State Identifiers 
                                                        </xsl:if>
        public static Guid <xsl:value-of select="../../@name"/>_StateID_<xsl:value-of select="@name"/> = Guid.NewGuid();   
                                                    </xsl:for-each>
                                                </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// EVENT_GENERATION (context: ModeledDomain) -->
                                                <xsl:template  name="EVENT_GENERATION">
    /**/
    public static class Events
    {                                                <xsl:for-each select="//Event">                                                                                                    
        public static void <xsl:value-of select="@name"/>(ModeledClass TargetInstance<xsl:for-each select="Parameter">, <xsl:value-of select="@dataType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/> </xsl:for-each>, long lMillisecondsDelay = 0)
        {
            EventPump.EnqueueEvent(new Event_<xsl:value-of select="@name"/>(TargetInstance<xsl:for-each select="Parameter">, <xsl:value-of select="@name"/> </xsl:for-each>, lMillisecondsDelay));
        }
        /**/
                                                    </xsl:for-each>
                                                    <xsl:call-template name="EVENTS"/> 
                                                    <xsl:call-template name="TRANSITIONS"  /> 
    } // end of Events 
                                                </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// EVENTS (context: ModeledDomain) -->
                                                <xsl:template  name="EVENTS">
                                                    <xsl:for-each select="//Event">
                                                        <xsl:if test="position() = 1">
        /**/
        // Event Objects
                                                        </xsl:if>
        /**/
                                                        <xsl:variable name="EVENT_NAME" select="@name"/>
        public class Event_<xsl:value-of select="@eventElementID"/> : ModeledEvent
        {
                                                            <xsl:for-each select="Parameter">
            public <xsl:value-of select="@dataType"/>/**/ <xsl:value-of select="@name"/>; 
                                                            </xsl:for-each>
            /**/
            public Event_<xsl:value-of select="@name"/>(ModeledClass TargetInstance<xsl:for-each select="Parameter">, <xsl:value-of select="@dataType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/> </xsl:for-each>, long lMillisecondsDelay)
                : base(TargetInstance, Events.EventID_<xsl:value-of select="@name"/>, lMillisecondsDelay)
            {
                this.EventName = "<xsl:value-of select="@name"/>";
                                                            <xsl:for-each select="Parameter">
                this.<xsl:value-of select="@name"/> = <xsl:value-of select="@name"/>;
                                                            </xsl:for-each>
            }
        } // end of event class: Event_<xsl:value-of select="@name"/>
                                                    </xsl:for-each>
                                                </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// STATES (context: Class) -->
                                                <xsl:template  name="STATES">
                                                    <xsl:for-each select="States/State [@isPigtailState='false' and @isIgnoreState='false' and @isMeatballState='false']">
                                                        <xsl:variable name="STATE_ELEMENT_ID" select="@elementID"></xsl:variable>
                                                        <xsl:if test="position() = 1">
        /**/
        // State Objects
                                                        </xsl:if>
        /**/
                                                        <xsl:variable name="EVENT_NAME" select="//Transition [TargetSideState/@elementID = $STATE_ELEMENT_ID]/@eventElementID"/>
        public class <xsl:value-of select="@name"/> : ModeledState   
        {
            public <xsl:value-of select="@name"/>()
                : base(<xsl:value-of select="../../@name"/>_StateID_<xsl:value-of select="@name"/>, o<xsl:value-of select="@name"/>_TransitionTable, "<xsl:value-of select="@name"/>")
            {
            }
            /**/ 
            /**/
            /**/
            //________________________________________________________________________________________________________________________________________
            public override void ExecuteAction(ModeledEvent _IncomingEvent)     // <xsl:value-of select="../../@name"/>:<xsl:value-of select="@name"/>
            {
                <xsl:value-of select="../../@name"/> self = (<xsl:value-of select="../../@name"/>)_IncomingEvent.TargetInstance;
/**/
                <xsl:choose>
                    <xsl:when test="string-length(text()) > 0">
/**/<xsl:value-of select="text()"/>
                    </xsl:when>
                    <xsl:otherwise>
                throw new Exception("Unimplemented state: <xsl:value-of select="../../@name"/>:<xsl:value-of select="@name"/>");
                    </xsl:otherwise>
                </xsl:choose>                                         
            }
        } <!--// end of state class: <xsl:value-of select="@name"/>-->
        /**/
        /**/
                                                    </xsl:for-each>
                                                </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// RELATIONSHIPS (context: Class) -->
                                                <xsl:template  name="RELATIONSHIPS">
                                                    <xsl:variable name="CLASS_NAME" select="@name"/>
                                                    <xsl:for-each select="Relationships/Relationship/ThatSide">
                                                         <xsl:if test="position() = 1">
        /**/
        // Relationships
                                                         </xsl:if>
                                                         <xsl:choose>
                                                            <xsl:when test='@isMany="true"'>
        public  List&lt;<xsl:value-of select="../ThatSide/@className"/><xsl:text>&gt; </xsl:text><xsl:value-of select="../@name"/>_<xsl:value-of select="../ThatSide/@className"/>s = new List&lt;<xsl:value-of select="../ThatSide/@className"/>&gt;();               // '<xsl:value-of select="../ThatSide/@className"/>' <xsl:value-of select="@role"/> '<xsl:value-of select="../ThisSide/@className"/>'
                                                            </xsl:when>
                                                            <xsl:otherwise>
        public <xsl:value-of select="../ThatSide/@className"/><xsl:text> </xsl:text><xsl:value-of select="../@name"/>_<xsl:value-of select="../ThatSide/@className"/> { get; set; }                          // '<xsl:value-of select="../ThatSide/@className"/>' <xsl:value-of select="@role"/> '<xsl:value-of select="../ThisSide/@className"/>'                                                       
                                                            </xsl:otherwise>
                                                        </xsl:choose>
                                                    </xsl:for-each>
                                                </xsl:template>
           
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// ATTRIBUTES (context: Class) -->
                                                <xsl:template  name="ATTRIBUTES">
                                                    <xsl:variable name="CLASS_NAME" select="@name"/>
                                                    <xsl:for-each select="Attributes/Attribute">
                                                         <xsl:if test="position() = 1">
        /**/
        // Attributes
                                                         </xsl:if>
        public <xsl:value-of select="@dataType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/> { get; set; }   <xsl:if test="string-length(@description) > 0">// <xsl:value-of select="@description"/></xsl:if>
                                                    </xsl:for-each>
        /**/
                                                </xsl:template>
                                                                                        
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// CLASSES (context: ModeledDomain) -->
                                                <xsl:template  name="CLASSES">
                                                    <xsl:for-each select="Classes/Class">   
                                                        <xsl:if  test="not(Stereotypes/Stereotype/@name = 'OMIT')"> 
    /**/
    /**/

    //___________________________________________________________________________________________________ Class: <xsl:value-of select="@name"/>
    //
                                                           <xsl:choose>
                                                               <xsl:when test="Supertype">
    public class <xsl:value-of select="@name"/> : <xsl:value-of select="Supertype/@name"/>
    {                                                                                                              
        public static new List&lt;<xsl:value-of select="@name"/>&gt; AllInstances = new List&lt;<xsl:value-of select="@name"/>&gt;();                                                            
                                                                </xsl:when>
                                                                <xsl:otherwise>
    public class <xsl:value-of select="@name"/> : ModeledClass    
    {                                                                                                              
        public static List&lt;<xsl:value-of select="@name"/>&gt; AllInstances = new List&lt;<xsl:value-of select="@name"/>&gt;();                                                            
                                                               </xsl:otherwise>
                                                          </xsl:choose>
                                                          <xsl:call-template name="STATE_IDS"  />
                                                          <xsl:call-template name="STATE_TRANSITION_TABLES"/>
                                                          <xsl:call-template name="CONSTRUCTOR"/>
                                                          <xsl:call-template name="STATES"/>
                                                          <xsl:call-template name="RELATIONSHIPS"/>
                                                          <xsl:call-template name="ATTRIBUTES"/>
                                                          <xsl:call-template name="METHODS"/>
    } // end of <xsl:value-of select="@name"/>
                                                        </xsl:if>  
                                                    </xsl:for-each>
                                                </xsl:template>
                               
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// INITIAL_INSTANCE_POPULATION -->
                                                <xsl:template name="INITIAL_INSTANCE_POPULATION">
    /**/
    public class ModelPopulation
    {  
        public static void Initialize() //
        {
                                                    <xsl:call-template name="SCRIPTED_RESPONSES"/>   
            // Instances
                                                    <xsl:for-each select="//Instances/Instance">
/**/
            /**/<xsl:value-of select="@className" /><xsl:text> </xsl:text><xsl:value-of select="@id" /> = new <xsl:value-of select="@className" />();
                                                        <xsl:for-each select="AttributeValue"> 
                                                            <xsl:variable name="ATTRIBUTE_NAME" select="@name"/>
                                                            <xsl:variable  name="ATTRIBUTE_TYPE" select="//Classes/Class/Attributes/Attribute [@name = $ATTRIBUTE_NAME]/@dataType"/>
                                    
                                                            <xsl:choose>
                                                                <xsl:when test="$ATTRIBUTE_TYPE='string'">
                /**/<xsl:value-of select="../@id" />.<xsl:value-of select="@name"/> = "<xsl:value-of select="@initialValue"/>";  
                                                                </xsl:when>
                                                                <xsl:otherwise>
                /**/<xsl:value-of select="../@id" />.<xsl:value-of select="@name"/> = <xsl:value-of select="@initialValue"/>;  
                                                                </xsl:otherwise>
                                                            </xsl:choose>
                                                        </xsl:for-each>
                                                        <xsl:if test="InitialEvent">
                /**/Events.<xsl:value-of select="InitialEvent/@name"/>(<xsl:value-of select="@id" /><xsl:if test="InitialEvent/@delay">, <xsl:value-of select="InitialEvent/@delay"/></xsl:if>);   
                                                        </xsl:if>     
                                                    </xsl:for-each>
            /**/
            // Relates
                                                    <xsl:for-each select="//Relates/Relate">            <!-- go through the normal formalizing relates -->
    /**/
                                                        <xsl:choose>
                                                            <xsl:when test="@name">
                                                                <xsl:variable name="RELATE_NAME" select="@name"/>
                                                                <xsl:variable name="RELATIONSHIP" select="//Relationship[@name = $RELATE_NAME]"/>
                                                                <xsl:variable name="THIS_CLASS_NAME" select="$RELATIONSHIP[1]/ThisSide/@className"/>        
                                                                <xsl:variable name="THAT_CLASS_NAME" select="$RELATIONSHIP[1]/ThatSide/@className"/>
                                                                <xsl:variable name="THAT_SIDE" select="$RELATIONSHIP[1]/ThatSide"/>
                                                                <xsl:variable name="THIS_SIDE" select="$RELATIONSHIP[1]/ThisSide"/>
                                                                <xsl:choose >
                                                                    <xsl:when test='$THAT_SIDE/@isMany="true"'>
            /**/<xsl:value-of select="@id1"/>.<xsl:value-of select="$RELATIONSHIP[1]/@name"/>_<xsl:value-of select="$THAT_CLASS_NAME"/>s.Add( <xsl:value-of select="@id2"/> );
                                                                    </xsl:when>
                                                                    <xsl:otherwise>
            /**/<xsl:value-of select="@id1"/>.<xsl:value-of select="$RELATIONSHIP[1]/@name"/>_<xsl:value-of select="$THAT_CLASS_NAME"/> = <xsl:value-of select="@id2"/>;
                                                                    </xsl:otherwise>
                                                                </xsl:choose>


                                                                <xsl:choose >
                                                                    <xsl:when test='$THIS_SIDE/@isMany="true"'>
            /**/<xsl:value-of select="@id2"/>.<xsl:value-of select="$RELATIONSHIP[1]/@name"/>_<xsl:value-of select="$THIS_CLASS_NAME"/>s.Add( <xsl:value-of select="@id1"/> );
                                                                    </xsl:when>
                                                                    <xsl:otherwise>
            /**/<xsl:value-of select="@id2"/>.<xsl:value-of select="$RELATIONSHIP[1]/@name"/>_<xsl:value-of select="$THIS_CLASS_NAME"/> = <xsl:value-of select="@id1"/>;
                                                                    </xsl:otherwise>
                                                                </xsl:choose>
                                                            </xsl:when>
                                                        </xsl:choose>    
                                                    </xsl:for-each>
        }  // end of Initialize() 
    }
                                                </xsl:template>
 
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// SCRIPTED_RESPONSES (context: Population) -->
                                                <xsl:template name="SCRIPTED_RESPONSES">                
                                                    <xsl:for-each select="//ScriptedResponses/Device">
            // Device '<xsl:value-of select="@id"/>'
            List&lt;string&gt; oDevice_<xsl:value-of select="@id"/> = new List&lt;string&gt;();
            CommunicatingDevice.ScriptedResponses.Add( "<xsl:value-of select="@id"/>", oDevice_<xsl:value-of select="@id"/> );
                                                        <xsl:for-each select="Response">
               oDevice_<xsl:value-of select="../@id"/>.Add("<xsl:value-of select="@string"/>");
                                                        </xsl:for-each>
                                                    </xsl:for-each>
                                                    /**/
                                                </xsl:template>  
    
<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// ASSERT -->
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

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// ENUMERATIONS -->
                                                <xsl:template name="ENUMERATIONS">
                                                    <xsl:for-each select="//Enumerations/Enumeration">
    /**/
    public enum <xsl:value-of select="@name"/>
    {
                                                        <xsl:for-each select="Enumerator">
        /**/<xsl:value-of select="@name"/>,               // <xsl:value-of select="@description"/>
                                                        </xsl:for-each>
    }
                                                    </xsl:for-each>
                                                    /**/
                                                    <xsl:for-each select="//Enumerations/Enumeration">
    public static class <xsl:value-of select="@name"/>_STRINGS
    {
    private static Dictionary&lt;<xsl:value-of select="@name"/>, string&gt; descriptions;
        
        public static string GetDescription(<xsl:value-of select="@name"/> enumerator)
        {
            if (descriptions == null)
            {
                descriptions = new Dictionary&lt;<xsl:value-of select="@name"/>, string&gt;();
                                                        <xsl:for-each select="Enumerator">
                descriptions.Add(<xsl:value-of select="../@name"/>.<xsl:value-of select="@name"/>, "<xsl:value-of select="@description"/>"); 
                                                        </xsl:for-each>
            }
            
            return descriptions[enumerator];
        }                                                  
    }
                                                    </xsl:for-each>
                                                </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// INTERFACES -->
                                                <xsl:template name="INTERFACES">
                                                    <xsl:for-each select="//Interfaces/Interface">
    /**/
    public static class <xsl:value-of select="@name"/>_Interface
    {
                                                        <xsl:for-each select="Methods/Method">
                                                            <xsl:choose>
                                                                <xsl:when test="@stereotypes='event'">
                                                                    <xsl:call-template name="INTERFACE_EVENT"/>
                                                                </xsl:when>
                                                                <xsl:otherwise>
                                                                    <xsl:call-template name="INTERFACE_METHOD"/>
                                                                </xsl:otherwise>
                                                            </xsl:choose>
                                                        </xsl:for-each>                                                          
    } // end of class: <xsl:value-of select="@name"/>_Interface                                                         
                                                    </xsl:for-each> 
                                                </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// INTERFACE_EVENT (context: Method) -->
                                                <xsl:template name="INTERFACE_EVENT">
        /**/
        // Interface Event: <xsl:value-of select="@name"/>
        public delegate <xsl:value-of select="@returnType"/> /**/<xsl:value-of select="@name"/>_Delegate(<xsl:for-each select="Parameter"><xsl:if test="position() > 1">, </xsl:if>  <xsl:value-of select="@dataType"/><xsl:text> </xsl:text> <xsl:value-of select="@name"/>  </xsl:for-each>);
        public static event <xsl:value-of select="@name"/>_Delegate <xsl:value-of select="@name"/>_Event;
        public static void Raise_<xsl:value-of select="@name"/>(<xsl:for-each select="Parameter"><xsl:if test="position() > 1">, </xsl:if>  <xsl:value-of select="@dataType"/><xsl:text> </xsl:text> <xsl:value-of select="@name"/> </xsl:for-each>)
        {
           <!-- /**/<xsl:if test="@returnType != 'void'"><xsl:value-of select="@returnType"/> returnResult = (<xsl:value-of select="@returnType"/>)0;</xsl:if>-->
            if (null != <xsl:value-of select="@name"/>_Event)
            {
                try
                {
                    
                    /**/<!--<xsl:if test="@returnType != 'void'">returnResult = </xsl:if>--><xsl:value-of select="@name"/>_Event.Invoke(<xsl:for-each select="Parameter"><xsl:if test="position() > 1">, </xsl:if> <xsl:value-of select="@name"/>  </xsl:for-each>);
                }
                catch {   /* do nothing, protecting ourselves from exceptions occurring in the callback(s) */ }
                finally { /* just continue even if there is an exception */ }
            }
           <!-- /**/<xsl:if test="@returnType != 'void'">return returnResult;</xsl:if> -->
        } // end of event invocation: Raise_<xsl:value-of select="@name"/>
                                                </xsl:template>

<!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////////// INTERFACE_METHOD (context: Method) -->
                                                <xsl:template name="INTERFACE_METHOD">
        /**/
        // Interface Method: <xsl:value-of select="@name"/>
        /**/public static <xsl:value-of select="@returnType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>(<xsl:for-each select="Parameter"><xsl:if test="position() > 1">, </xsl:if><xsl:if test="@kind='out'"> out </xsl:if><xsl:value-of select="@dataType"/><xsl:text> </xsl:text> <xsl:value-of select="@name"/> </xsl:for-each>)
        {
                                                    <xsl:choose>
                                                        <xsl:when test="text()">
/**/<xsl:value-of select="text()"/>
                                                        </xsl:when>
                                                        <xsl:otherwise>
             throw new Exception("Unimplemented interface method: <xsl:value-of select="@name"/>");                                                            
                                                        </xsl:otherwise>
                                                    </xsl:choose>
        }
                                                </xsl:template>

    <!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////// STATIC_HELPER_FUNCTION (context: Method) -->
<!--                                                <xsl:template name="STATIC_HELPER_FUNCTION">
        /**/
        // Private helper function: <xsl:value-of select="@name"/>  (this is a bit of a hack because this is really only a local helper function -\- should be fixed)
        /**/public static <xsl:value-of select="@returnType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>(<xsl:for-each select="Parameter"><xsl:if test="position() > 1">, </xsl:if><xsl:if test="@kind='out'"> out </xsl:if><xsl:value-of select="@dataType"/><xsl:text> </xsl:text> <xsl:value-of select="@name"/> </xsl:for-each>)
        {
        <xsl:value-of select="text()"/>
        }
                                                </xsl:template>-->
    
    <!-- ///////////////////////////////////////////////////////////////////////////////////////////////////////////// METHODS (context: Class) -->
                                                <xsl:template name="METHODS">
                                                    <xsl:for-each select="Operations/Operation">                                                       
        /**/
        /**/public static <xsl:value-of select="@returnType"/><xsl:text> </xsl:text><xsl:value-of select="@name"/>(<xsl:for-each select="Parameter"><xsl:if test="position() > 1">, </xsl:if><xsl:if test="@kind='out'"> out </xsl:if><xsl:value-of select="@dataType"/><xsl:text> </xsl:text> <xsl:value-of select="@name"/> </xsl:for-each>)
        {
        <xsl:value-of select="text()"/>
        }
                                                    </xsl:for-each>
                                                </xsl:template>
    
    
</xsl:stylesheet>















