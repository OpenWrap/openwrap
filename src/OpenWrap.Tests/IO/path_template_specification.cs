using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.IO;
using OpenWrap.Testing;

namespace OpenWrap.Tests.IO
{
    public class path_template_specification : context
    {
        [Test]
        public void var_name_is_correct()
        {
            Template("{source}", "source").Name.ShouldBe("source");
        }
        [Test]
        public void simple_parameter_is_parsed()
        {
            Template("{source}", "src").Value.ShouldBe("src");
        }
        [Test]
        public void parameter_with_value_is_parsed()
        {
            Template("{source: src}", "src").Value.ShouldBe("src");
            Template("{source: src}", "source").Value.ShouldBe(null);
        }
        [Test]
        public void parameter_with_value_is_replaced_with_transform()
        {
            Template("{source: src=source}", "src").Value.ShouldBe("source");
            Template("{source: src=source}", "source").Value.ShouldBe(null);
        }
        class TemplateResult
        {
            public TemplateResult(bool success, IDictionary<string,string> dic)
            {
                var key = dic.FirstOrDefault();
                
                Name = key.Key;
                Value = key.Value;
                Success = success;
            }
            public string Name;
            public string Value;
            public bool Success;
        }
        TemplateResult Template(string template, string segment)
        {
            var parser = TemplatePathSegment.TryParse(template);
            var dic = new Dictionary<string, string>();
            var currentSeg = new LinkedListNode<string>(segment);
            var success = parser.TryParse(dic, new LinkedListNode<PathSegment>(parser), ref currentSeg);
            return new TemplateResult(success, dic);
        }
    }
    public class path_template_processor_specification : context
    {
        [Test]
        public void files_are_found()
        {
            
        }
    }
}
