using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using OpenWrap;
using OpenWrap.Collections;
using OpenWrap.Testing;
using Tests;

namespace OpenWrap.Tests.Collections
{
    public class indexed_dic : context
    {
        [Test]
        public void can_add()
        {
            new IndexedDictionary<string, int>(_ => _.ToString(),(_1,_2)=> { }) { 3 }
                .ShouldContain("3", 3);
        }

        [Test]
        public void can_remove()
        {
            new IndexedDictionary<string, int>(_ => _.ToString(), (_1, _2) => { }) { 3 }
                .Check(x => x.Remove("3").ShouldBeTrue())
                .ShouldBeEmpty();
        }

        [Test]
        public void can_set()
        {
            new IndexedDictionary<string, int>(_ => _.ToString(), (_1, _2) => { }) { 3 }
                .Check(x => x["4"] = 4)
                .ShouldContain("4", 4);
        }

        [Test]
        public void can_get()
        {
            new IndexedDictionary<string, int>(_ => _.ToString(), (_1, _2) => { }) { 3 }
                ["3"].ShouldBe(3);
        }

        [Test]
        public void cannot_get_unknown()
        {
            Executing(() => new IndexedDictionary<string, int>(_ => _.ToString(), (_1, _2) => { }) { 3 }
                                ["4"].ShouldBe(4))
                                .ShouldThrow<ArgumentException>();
        }
        [Test]
        public void contains_key()
        {
            new IndexedDictionary<string, int>(_ => _.ToString(), (_1, _2) => { }) { 3 }
                .ContainsKey("3").ShouldBeTrue();
        }
        [Test]
        public void cannot_add_existing()
        {
            Executing(() => new IndexedDictionary<string, int>(_ => _.ToString(), (_1, _2) => { }) { 3 }.Add(3))
                .ShouldThrow<ArgumentException>();
        }
        
        [Test]
        public void has_values()
        {
            new IndexedDictionary<string, int>(_ => _.ToString(), (_1, _2) => { }) { 3, 4 }
                .Values.ShouldBe(3, 4);
        }

        [Test]
        public void not_readonly()
        {
            new IndexedDictionary<string, int>(_ => _.ToString(), (_1, _2) => { })
                .IsReadOnly.ShouldBeFalse();
        }
        [Test]
        public void has_keys()
        {
            new IndexedDictionary<string, int>(_ => _.ToString(), (_1, _2) => { }) { 3, 4 }
                .Keys.ShouldBe("3", "4");
        }

        [Test]
        public void can_clear()
        {
            new IndexedDictionary<string, int>(_ => _.ToString(), (_1, _2) => { }) { 3, 4 }
                .Check(x => x.Clear())
                .ShouldBeEmpty();
        }

        [Test]
        public void can_try_get_value()
        {
            int val = 0;
            new IndexedDictionary<string, int>(_ => _.ToString(), (_1, _2) => { }) { 3, 4 }
                .Check(_ => _.TryGetValue("3", out val).ShouldBeTrue())
                .Check(_ => val.ShouldBe(3))
                .Check(_ => _.TryGetValue("12", out val).ShouldBeFalse());
        }

        [Test]
        public void can_replace_by_set()
        {
            Func<string> a = () => "3";
            Func<string> b = () => "3";
            new IndexedDictionary<string, Func<string>>(_ => _(), (_1, _2) => { }) { a }
                .Check(_=>_["3"] = b)
                ["3"].ShouldBeSameInstanceAs(b);
        }
    }
}