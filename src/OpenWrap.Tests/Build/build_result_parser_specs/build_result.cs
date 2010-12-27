using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using OpenWrap.Build;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Build.build_result_parser_specs
{
    public class parsing : OpenWrap.Testing.context
    {
        [TestCase(@"'c:\mordor\file.dll'")]
        [TestCase(@"""c:\mordor\file.dll""")]
        public void file_is_parsed(string fileSpec)
        {
            new DefaultFileBuildResultParser().Parse(@"[built(bin-net35, " + fileSpec + ")]")
                    .First()
                    .Check(x=>x.AllowBinDuplicate.ShouldBeTrue())
                    .Check(x => x.FileName.ShouldBe("file.dll"))
                    .Check(x => x.Path.FullPath.ShouldBe(@"c:\mordor\file.dll"));
        }
        [Test]
        public void duplicate_is_parsed()
        {
            new DefaultFileBuildResultParser().Parse(@"[built(bin-net35, 'file.xml', false)]")
                    .First()
                    .AllowBinDuplicate.ShouldBeFalse();
        }
    }
}
