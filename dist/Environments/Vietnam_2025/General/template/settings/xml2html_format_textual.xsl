<?xml version='1.0'?>
<!--***************************************************************
    * XSL for generating html from textual template.              *
	***************************************************************-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:strip-space elements="*"/>

<xsl:template match="template[@type='textual']" priority="2">
	<xsl:apply-templates/>
</xsl:template>

<xsl:template match="/template[@type='textual']//text" priority="2">
	<pre>
		<xsl:value-of select="."/>
	</pre>
	<br/>
	<br/>
</xsl:template>

<!-- Ignore other type of nodes for textual template-->
<xsl:template match="/template[@type='textual']//node()" priority="1">
</xsl:template>

</xsl:stylesheet>
