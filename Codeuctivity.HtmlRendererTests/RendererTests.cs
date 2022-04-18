﻿using Codeuctivity.HtmlRenderer;
using Codeuctivity.HtmlRendererTests.Infrastructure;
using Codeuctivity.PdfjsSharp;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Codeuctivity.HtmlRendererTests
{
    public class RendererTests
    {
        [Theory]
        [InlineData("BasicTextFormated.html")]
        public async Task ShouldConvertHtmlToPdf(string testFileName)
        {
            var sourceHtmlFilePath = $"../../../TestInput/{testFileName}";
            var actualFilePath = Path.Combine(Path.GetTempPath(), $"ActualConvertHtmlToPdf{testFileName}.pdf");
            var expectReferenceFilePath = $"../../../ExpectedTestOutcome/ExpectedFromHtmlConvertHtmlToPdf{testFileName}.png";

            if (File.Exists(actualFilePath))
            {
                File.Delete(actualFilePath);
            }

            await using (var chromiumRenderer = await Renderer.CreateAsync())
            {
                await chromiumRenderer.ConvertHtmlToPdf(sourceHtmlFilePath, actualFilePath);

                var actualImagePathDirectory = Path.Combine(Path.GetTempPath(), testFileName);

                using var rasterize = new Rasterizer();
                var actualImages = await rasterize.ConvertToPngAsync(actualFilePath, actualImagePathDirectory);

                Assert.Single(actualImages);
                DocumentAsserter.AssertImageIsEqual(actualImages.Single(), expectReferenceFilePath, 300);
            }
            await ChromiumProcessDisposedAsserter.AssertNoChromeProcessIsRunning();
        }

        [Theory]
        [InlineData("BasicTextFormated.html")]
        public async Task ShouldConvertHtmlToPng(string testFileName)
        {
            var sourceHtmlFilePath = $"../../../TestInput/{testFileName}";
            var actualFilePath = Path.Combine(Path.GetTempPath(), $"ActualConvertHtmlToPng{testFileName}.png");
            var expectReferenceFilePath = $"../../../ExpectedTestOutcome/ExpectedConvertHtmlToPng{testFileName}.png";

            if (File.Exists(actualFilePath))
            {
                File.Delete(actualFilePath);
            }

            await using (var chromiumRenderer = await Renderer.CreateAsync())
            {
                await chromiumRenderer.ConvertHtmlToPng(sourceHtmlFilePath, actualFilePath);

                DocumentAsserter.AssertImageIsEqual(actualFilePath, expectReferenceFilePath, 6300);
            }

            await ChromiumProcessDisposedAsserter.AssertNoChromeProcessIsRunning();
        }

        [Fact]
        public async Task ShouldDisposeGracefull()
        {
            var initialChromiumTasks = ChromiumProcessDisposedAsserter.CountChromiumTasks();

            await using (var chromiumRenderer = new Renderer())
            {
                Assert.Null(chromiumRenderer.BrowserFetcher);
            }
            var afterDisposeChromiumTasks = ChromiumProcessDisposedAsserter.CountChromiumTasks();
            Assert.Equal(afterDisposeChromiumTasks, initialChromiumTasks);
        }
    }
}