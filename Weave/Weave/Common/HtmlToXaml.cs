using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace Weave.Common
{
    interface ITextContainer
    {
        void Add(Inline text);
        void Add(Block paragraph);
    }

    public sealed class SpanTextContainer : ITextContainer
    {
        private readonly Span _span;

        public SpanTextContainer(Span span)
        {
            _span = span;
        }

        public void Add(Inline text)
        {
            _span.Inlines.Add(text);
        }

        public void Add(Block paragraph)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class ParagraphTextContainer : ITextContainer
    {
        private readonly Paragraph _block;

        public ParagraphTextContainer(Paragraph block)
        {
            _block = block;
        }

        public void Add(Inline text)
        {
            _block.Inlines.Add(text);
        }

        public void Add(Block paragraph)
        {
            throw new NotSupportedException();
        }
    }

    public sealed class RichTextBlockTextContainer : ITextContainer
    {
        private readonly RichTextBlock _richTextBlock;

        public RichTextBlockTextContainer(RichTextBlock richTextBlock)
        {
            _richTextBlock = richTextBlock;
        }

        public void Add(Inline text)
        {
            //throw new NotSupportedException();
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(text);

            _richTextBlock.Blocks.Add(paragraph);
        }

        public void Add(Block paragraph)
        {
            _richTextBlock.Blocks.Add(paragraph);
        }
    }
}
