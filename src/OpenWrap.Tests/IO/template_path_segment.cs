using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenWrap.IO;
using OpenWrap.Testing;

namespace Testing.contexts
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
}
