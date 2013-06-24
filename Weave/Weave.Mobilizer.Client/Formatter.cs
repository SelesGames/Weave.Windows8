using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Weave.Mobilizer.Client
{
    public class Formatter
    {
        const string HTML_TEMPLATE_PATH1 = "ms-appx:///Weave.Mobilizer.Client/Templates/html_template1.txt";
        const string HTML_TEMPLATE_PATH2 = "ms-appx:///Weave.Mobilizer.Client/Templates/html_template2.txt";
        const string HTML_TEMPLATE_PATH3 = "ms-appx:///Weave.Mobilizer.Client/Templates/html_template3.txt";
        const string CSS_TEMPLATE_PATH = "ms-appx:///Weave.Mobilizer.Client/Templates/css_template.txt";
        const string BODY_TEMPLATE_PATH = "ms-appx:///Weave.Mobilizer.Client/Templates/body_template.txt";
        const string CSS_IMAGE_HEADER_PATH = "ms-appx:///Weave.Mobilizer.Client/Templates/css_imageHeader.txt";
        const string CSS_TEXT_HEADER_PATH = "ms-appx:///Weave.Mobilizer.Client/Templates/css_textHeader.txt";

        bool areTemplatesLoaded = false;

        string htmlTemplate1;
        string htmlTemplate2;
        string htmlTemplate3;
        string cssTemplate;
        string bodyTemplate;
        string cssImageHeader;
        string cssTextHeader;

        public Encoding Encoding { get; set; }

        public Formatter()
        {
            Encoding = new UTF8Encoding(false, false);
        }

        public async Task<string> CreateHtml(
            string source, 
            string title, 
            string link,
            string body, 
            string foreground, 
            string background, 
            string fontName, 
            string fontSize, 
            string linkColor,
            string imageLink,
            string sourceIconLink
            )
        {
            if (!areTemplatesLoaded)
                await ReadHtmlTemplate();

            if (sourceIconLink != null) sourceIconLink = String.Format("<img id=\"sg_sourceIcon\" alt=\"Source icon\" src=\"{0}\" />", sourceIconLink);
            else sourceIconLink = "";

            var sb = new StringBuilder();
                
            sb
                .AppendLine(htmlTemplate1)

                .AppendLine(
                    new StringBuilder(cssTemplate)
                        .Replace("[FOREGROUND]", foreground)
                        .Replace("[BACKGROUND]", background)
                        .Replace("[FONT]", fontName)
                        .Replace("[FONTSIZE]", fontSize)
                        .Replace("[ACCENT]", linkColor)
                        .ToString())

                .AppendLine(imageLink == null ? cssTextHeader : cssImageHeader.Replace("[IMAGE_URL]", imageLink))

                .AppendLine(htmlTemplate2)

                //.AppendLine(imageLink == null ? "" : String.Format("<img src=\"{0}\" width=\"750px\" style=\"margin: 0\" />", imageLink))

                .AppendLine(
                    new StringBuilder(bodyTemplate)
                        .Replace("[SOURCE]", source)
                        .Replace("[TITLE]", title)
                        .Replace("[LINK]", link)
                        .Replace("[BODY]", body)
                        .Replace("[SOURCE_ICON]", sourceIconLink)
                        .ToString())

                .AppendLine(htmlTemplate3);

            return sb.ToString();
        }

        private async Task ReadHtmlTemplate()
        {
            htmlTemplate1 = await LoadTemplate(HTML_TEMPLATE_PATH1);
            htmlTemplate2 = await LoadTemplate(HTML_TEMPLATE_PATH2);
            htmlTemplate3 = await LoadTemplate(HTML_TEMPLATE_PATH3);
            cssTemplate = await LoadTemplate(CSS_TEMPLATE_PATH);
            bodyTemplate = await LoadTemplate(BODY_TEMPLATE_PATH);
            cssImageHeader = await LoadTemplate(CSS_IMAGE_HEADER_PATH);
            cssTextHeader = await LoadTemplate(CSS_TEXT_HEADER_PATH);
            areTemplatesLoaded = true;
        }

        private async Task<String> LoadTemplate(string templatePath)
        {
            String template = null;
            try
            {
                var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(new Uri(templatePath));// await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync(templatePath);
                using (var stream = await file.OpenStreamForReadAsync())
                using (StreamReader streamReader = new StreamReader(stream, Encoding))
                {
                    template = streamReader.ReadToEnd();
                }
            }
            catch (Exception e)
            {
            }
            return template;
        }
    }
}
