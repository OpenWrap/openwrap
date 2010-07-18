using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Collections
{
    public class when_enumerating_from_multiple_inputs : Testing.context
    {
        [Test]
        public void the_outputs_are_returned_in_order()
        {
            var merged = First().Merge(Second());
            var enumerator = merged.GetEnumerator() as MultiThreadedEnumerator<int>;
            // on first call, enumeration is triggered on both enumerables
            // and the results are queued

            enumerator.MoveNext().ShouldBeTrue();

            Thread.Sleep(TimeSpan.FromSeconds(2));

            enumerator.Current.ShouldBe(0);
            enumerator.CachedItems.Count.ShouldBe(1);
            enumerator.CachedItems.Peek().ShouldBe(5);

            enumerator.MoveNext();
            enumerator.CachedItems.Count.ShouldBe(0);
            enumerator.Current.ShouldBe(5);

        }
        [Test]
        public void multiple_reads_in_a_row_get_ordered_result()
        {
            var merged = Second().Merge(new[] { 10, 11, 12 }).ToList();
            merged.ShouldHaveSameElementsAs(new[]{ 10, 11, 12, 5, 6, 7});
        }

        public IEnumerable<int> First()
        {
            yield return 0;
            Thread.Sleep(TimeSpan.FromMilliseconds(50));
            yield return 1;
        }
        public IEnumerable<int> Second()
        {
            Thread.Sleep(TimeSpan.FromSeconds(1));
            yield return 5;
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            yield return 6;
            Thread.Sleep(TimeSpan.FromMilliseconds(100));
            yield return 7;
        }
    }
}
