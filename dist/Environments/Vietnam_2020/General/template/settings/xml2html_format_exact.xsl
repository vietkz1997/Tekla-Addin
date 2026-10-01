<?xml version='1.0'?>
<!--****************************************************************
    * XSL stylesheet for generating html with absolute positioning *
	* from graphical template.                                     *                                                     *
	****************************************************************-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:include href="./encoding_html.xsl"/>
<xsl:include href="./xml2html_format_textual.xsl"/>
<xsl:variable name="POS_UNIT_IN_PIXELS">9</xsl:variable>
<xsl:variable name="FONT_SIZE_IN_PIXELS">6</xsl:variable>
<xsl:variable name="BOTTOM_MARGIN">2</xsl:variable>

<xsl:template match="template_seq">
	<html>
	<head>
	<title>
	<xsl:value-of select="@name"/>
	</title>
	<style type="text/css">
	div
	{
		position: absolute;
		font-family: arial, verdana;
	}
	<!--Line vertical-->
	.lv
	{
		border: 0px solid; 
		border-color:black;
		border-left: 1px solid;
	}
	<!--Line horizontal-->
	.lh
	{
		border: 0px solid; 
		border-color:black;
		border-top: 1px solid;
	}
	</style>
	</head>
	<body margintop="0" marginleft="0" marginwidth="0" marginheight="0">
		<!-- Gludge for older netscapes to show the scrollbar -->
		<xsl:element name="table">
			<xsl:attribute name="height">
				<xsl:value-of select="ceiling(@height div @width * 100 * $POS_UNIT_IN_PIXELS)"/>
				<xsl:text>px</xsl:text>
			</xsl:attribute>
			<xsl:attribute name="border">
				<xsl:text>0</xsl:text>
			</xsl:attribute>
			<tr><td><br/></td></tr>
		</xsl:element>
		<xsl:element name="div">
		<xsl:attribute name="style">
				<xsl:text>position:absolute; top: 0; left: 0; margin: 0px; width: 98%; height:100%;</xsl:text>
			</xsl:attribute>
			<xsl:apply-templates/>
		</xsl:element>
	</body>
	</html>	
</xsl:template>

<xsl:template match="template_ref">
    <xsl:apply-templates select="document(@href)//template"/>
</xsl:template>

<xsl:template match="template[@type='graphical']">
	<xsl:element name="div">
		<xsl:attribute name="style">
				<xsl:text>position:relative; top: 0; left: 0; margin: 0px; width: 98%; height:</xsl:text>
				<xsl:value-of select="floor((number(/template/object_seq[last()]/@y1) + $BOTTOM_MARGIN) * $POS_UNIT_IN_PIXELS)"/>
				<xsl:text>px;</xsl:text>
		</xsl:attribute>
		<xsl:apply-templates/>
	</xsl:element>
</xsl:template>

<xsl:template match="/template[@type='graphical']//text | template[@type='graphical']//valuefield">
	<xsl:element name="div">
		<xsl:attribute name="style">
			<xsl:text>position:absolute; top:</xsl:text>
			<xsl:value-of select="ceiling(number(@y1) * $POS_UNIT_IN_PIXELS)"/>
			<xsl:text>px; left:</xsl:text>
			<xsl:value-of select="ceiling(number(@x1) * $POS_UNIT_IN_PIXELS)"/>
			<xsl:text>px; font-family:</xsl:text>
			<xsl:value-of select="@fontname"/>
			<xsl:text>;color:</xsl:text>
			<xsl:value-of select="@fontcolor"/>
			<xsl:text>; font-size:</xsl:text>
			<xsl:value-of select="floor(number(@fontsize) * $FONT_SIZE_IN_PIXELS)"/>
			<xsl:text>px;</xsl:text>	

			<!-- Select font style type -->
			<xsl:choose>
				<xsl:when test="number(@fontstyle) = 0">
					<xsl:text>font-weight: normal;</xsl:text>
					<xsl:text>font-style: normal;</xsl:text>
				</xsl:when>
				<xsl:when test="number(@fontstyle) = 1">
					<xsl:text>font-weight: bold;</xsl:text>
				</xsl:when>
				<xsl:when test="number(@fontstyle) = 2">
					<xsl:text>font-style: italic;</xsl:text>
				</xsl:when>
			</xsl:choose>
		</xsl:attribute>
		<!--<xsl:value-of select="."/>-->
		<xsl:call-template name="spaces">
			<xsl:with-param name="string" select="."/>
		</xsl:call-template>
	</xsl:element>
</xsl:template>

<xsl:template match="line_horizontal">
	<xsl:element name="div">
		<xsl:attribute name="class">lh</xsl:attribute>
		<xsl:attribute name="style">
			<xsl:text>position:absolute; width:</xsl:text>
			<xsl:value-of select="ceiling((number(@x2) - number(@x1)) * $POS_UNIT_IN_PIXELS)"/>		
			<xsl:text>px; top:</xsl:text>
			<xsl:value-of select="ceiling(number(@y1) * $POS_UNIT_IN_PIXELS)"/>
			<xsl:text>px; left:</xsl:text>
			<xsl:value-of select="ceiling(number(@x1) * $POS_UNIT_IN_PIXELS)"/>
			<!-- Select line type -->
			<xsl:choose>
				<xsl:when test="boolean(number(@linetype) - 2)">
					<xsl:text>px; border-top: solid </xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>px; border-top: dashed </xsl:text>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:value-of select="@linewidth"/>
			<xsl:text>px; border-color:</xsl:text>
			<xsl:value-of select="@linecolor"/>
			<xsl:text>;</xsl:text>
		</xsl:attribute>
	</xsl:element>
</xsl:template>

<xsl:template match="line_vertical">
	<xsl:element name="div">
		<xsl:attribute name="class">lv</xsl:attribute>
		<xsl:attribute name="style">
			<xsl:text>position:absolute; height:</xsl:text>
			<xsl:value-of select="ceiling((number(@y2)-number(@y1)) * $POS_UNIT_IN_PIXELS)"/>		
			<xsl:text>px; top:</xsl:text>
			<xsl:value-of select="ceiling(number(@y1) * $POS_UNIT_IN_PIXELS)"/>
			<xsl:text>px; left:</xsl:text>
			<xsl:value-of select="ceiling(number(@x1) * $POS_UNIT_IN_PIXELS)"/>
			<!-- Select line type -->
			<xsl:choose>
				<xsl:when test="boolean(number(@linetype) - 2)">
					<xsl:text>px; border-left: solid </xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>px; border-left: dashed </xsl:text>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:value-of select="@linewidth"/>
			<xsl:text>px; width: 1px; border-color:</xsl:text>
			<xsl:value-of select="@linecolor"/>						
		</xsl:attribute>
	</xsl:element>
</xsl:template>

<xsl:template match="picture">
	<xsl:element name="div">
		<xsl:attribute name="style">
			<xsl:text>position:absolute; top:</xsl:text>
			<xsl:value-of select="ceiling((number(@y1) - number(@height)) * $POS_UNIT_IN_PIXELS)"/>
			<xsl:text>px; left:</xsl:text>
			<xsl:value-of select="ceiling(number(@x1) * $POS_UNIT_IN_PIXELS)"/>
			<xsl:text>px;</xsl:text>
		</xsl:attribute>
		<xsl:element name="img">
			<xsl:attribute name="src">
				<xsl:text>file://localhost/</xsl:text>
				<xsl:value-of select="normalize-space(text())"/>
			</xsl:attribute>
			<xsl:attribute name="border">0</xsl:attribute>
			<xsl:attribute name="height">
				<xsl:value-of select="ceiling(number(@height) * $POS_UNIT_IN_PIXELS)"/>
			</xsl:attribute>
			<xsl:attribute name="width">
				<xsl:value-of select="ceiling(number(@width) * $POS_UNIT_IN_PIXELS)"/>
			</xsl:attribute>
		</xsl:element>
	</xsl:element>
</xsl:template>

<!-- Template to replace spaces with html &nbsp; -->
<xsl:template name="spaces">
    <xsl:param name="string"/>
    <xsl:choose><xsl:when test="contains($string, '&#32;')"><xsl:value-of select="substring-before($string, '&#32;')"/>
	<xsl:text disable-output-escaping="yes">&amp;nbsp;&amp;nbsp;</xsl:text><xsl:call-template name="spaces">
                <xsl:with-param name="string" select="substring-after($string, '&#32;')"/>
            </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="$string"/>
        </xsl:otherwise>
    </xsl:choose>
</xsl:template>
</xsl:stylesheet>
