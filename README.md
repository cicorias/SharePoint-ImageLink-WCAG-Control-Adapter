SharePoint-ImageLink-WCAG-Control-Adapter
=========================================

Provides a Control Adapter that will append or insert an 'alt=""' tag to comply with the WCAG F38 test that indicates decorate images should have alt tags.

# WCAG - (Web Content Accessibility Guidelines) #
The [WCAG (Web Content Accessibility Guidelines)](http://www.w3.org/WAI/intro/wcag "WCAG") provide a baseline for accessibility standards so various tools, such as screen readers, can provide a reasonable experience for those with accessibility challenges.

With regards to images, the guideline provides that all Image tabs <img…> should probably (I say probably here for various reasons) have an ALT tag.

In the case of filler images, or “decorative” that isn’t representative of content, according to F38 here: They should have an empty ALT tag – thus ‘alt=””’.

[F38: Failure of Success Criterion 1.1.1 due to not marking up decorative images in HTML in a way that allows assistive technology to ignore them](http://www.w3.org/TR/WCAG20-TECHS/F38.html "F38 Rule")

**The above reference specifically states for validation:**

## Procedure ##

> For any img element that is used for purely decorative content:
> 
> 1.Check whether the element has no role attribute or has a role attribute value that is not "presentation".
> 
> 2.Check whether the element has no alt attribute or has an alt attribute with a value that is not null.
> 
> Expected Results
> 
> If step #1 is true and if step #2 is true, this failure condition applies and content fails the Success Criterion.
> 


## How this Applies to SharePoint 2013 ##

In SharePoint 2013, if using Display Templates, the generation of the master page is done by the Design Manager “parts”.

Inside of HTML version of the master pages, you will see the following:

    <body>
        <!--SPM:<SharePoint:ImageLink runat="server"  />-->


This will translate to just using the ImageLink SharePoint Web Control, and will emit the following:

    <div id="imgPrefetch" style="display:none">
    <img src="/_layouts/15/images/favicon.ico?rev=23" />
    <img src="/_layouts/15/images/spcommon.png?rev=23" />
    <img src="/_layouts/15/images/spcommon.png?rev=23" />
    <img src="/_layouts/15/images/siteIcon.png?rev=23" />
    

So, we need to “add” an alt=”” tag to this “block” of HTML.

To do this, we can utilize a ControlAdapter – which is a Web Forms based concept, that allows interception at Render time for the control. In the past, ControlAdapters were used in SharePoint 2007 to provide all the rewriting of HTML Tables to more CSS friendly versions – ultimately at the time to help with the WCAG needs.

## ControlAdapter ##

[ControlAdapter on MSDN](http://msdn.microsoft.com/en-us/library/system.web.ui.adapters.controladapter(v=vs.110).aspx "MSDN ControlAdapter Class")

The main part of the control adapter to do this re-rendering is within the Render statement.  Below are the primary methods that will do this rendering and fixup of the IMG tags:


    namespace ImageLinkControlAdapter.Code
    {
	    public class ImageLinkAdapter : ControlAdapter
	    {
		    protected override void Render(System.Web.UI.HtmlTextWriter writer)
		    {
			    /// First we get the control's planned HTML that is emmitted...
			    using (StringWriter baseStringWriter = new StringWriter())
			    using (HtmlTextWriter baseWriter = new HtmlTextWriter(baseStringWriter))
			    {
				    base.Render(baseWriter);
				    baseWriter.Flush();
				    baseWriter.Close();
				    /// Now we have an HTML element...
				    string baseHtml = baseStringWriter.ToString();
				    /// now fixit up...
				    writer.Write(RebuildImgTag(baseHtml));
			    }
    		}
    
    
		    internal string RebuildImgTag(string existingTagHtml)
		    {
			    var pattern = @"<img\s[^>]*>";
			    var rv = Regex.Replace(existingTagHtml, pattern, this.InsertAlt);
			    
			    return rv;
			    
		    }
		    
		    internal string InsertAlt(Match match)
		    {
			    return this.InsertAlt(match.ToString());
		    }
		    
		    internal string InsertAlt(string existingTag)
		    {
			    if (!existingTag.StartsWith("<img", StringComparison.InvariantCultureIgnoreCase))
				    return existingTag;
			    
			    if (existingTag.Contains("alt=", StringComparison.InvariantCultureIgnoreCase))
				    return existingTag;
			    
			    var insertPoint = existingTag.IndexOf("/>");
			    var rv = existingTag.Insert(insertPoint, "alt=\"\"");
			    return rv;
		    }
		    
		    }
	    
    internal static class StringExtensions
    {
    	public static bool Contains(this string source, string toCheck, StringComparison comp)
    	{
    		return source.IndexOf(toCheck, comp) >= 0;
    	}
    }


When all is done, we get the fixed up HTML as follows:

    <div id="imgPrefetch" style="display:none">
    <img src="/_layouts/15/images/favicon.ico?rev=23" alt="" />
    <img src="/_layouts/15/images/spcommon.png?rev=23" alt="" />
    <img src="/_layouts/15/images/spcommon.png?rev=23" alt="" />
    <img src="/_layouts/15/images/siteIcon.png?rev=23" alt="" />


Finally, the full Visual Studio 2013 Solution and source is located here: 

[https://github.com/cicorias/SharePoint-ImageLink-WCAG-Control-Adapter](https://github.com/cicorias/SharePoint-ImageLink-WCAG-Control-Adapter)

As a bonus, there’s a Feature Receiver that will deploy the *.browser file to the Web Application’s App_Browsers directory as well.


## Links ##

[http://www.w3.org/WAI/intro/wcag](http://www.w3.org/WAI/intro/wcag)

[https://github.com/cicorias/SharePoint-ImageLink-WCAG-Control](https://github.com/cicorias/SharePoint-ImageLink-WCAG-Control)
