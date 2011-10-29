using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Repositories.factories.indexed_http
{
    public class from_user_input_with_authentication
        : contexts.indexed_http_repository
    {
        public from_user_input_with_authentication()
        {
            given_remote_resource(
                "http://middle.earth/rohan/index.wraplist",
                "application/vnd.openwrap.index+xml",
                "<package-list><link rel=\"publish\" href=\"publish\" /></package-list>",
                "username", 
                "password");
            when_detecting("http://middle.earth/rohan/", "username", "password");
        }
        [Test]
        public void repository_is_detected()
        {
            Repository.ShouldNotBeNull();
        }
        [Test]
        public void feed_is_detected_as_publishable()
        {
            Repository.Feature<ISupportPublishing>().ShouldNotBeNull();
        }
    }
    public class from_user_input_with_incorrect_authentication
        : contexts.indexed_http_repository
    {
        public from_user_input_with_incorrect_authentication()
        {
            given_remote_resource(
                "http://middle.earth/rohan/index.wraplist",
                "application/vnd.openwrap.index+xml",
                "<package-list><link rel=\"publish\" href=\"publish\" /></package-list>",
                "sauron",
                "password");
            when_detecting("http://middle.earth/rohan/", "username", "password");
        }
        [Test]
        public void feed_is_detected_as_publishable()
        {
            Repository.ShouldBeNull();
        }
    }
}