<?xml version='1.0'?>
<!--*****************************************************************
    * XSL stylesheet for generating html with tables from graphical *
	* template.                                                     *
	*****************************************************************-->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:include href="./encoding_html.xsl"/>
<xsl:include href="./xml2html_format_textual.xsl"/>
<xsl:variable name="POS_UNIT_IN_PIXELS">7</xsl:variable>
<xsl:variable name="FONT_SIZE_IN_PIXELS">6</xsl:variable>
<xsl:strip-space elements="*"/>

<xsl:template match="template_seq">
    <html>
    <head>
    <title>
    <xsl:value-of select="@name"/>
    </title>
    <style type="text/css">
    <!--Line horizontal-->
    .lh
    {
        border: 0px solid; 
        border-color:black;
        border-top: 1px solid;
        font-size:1px;
    }
    td
    {
        padding: 2px;
        vertical-align: top; 
        align: left;
    }
    table
    { 
        border: none;
    }
    </style>
    </head>
    <body margintop="10" marginleft="10" marginwidth="10" marginheight="10">
        <xsl:apply-templates/>
    </body>
    </html>
</xsl:template>

<xsl:template match="template_ref">
    <xsl:apply-templates select="document(@href)//template"/>
</xsl:template>

<xsl:template match="template[@type='graphical']">
	<xsl:apply-templates/><br/><br/>
</xsl:template>

<xsl:template match="object_seq">
	<xsl:if test="position() = 1">
		<xsl:element name="table">
			<xsl:attribute name="height">
				<xsl:value-of select="round(number(@y1) * $POS_UNIT_IN_PIXELS)"/>
			</xsl:attribute>
		</xsl:element>	
	</xsl:if>

	<xsl:element name="table">
		<xsl:if test="position() != last()">
			<xsl:attribute name="height">
				<xsl:value-of select="round((number(following-sibling::*/@y1) - number(@y1)) * $POS_UNIT_IN_PIXELS)"/>
			</xsl:attribute>
		</xsl:if>
		<tr>
			<xsl:apply-templates/>
		</tr>
	</xsl:element>

</xsl:template>

<xsl:template match="text | valuefield | line_horizontal | picture">
		<xsl:choose>
			<!-- First cell in table -->
			<xsl:when test="position() = 1">
				<xsl:element name="td">
					<xsl:attribute name="width">
						<xsl:value-of select="round(number(@x1) * $POS_UNIT_IN_PIXELS)"/>
					</xsl:attribute>
				</xsl:element>
				<xsl:element name="td">
					<xsl:if test="position() != last()">
					<xsl:attribute name="width">
						<xsl:value-of select="round((number(following-sibling::*/@x1) - number(@x1)) * $POS_UNIT_IN_PIXELS)"/>
					</xsl:attribute>
					</xsl:if>
					<xsl:call-template name="select_element"/>
				</xsl:element>
			</xsl:when>
		
			<!-- Last cell in table but not the only cell -->
			<xsl:when test="position() = last() and position() != 1">
				<xsl:element name="td">
					<xsl:call-template name="select_element"/>																		
				</xsl:element>
			</xsl:when>

			<!-- Cell in the middle -->
			<xsl:otherwise>
				<xsl:element name="td">
					<xsl:attribute name="width">
						<xsl:value-of select="round((number(following-sibling::*/@x1) - number(@x1)) * $POS_UNIT_IN_PIXELS)"/>
					</xsl:attribute>
					<xsl:call-template name="select_element"/>
				</xsl:element>
			</xsl:otherwise>
		</xsl:choose>
</xsl:template>

<xsl:template name="select_element">
	<xsl:if test="name() = 'text' or name() = 'valuefield'">
		<xsl:call-template name="formatted_text"></xsl:call-template>
	</xsl:if>
	<xsl:if test="name() = 'line_horizontal'">
		<xsl:call-template name="line_horizontal_with_div"></xsl:call-template>
	</xsl:if>
	<xsl:if test="name() = 'picture'">
		<xsl:call-template name="picture"></xsl:call-template>
	</xsl:if>
</xsl:template>

<xsl:template name="line_horizontal_with_hr">
<xsl:element name="hr">
		<xsl:attribute name="class">lh</xsl:attribute>
		<xsl:attribute name="align">left</xsl:attribute>
		<xsl:attribute name="size">
			<xsl:value-of select="@linewidth"/>
		</xsl:attribute>
		<xsl:attribute name="style">
			<xsl:text>width:</xsl:text>
			<xsl:value-of select="ceiling((number(@x2) - number(@x1)) * $POS_UNIT_IN_PIXELS)"/>
			<!-- Select line type -->
			<xsl:choose>
				<xsl:when test="boolean(number(@linetype) - 2)">
					<xsl:text>px; border-top: solid; height:</xsl:text>
				</xsl:when>
				<xsl:otherwise>
					<xsl:text>px; border-top: dashed; height:</xsl:text>
				</xsl:otherwise>
			</xsl:choose>
			<xsl:value-of select="@linewidth"/>
			<xsl:text>px; border-color:</xsl:text>
			<xsl:value-of select="@linecolor"/>
			<xsl:text>;</xsl:text>
		</xsl:attribute>
</xsl:element>
</xsl:template>

<xsl:template name="line_horizontal_with_div">
	<xsl:element name="div">
		<xsl:attribute name="class">lh</xsl:attribute>
		<xsl:attribute name="style">
			<xsl:text>width:</xsl:text>
			<xsl:value-of select="ceiling((number(@x2) - number(@x1)) * $POS_UNIT_IN_PIXELS)"/>		
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

<xsl:template name="picture">
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
</xsl:template>

<xsl:template name="formatted_text">
	<xsl:element name="div">
		<xsl:attribute name="style">
			<xsl:text>font-family:</xsl:text>
			<xsl:value-of select="@fontname"/>
			<xsl:text>,arial;color:</xsl:text>
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
