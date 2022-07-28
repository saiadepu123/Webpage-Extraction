using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebpageExtraction.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExtractWebpage : ControllerBase
    {
        List<string> contentStrings = new List<string>();

        [HttpGet]
        [Route("loadurl")]
        public dynamic LoadUrl(string url)
        {
            return ExtractContent(url);
        }

        dynamic ExtractContent(string url)
        {
            var imageLinks = new List<string>();
            var document = new HtmlWeb().Load(url);
            var sentenseList = ExtractText(document);
            var words = new List<string>();
            foreach (var sentense in sentenseList)
            {
                var wds = sentense.Split(' ').Select(wd => wd.Trim('\n','\t', ' ')).Where(w => w != "&amp;" && w!="");
                    words.AddRange(wds);
            }
            dynamic obj = new System.Dynamic.ExpandoObject();
            var ImageURLs = document.DocumentNode.Descendants("img")
                                            .Select(e => e.GetAttributeValue("src", null))
                                            .Where(s => !String.IsNullOrEmpty(s));


            var top10Words = words.GroupBy(word => word)
                         .Select(group => new
                         {
                             Word = group.Key,
                             Count = group.Count()
                         })
                         .OrderByDescending(x => x.Count).Take(10);

            obj.images = ImageURLs;
            obj.wordscount = words.Count;
            obj.top10words = top10Words;
            return obj;
        }
        private List<string> ExtractText(HtmlDocument currNode)
        {
            IEnumerable<HtmlNode> textNodes = currNode.DocumentNode.Descendants().Where(n =>
               n.NodeType == HtmlNodeType.Text &&
               n.ParentNode.Name != "script" &&
               n.ParentNode.Name != "style");
            foreach (var textnode in textNodes)
            {
                if (!textnode.HasChildNodes)
                {
                    var text = textnode.InnerText.Trim(' ', '\r', '\n');
                    if (!string.IsNullOrEmpty(text))
                    {
                        contentStrings.Add(text);
                    }
                }
            }
            return contentStrings;
        }
    }
}
