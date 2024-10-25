<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform">

  <!-- Include the common stylesheet -->
  <xsl:include href="common.xslt"/>

  <!-- Match the root element -->
  <xsl:template match="/catalog">
    <html>
      <body>
        <h1>Book Catalog</h1>
        <!-- Apply templates to each book -->
        <xsl:apply-templates select="book"/>
      </body>
    </html>
  </xsl:template>

  <!-- Match each book element -->
  <xsl:template match="book">
    <div class="book">
      <!-- Use templates from common.xslt -->
      <xsl:call-template name="formatTitle"/>
      <xsl:call-template name="formatAuthor"/>
      <p>Genre: <xsl:value-of select="genre"/></p>
      <p>Price: $<xsl:value-of select="price"/></p>
    </div>
  </xsl:template>

</xsl:stylesheet>