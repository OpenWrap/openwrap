using System;
using System.Linq;
using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Repositories.indexed_http
{
    public class reading_a_wrap_list : context.wrap_list
    {
        public reading_a_wrap_list()
        {
            given_repository(); 
        }

        [Test]
        public void there_are_4_packages()
        {
            Repository.PackagesByName.Count().ShouldBe(4);
        }
    }
}
