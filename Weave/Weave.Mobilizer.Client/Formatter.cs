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

        bool areTemplatesLoaded = false;

        string htmlTemplate1;
        string htmlTemplate2;
        string htmlTemplate3;
        string cssTemplate;
        string bodyTemplate;

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
            string imageLink)
        {
            if (!areTemplatesLoaded)
                await ReadHtmlTemplate();

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

                .AppendLine(
                    new StringBuilder(htmlTemplate2)
                    .Replace("[IMAGE_URL]", imageLink == null ? "" : imageLink)
                    .ToString())

                //.AppendLine(imageLink == null ? "" : String.Format("<img src=\"{0}\" width=\"750px\" style=\"margin: 0\" />", imageLink))

                .AppendLine(
                    new StringBuilder(bodyTemplate)
                        .Replace("[SOURCE]", source)
                        .Replace("[TITLE]", title)
                        .Replace("[LINK]", link)
                        .Replace("[BODY]", body)
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
