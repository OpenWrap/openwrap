using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.IO;
using OpenWrap.Testing;

namespace OpenWrap.Tests.IO
{
    public abstract class template_path_segment : context
    {
        public class TemplateResult
        {
            public TemplateResult(bool success, IDictionary<string, string> dic)
            {
                Success = success;
                _dic = dic;
            }

            public bool Success;
            readonly IDictionary<string, string> _dic;

            public TemplateResult ShouldNotHaveName(string name)
            {
                _dic.ContainsKey(name).ShouldBeFalse();
                return this;
            }
            public TemplateResult ShouldHaveName(string name)
            {
                _dic.ContainsKey(name).ShouldBeTrue();
                return this;
            }
            public TemplateResult ShouldHaveValue(string name, string value)
            {
                _dic.ContainsKey(name).ShouldBeTrue();
                _dic[name].ShouldBe(value);

                return this;
            }
        }

        protected TemplateResult Template(string template, string segment)
        {
            var parser = TemplatePathSegment.TryParse(template);
            var dic = new Dictionary<string, string>();
            var currentSeg = new LinkedListNode<string>(segment);
            var success = parser.TryParse(dic, new LinkedListNode<PathSegment>(parser), ref currentSeg);
            return new TemplateResult(success, dic);
        }
    }

    public class template_path_segment_specs : template_path_segment
    {
        [Test]
        public void var_name_is_correct()
        {
            Template("{source}", "source").ShouldHaveName("source");
        }
        [Test]
        public void simple_parameter_is_parsed()
        {
            Template("{source}", "src").ShouldHaveValue("source", "src");
        }
        [Test]
        public void parameter_with_value_is_parsed()
        {
            Template("{source: src}", "src")
                .ShouldHaveName("source")
                .ShouldHaveValue("source", "src");
            Template("{source: src}", "source").ShouldNotHaveName("source");
        }
        [Test]
        public void parameter_with_value_is_replaced_with_transform()
        {
            Template("{source: src=source}", "src").ShouldHaveValue("source", "source");
            Template("{source: src=source}", "source").ShouldNotHaveName("source");
        }
    }
}
