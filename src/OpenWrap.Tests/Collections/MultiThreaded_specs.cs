using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using OpenWrap.Collections;
using OpenWrap.Testing;

namespace OpenWrap.Tests.Collections
{
    [Ignore]
    public class when_enumerating_from_multiple_inputs : OpenWrap.Testing.context
    {
        [Test]
        public void the_outputs_are_returned_in_order()
        {
            var first = new AutoResetEvent(false);
            var second = new AutoResetEvent(false);

            var merged = First(first).Merge(Second(second));

            var enumerator = merged.GetEnumerator() as MultiThreadedEnumerator<int>;

            // on first call, enumeration is triggered on both enumerablesO
            // and the results are queued
            first.Set();

            // trigger a read on first and second

            enumerator.MoveNext().ShouldBeTrue();

            // first responds first
            enumerator.Current.ShouldBe(0);

            // then second
            second.Set();

            // read cached second value
            enumerator.MoveNext();
            enumerator.CachedItems.Count.ShouldBe(0);
            enumerator.Current.ShouldBe(5);

            // trigger a read on first and second, let first respond
            first.Set();
            enumerator.MoveNext().ShouldBeTrue();
            // first has responded
            enumerator.Current.ShouldBe(1);

            //second responds, value is cached
            second.Set();

            // value is read from the cache
            enumerator.MoveNext().ShouldBeTrue();
            enumerator.Current.ShouldBe(6);
            
            // end of both enumerations
            enumerator.MoveNext().ShouldBeFalse();

        }
        [Test]
        public void multiple_reads_in_a_row_get_ordered_result()
        {
            var signal = new AutoResetEvent(false);
            var merged = SecondWait(signal).Merge(FirstWait(signal));
            merged.ShouldBe(new[]{0,1,42,43});
        }
        public IEnumerable<int> First(EventWaitHandle wait)
        {
            wait.WaitOne();
            yield return 0;

            wait.WaitOne();
            yield return 1;
        }
        public IEnumerable<int> Second(EventWaitHandle wait)
        {
            wait.WaitOne();
            yield return 5;

            wait.WaitOne();
            yield return 6;
        }

        public IEnumerable<int> FirstWait(EventWaitHandle finished)
        {
            yield return 0;
            yield return 1;
            finished.Set();
        }
        public IEnumerable<int> SecondWait(EventWaitHandle waitFor)
        {
            waitFor.WaitOne();
            yield return 42;
            yield return 43;
        }
    }
}
