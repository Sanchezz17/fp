using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NHunspell;
using TagsCloudGenerator.Core.Filters;
using TagsCloudGenerator.Core.Layouters;
using TagsCloudGenerator.Core.Normalizers;
using TagsCloudGenerator.Infrastructure;

namespace TagsCloudGenerator.Core.Translators
{
    public class TextToTagsTranslator
    {
        private readonly IWordsNormalizer wordsNormalizer;
        private readonly Hunspell hunspell;
        private readonly IWordsFilter wordsFilter;
        private readonly IRectangleCloudLayouter layouter;

        public TextToTagsTranslator(IWordsNormalizer wordsNormalizer, Hunspell hunspell,
            IWordsFilter wordsFilter, IRectangleCloudLayouter layouter)
        {
            this.wordsNormalizer = wordsNormalizer;
            this.hunspell = hunspell;
            this.wordsFilter = wordsFilter;
            this.layouter = layouter;
        }

        public IEnumerable<TagInfo> TranslateTextToTags(
            IEnumerable<string> text,
            HashSet<string> boringWords,
            string fontFamily,
            int minFontSize)
        {
            minFontSize = Math.Min(120, minFontSize);
            var normalizedWords = wordsNormalizer.GetNormalizedWords(text, boringWords, hunspell);

            var filteredWords = wordsFilter.GetFilteredWords(normalizedWords);

            var frequencies = GetWordsFrequencies(filteredWords);
            var minFrequency = frequencies.Any() ? frequencies.Min(pair => pair.Value) : 0;

            foreach (var pair in frequencies)
            {
                var fontSize = pair.Value - minFrequency + (int)(minFontSize * 1.15);
                var font = new Font(fontFamily, fontSize);
                var value = pair.Key;
                var rectSize = TextRenderer.MeasureText(value, font);
                var rect = layouter.PutNextRectangle(rectSize);
                yield return new TagInfo(value, font, rect);
            }
        }

        private Dictionary<string, int> GetWordsFrequencies(IEnumerable<string> words)
        {
            var frequencies = new Dictionary<string, int>();
            foreach (var word in words)
            {
                if (!frequencies.ContainsKey(word))
                {
                    frequencies[word] = 0;
                }

                frequencies[word]++;
            }

            return frequencies;
        }
    }
}